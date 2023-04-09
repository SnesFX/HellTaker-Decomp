using System;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal class ScheduleRuntimeClip : RuntimeClipBase
	{
		private TimelineClip m_Clip;

		private Playable m_Playable;

		private Playable m_ParentMixer;

		private double m_StartDelay;

		private double m_FinishTail;

		private bool m_Started;

		public override double start => Math.Max(0.0, m_Clip.start - m_StartDelay);

		public override double duration => m_Clip.duration + m_FinishTail + m_Clip.start - start;

		public TimelineClip clip => m_Clip;

		public Playable mixer => m_ParentMixer;

		public Playable playable => m_Playable;

		public override bool enable
		{
			set
			{
				if (value && m_Playable.GetPlayState() != PlayState.Playing)
				{
					m_Playable.Play();
				}
				else if (!value && m_Playable.GetPlayState() != 0)
				{
					m_Playable.Pause();
					if (m_ParentMixer.IsValid())
					{
						m_ParentMixer.SetInputWeight(m_Playable, 0f);
					}
				}
				m_Started &= value;
			}
		}

		public void SetTime(double time)
		{
			m_Playable.SetTime(time);
		}

		public ScheduleRuntimeClip(TimelineClip clip, Playable clipPlayable, Playable parentMixer, double startDelay = 0.2, double finishTail = 0.1)
		{
			Create(clip, clipPlayable, parentMixer, startDelay, finishTail);
		}

		private void Create(TimelineClip clip, Playable clipPlayable, Playable parentMixer, double startDelay, double finishTail)
		{
			m_Clip = clip;
			m_Playable = clipPlayable;
			m_ParentMixer = parentMixer;
			m_StartDelay = startDelay;
			m_FinishTail = finishTail;
			clipPlayable.Pause();
		}

		public override void EvaluateAt(double localTime, FrameData frameData)
		{
			if (frameData.timeHeld)
			{
				enable = false;
				return;
			}
			bool flag = frameData.seekOccurred || frameData.timeLooped || frameData.evaluationType == FrameData.EvaluationType.Evaluate;
			if (localTime > start + duration - m_FinishTail)
			{
				return;
			}
			float weight = clip.EvaluateMixIn(localTime) * clip.EvaluateMixOut(localTime);
			if (mixer.IsValid())
			{
				mixer.SetInputWeight(playable, weight);
			}
			if (!m_Started || flag)
			{
				double startTime = clip.ToLocalTime(Math.Max(localTime, clip.start));
				double startDelay = Math.Max(clip.start - localTime, 0.0) * clip.timeScale;
				double num = m_Clip.duration * clip.timeScale;
				if (m_Playable.IsPlayableOfType<AudioClipPlayable>())
				{
					((AudioClipPlayable)m_Playable).Seek(startTime, startDelay, num);
				}
				m_Started = true;
			}
		}
	}
}
