using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	[TrackClipType(typeof(ActivationPlayableAsset))]
	[TrackBindingType(typeof(GameObject))]
	public class ActivationTrack : TrackAsset
	{
		public enum PostPlaybackState
		{
			Active = 0,
			Inactive = 1,
			Revert = 2,
			LeaveAsIs = 3
		}

		[SerializeField]
		private PostPlaybackState m_PostPlaybackState = PostPlaybackState.LeaveAsIs;

		private ActivationMixerPlayable m_ActivationMixer;

		public PostPlaybackState postPlaybackState
		{
			get
			{
				return m_PostPlaybackState;
			}
			set
			{
				m_PostPlaybackState = value;
				UpdateTrackMode();
			}
		}

		internal override bool CanCompileClips()
		{
			if (base.hasClips)
			{
				return base.CanCompileClips();
			}
			return true;
		}

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			ScriptPlayable<ActivationMixerPlayable> scriptPlayable = ActivationMixerPlayable.Create(graph, inputCount);
			m_ActivationMixer = scriptPlayable.GetBehaviour();
			UpdateTrackMode();
			return scriptPlayable;
		}

		internal void UpdateTrackMode()
		{
			if (m_ActivationMixer != null)
			{
				m_ActivationMixer.postPlaybackState = m_PostPlaybackState;
			}
		}

		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			GameObject gameObjectBinding = GetGameObjectBinding(director);
			if (gameObjectBinding != null)
			{
				driver.AddFromName(gameObjectBinding, "m_IsActive");
			}
		}

		protected override void OnCreateClip(TimelineClip clip)
		{
			clip.displayName = "Active";
			base.OnCreateClip(clip);
		}
	}
}
