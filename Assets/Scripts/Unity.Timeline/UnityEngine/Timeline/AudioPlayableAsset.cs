using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class AudioPlayableAsset : PlayableAsset, ITimelineClipAsset
	{
		[SerializeField]
		private AudioClip m_Clip;

		[SerializeField]
		private bool m_Loop;

		[SerializeField]
		[HideInInspector]
		private float m_bufferingTime = 0.1f;

		[SerializeField]
		private AudioClipProperties m_ClipProperties = new AudioClipProperties();

		internal float bufferingTime
		{
			get
			{
				return m_bufferingTime;
			}
			set
			{
				m_bufferingTime = value;
			}
		}

		public AudioClip clip
		{
			get
			{
				return m_Clip;
			}
			set
			{
				m_Clip = value;
			}
		}

		public bool loop
		{
			get
			{
				return m_Loop;
			}
			set
			{
				m_Loop = value;
			}
		}

		public override double duration
		{
			get
			{
				if (m_Clip == null)
				{
					return base.duration;
				}
				return (double)m_Clip.samples / (double)m_Clip.frequency;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				yield return AudioPlayableBinding.Create(base.name, this);
			}
		}

		public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Blending | (m_Loop ? ClipCaps.Looping : ClipCaps.None);

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			if (m_Clip == null)
			{
				return Playable.Null;
			}
			AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(graph, m_Clip, m_Loop);
			audioClipPlayable.GetHandle().SetScriptInstance(m_ClipProperties.Clone());
			return audioClipPlayable;
		}
	}
}
