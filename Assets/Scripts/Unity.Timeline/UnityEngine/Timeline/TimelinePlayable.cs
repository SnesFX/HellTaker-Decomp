using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class TimelinePlayable : PlayableBehaviour
	{
		private IntervalTree<RuntimeElement> m_IntervalTree = new IntervalTree<RuntimeElement>();

		private List<RuntimeElement> m_ActiveClips = new List<RuntimeElement>();

		private List<RuntimeElement> m_CurrentListOfActiveClips;

		private int m_ActiveBit;

		private List<ITimelineEvaluateCallback> m_EvaluateCallbacks = new List<ITimelineEvaluateCallback>();

		private Dictionary<TrackAsset, Playable> m_PlayableCache = new Dictionary<TrackAsset, Playable>();

		internal static bool muteAudioScrubbing = true;

		public static ScriptPlayable<TimelinePlayable> Create(PlayableGraph graph, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
		{
			if (tracks == null)
			{
				throw new ArgumentNullException("Tracks list is null", "tracks");
			}
			if (go == null)
			{
				throw new ArgumentNullException("GameObject parameter is null", "go");
			}
			ScriptPlayable<TimelinePlayable> scriptPlayable = ScriptPlayable<TimelinePlayable>.Create(graph);
			scriptPlayable.SetTraversalMode(PlayableTraversalMode.Passthrough);
			scriptPlayable.GetBehaviour().Compile(graph, scriptPlayable, tracks, go, autoRebalance, createOutputs);
			return scriptPlayable;
		}

		public void Compile(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
		{
			if (tracks == null)
			{
				throw new ArgumentNullException("Tracks list is null", "tracks");
			}
			if (go == null)
			{
				throw new ArgumentNullException("GameObject parameter is null", "go");
			}
			List<TrackAsset> list = new List<TrackAsset>(tracks);
			int capacity = list.Count * 2 + list.Count;
			m_CurrentListOfActiveClips = new List<RuntimeElement>(capacity);
			m_ActiveClips = new List<RuntimeElement>(capacity);
			m_EvaluateCallbacks.Clear();
			m_PlayableCache.Clear();
			CompileTrackList(graph, timelinePlayable, list, go, createOutputs);
		}

		private void CompileTrackList(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool createOutputs)
		{
			foreach (TrackAsset track in tracks)
			{
				if (track.IsCompilable() && !m_PlayableCache.ContainsKey(track))
				{
					track.SortClips();
					CreateTrackPlayable(graph, timelinePlayable, track, go, createOutputs);
				}
			}
		}

		private void CreateTrackOutput(PlayableGraph graph, TrackAsset track, GameObject go, Playable playable, int port)
		{
			if (track.isSubTrack)
			{
				return;
			}
			foreach (PlayableBinding output in track.outputs)
			{
				PlayableOutput playableOutput = output.CreateOutput(graph);
				playableOutput.SetReferenceObject(output.sourceObject);
				playableOutput.SetSourcePlayable(playable);
				playableOutput.SetSourceOutputPort(port);
				playableOutput.SetWeight(1f);
				if (track as AnimationTrack != null)
				{
					EvaluateWeightsForAnimationPlayableOutput(track, (AnimationPlayableOutput)playableOutput);
				}
				if (playableOutput.IsPlayableOutputOfType<AudioPlayableOutput>())
				{
					((AudioPlayableOutput)playableOutput).SetEvaluateOnSeek(!muteAudioScrubbing);
				}
				if (track.timelineAsset.markerTrack == track)
				{
					PlayableDirector component = go.GetComponent<PlayableDirector>();
					playableOutput.SetUserData(component);
					INotificationReceiver[] components = go.GetComponents<INotificationReceiver>();
					foreach (INotificationReceiver receiver in components)
					{
						playableOutput.AddNotificationReceiver(receiver);
					}
				}
			}
		}

		private void EvaluateWeightsForAnimationPlayableOutput(TrackAsset track, AnimationPlayableOutput animOutput)
		{
			m_EvaluateCallbacks.Add(new AnimationOutputWeightProcessor(animOutput));
		}

		private static Playable CreatePlayableGraph(PlayableGraph graph, TrackAsset asset, GameObject go, IntervalTree<RuntimeElement> tree, Playable timelinePlayable)
		{
			return asset.CreatePlayableGraph(graph, go, tree, timelinePlayable);
		}

		private Playable CreateTrackPlayable(PlayableGraph graph, Playable timelinePlayable, TrackAsset track, GameObject go, bool createOutputs)
		{
			if (!track.IsCompilable())
			{
				return timelinePlayable;
			}
			if (m_PlayableCache.TryGetValue(track, out var value))
			{
				return value;
			}
			if (track.name == "root")
			{
				return timelinePlayable;
			}
			TrackAsset trackAsset = track.parent as TrackAsset;
			Playable playable = ((trackAsset != null) ? CreateTrackPlayable(graph, timelinePlayable, trackAsset, go, createOutputs) : timelinePlayable);
			Playable playable2 = CreatePlayableGraph(graph, track, go, m_IntervalTree, timelinePlayable);
			bool flag = false;
			if (!playable2.IsValid())
			{
				throw new InvalidOperationException(string.Concat(track.name, "(", track.GetType(), ") did not produce a valid playable. Use the compilable property to indicate whether the track is valid for processing"));
			}
			if (playable.IsValid() && playable2.IsValid())
			{
				int inputCount = playable.GetInputCount();
				playable.SetInputCount(inputCount + 1);
				flag = graph.Connect(playable2, 0, playable, inputCount);
				playable.SetInputWeight(inputCount, 1f);
			}
			if (createOutputs && flag)
			{
				CreateTrackOutput(graph, track, go, playable, playable.GetInputCount() - 1);
			}
			CacheTrack(track, playable2, flag ? (playable.GetInputCount() - 1) : (-1), playable);
			return playable2;
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			Evaluate(playable, info);
		}

		private void Evaluate(Playable playable, FrameData frameData)
		{
			if (m_IntervalTree == null)
			{
				return;
			}
			double time = playable.GetTime();
			m_ActiveBit = ((m_ActiveBit == 0) ? 1 : 0);
			m_CurrentListOfActiveClips.Clear();
			m_IntervalTree.IntersectsWith(DiscreteTime.GetNearestTick(time), m_CurrentListOfActiveClips);
			foreach (RuntimeElement currentListOfActiveClip in m_CurrentListOfActiveClips)
			{
				currentListOfActiveClip.intervalBit = m_ActiveBit;
			}
			for (int i = 0; i < m_ActiveClips.Count; i++)
			{
				RuntimeElement runtimeElement = m_ActiveClips[i];
				if (runtimeElement.intervalBit != m_ActiveBit)
				{
					runtimeElement.EvaluateAt(time, frameData);
					runtimeElement.enable = false;
				}
			}
			m_ActiveClips.Clear();
			for (int j = 0; j < m_CurrentListOfActiveClips.Count; j++)
			{
				m_CurrentListOfActiveClips[j].EvaluateAt(time, frameData);
				m_ActiveClips.Add(m_CurrentListOfActiveClips[j]);
			}
			int count = m_EvaluateCallbacks.Count;
			for (int k = 0; k < count; k++)
			{
				m_EvaluateCallbacks[k].Evaluate();
			}
		}

		private void CacheTrack(TrackAsset track, Playable playable, int port, Playable parent)
		{
			m_PlayableCache[track] = playable;
		}

		private static void ForAOTCompilationOnly()
		{
			new List<IntervalTree<RuntimeElement>.Entry>();
		}
	}
}
