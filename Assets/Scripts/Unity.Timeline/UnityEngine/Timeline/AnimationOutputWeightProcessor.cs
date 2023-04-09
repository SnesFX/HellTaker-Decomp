using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal class AnimationOutputWeightProcessor : ITimelineEvaluateCallback
	{
		private struct WeightInfo
		{
			public Playable mixer;

			public Playable parentMixer;

			public int port;

			public bool modulate;
		}

		private AnimationPlayableOutput m_Output;

		private AnimationMotionXToDeltaPlayable m_MotionXPlayable;

		private AnimationMixerPlayable m_PoseMixer;

		private AnimationLayerMixerPlayable m_LayerMixer;

		private readonly List<WeightInfo> m_Mixers = new List<WeightInfo>();

		public AnimationOutputWeightProcessor(AnimationPlayableOutput output)
		{
			m_Output = output;
			output.SetWeight(0f);
			FindMixers();
		}

		private static Playable FindFirstAnimationPlayable(Playable p)
		{
			Playable playable = p;
			while (playable.IsValid() && playable.GetInputCount() > 0 && !playable.IsPlayableOfType<AnimationLayerMixerPlayable>() && !playable.IsPlayableOfType<AnimationMotionXToDeltaPlayable>() && !playable.IsPlayableOfType<AnimationMixerPlayable>())
			{
				playable = playable.GetInput(0);
			}
			return playable;
		}

		private void FindMixers()
		{
			m_Mixers.Clear();
			m_PoseMixer = AnimationMixerPlayable.Null;
			m_LayerMixer = AnimationLayerMixerPlayable.Null;
			m_MotionXPlayable = AnimationMotionXToDeltaPlayable.Null;
			Playable sourcePlayable = m_Output.GetSourcePlayable();
			int sourceOutputPort = m_Output.GetSourceOutputPort();
			if (!sourcePlayable.IsValid() || sourceOutputPort < 0 || sourceOutputPort >= sourcePlayable.GetInputCount())
			{
				return;
			}
			Playable playable = FindFirstAnimationPlayable(sourcePlayable.GetInput(sourceOutputPort));
			Playable playable2 = playable;
			if (playable2.IsPlayableOfType<AnimationMotionXToDeltaPlayable>())
			{
				m_MotionXPlayable = (AnimationMotionXToDeltaPlayable)playable2;
				playable = m_MotionXPlayable.GetInput(0);
			}
			if (playable.IsValid() && playable.IsPlayableOfType<AnimationMixerPlayable>())
			{
				m_PoseMixer = (AnimationMixerPlayable)playable;
				Playable input = m_PoseMixer.GetInput(0);
				if (input.IsValid() && input.IsPlayableOfType<AnimationLayerMixerPlayable>())
				{
					m_LayerMixer = (AnimationLayerMixerPlayable)input;
				}
			}
			else if (playable.IsValid() && playable.IsPlayableOfType<AnimationLayerMixerPlayable>())
			{
				m_LayerMixer = (AnimationLayerMixerPlayable)playable;
			}
			if (m_LayerMixer.IsValid())
			{
				int inputCount = m_LayerMixer.GetInputCount();
				for (int i = 0; i < inputCount; i++)
				{
					FindMixers(m_LayerMixer, i, m_LayerMixer.GetInput(i));
				}
			}
		}

		private void FindMixers(Playable parent, int port, Playable node)
		{
			if (!node.IsValid())
			{
				return;
			}
			Type playableType = node.GetPlayableType();
			if (playableType == typeof(AnimationMixerPlayable) || playableType == typeof(AnimationLayerMixerPlayable))
			{
				int inputCount = node.GetInputCount();
				for (int i = 0; i < inputCount; i++)
				{
					FindMixers(node, i, node.GetInput(i));
				}
				WeightInfo weightInfo = default(WeightInfo);
				weightInfo.parentMixer = parent;
				weightInfo.mixer = node;
				weightInfo.port = port;
				weightInfo.modulate = playableType == typeof(AnimationLayerMixerPlayable);
				WeightInfo item = weightInfo;
				m_Mixers.Add(item);
			}
			else
			{
				int inputCount2 = node.GetInputCount();
				for (int j = 0; j < inputCount2; j++)
				{
					FindMixers(parent, port, node.GetInput(j));
				}
			}
		}

		public void Evaluate()
		{
			m_Output.SetWeight(1f);
			for (int i = 0; i < m_Mixers.Count; i++)
			{
				WeightInfo weightInfo = m_Mixers[i];
				float num = (weightInfo.modulate ? weightInfo.parentMixer.GetInputWeight(weightInfo.port) : 1f);
				weightInfo.parentMixer.SetInputWeight(weightInfo.port, num * WeightUtility.NormalizeMixer(weightInfo.mixer));
			}
			float num2 = WeightUtility.NormalizeMixer(m_LayerMixer);
			if (m_Output.GetTarget() == null)
			{
				return;
			}
			if (!Application.isPlaying && m_MotionXPlayable.IsValid() && m_MotionXPlayable.IsAbsoluteMotion())
			{
				m_PoseMixer.SetInputWeight(0, num2);
				m_PoseMixer.SetInputWeight(1, 1f - num2);
				return;
			}
			if (!m_PoseMixer.Equals(AnimationMixerPlayable.Null))
			{
				m_PoseMixer.SetInputWeight(0, 1f);
				m_PoseMixer.SetInputWeight(1, 0f);
			}
			m_Output.SetWeight(num2);
		}
	}
}
