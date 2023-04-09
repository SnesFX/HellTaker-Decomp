using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	[NotKeyable]
	public class AnimationPlayableAsset : PlayableAsset, ITimelineClipAsset, IPropertyPreview, ISerializationCallbackReceiver
	{
		public enum LoopMode
		{
			[Tooltip("Use the loop time setting from the source AnimationClip.")]
			UseSourceAsset = 0,
			[Tooltip("The source AnimationClip loops during playback.")]
			On = 1,
			[Tooltip("The source AnimationClip does not loop during playback.")]
			Off = 2
		}

		private enum Versions
		{
			Initial = 0,
			RotationAsEuler = 1
		}

		private static class AnimationPlayableAssetUpgrade
		{
			public static void ConvertRotationToEuler(AnimationPlayableAsset asset)
			{
				asset.m_EulerAngles = asset.m_Rotation.eulerAngles;
			}
		}

		[SerializeField]
		private AnimationClip m_Clip;

		[SerializeField]
		private Vector3 m_Position = Vector3.zero;

		[SerializeField]
		private Vector3 m_EulerAngles = Vector3.zero;

		[SerializeField]
		private bool m_UseTrackMatchFields = true;

		[SerializeField]
		private MatchTargetFields m_MatchTargetFields = MatchTargetFieldConstants.All;

		[SerializeField]
		private bool m_RemoveStartOffset = true;

		[SerializeField]
		private bool m_ApplyFootIK = true;

		[SerializeField]
		private LoopMode m_Loop;

		private static readonly int k_LatestVersion = 1;

		[SerializeField]
		[HideInInspector]
		private int m_Version;

		[SerializeField]
		[Obsolete("Use m_RotationEuler Instead", false)]
		[HideInInspector]
		private Quaternion m_Rotation = Quaternion.identity;

		public Vector3 position
		{
			get
			{
				return m_Position;
			}
			set
			{
				m_Position = value;
			}
		}

		public Quaternion rotation
		{
			get
			{
				return Quaternion.Euler(m_EulerAngles);
			}
			set
			{
				m_EulerAngles = value.eulerAngles;
			}
		}

		public Vector3 eulerAngles
		{
			get
			{
				return m_EulerAngles;
			}
			set
			{
				m_EulerAngles = value;
			}
		}

		public bool useTrackMatchFields
		{
			get
			{
				return m_UseTrackMatchFields;
			}
			set
			{
				m_UseTrackMatchFields = value;
			}
		}

		public MatchTargetFields matchTargetFields
		{
			get
			{
				return m_MatchTargetFields;
			}
			set
			{
				m_MatchTargetFields = value;
			}
		}

		public bool removeStartOffset
		{
			get
			{
				return m_RemoveStartOffset;
			}
			set
			{
				m_RemoveStartOffset = value;
			}
		}

		public bool applyFootIK
		{
			get
			{
				return m_ApplyFootIK;
			}
			set
			{
				m_ApplyFootIK = value;
			}
		}

		public LoopMode loop
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

		internal bool hasRootTransforms
		{
			get
			{
				if (m_Clip != null)
				{
					return HasRootTransforms(m_Clip);
				}
				return false;
			}
		}

		internal AppliedOffsetMode appliedOffsetMode { get; set; }

		public AnimationClip clip
		{
			get
			{
				return m_Clip;
			}
			set
			{
				if (value != null)
				{
					base.name = "AnimationPlayableAsset of " + value.name;
				}
				m_Clip = value;
			}
		}

		public override double duration
		{
			get
			{
				double animationClipLength = TimeUtility.GetAnimationClipLength(clip);
				if (animationClipLength < 1.401298464324817E-45)
				{
					return base.duration;
				}
				return animationClipLength;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				yield return AnimationPlayableBinding.Create(base.name, this);
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				ClipCaps clipCaps = ClipCaps.All;
				if (m_Clip == null || m_Loop == LoopMode.Off || (m_Loop == LoopMode.UseSourceAsset && !m_Clip.isLooping))
				{
					clipCaps &= ~ClipCaps.Looping;
				}
				if (m_Clip == null || m_Clip.empty)
				{
					clipCaps &= ~ClipCaps.ClipIn;
				}
				return clipCaps;
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			return CreatePlayable(graph, m_Clip, position, eulerAngles, removeStartOffset, appliedOffsetMode, applyFootIK, m_Loop);
		}

		internal static Playable CreatePlayable(PlayableGraph graph, AnimationClip clip, Vector3 positionOffset, Vector3 eulerOffset, bool removeStartOffset, AppliedOffsetMode mode, bool applyFootIK, LoopMode loop)
		{
			if (clip == null || clip.legacy)
			{
				return Playable.Null;
			}
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(graph, clip);
			animationClipPlayable.SetRemoveStartOffset(removeStartOffset);
			animationClipPlayable.SetApplyFootIK(applyFootIK);
			animationClipPlayable.SetOverrideLoopTime(loop != LoopMode.UseSourceAsset);
			animationClipPlayable.SetLoopTime(loop == LoopMode.On);
			Playable playable = animationClipPlayable;
			if (ShouldApplyScaleRemove(mode))
			{
				AnimationRemoveScalePlayable animationRemoveScalePlayable = AnimationRemoveScalePlayable.Create(graph, 1);
				graph.Connect(playable, 0, animationRemoveScalePlayable, 0);
				animationRemoveScalePlayable.SetInputWeight(0, 1f);
				playable = animationRemoveScalePlayable;
			}
			if (ShouldApplyOffset(mode, clip))
			{
				AnimationOffsetPlayable animationOffsetPlayable = AnimationOffsetPlayable.Create(graph, positionOffset, Quaternion.Euler(eulerOffset), 1);
				graph.Connect(playable, 0, animationOffsetPlayable, 0);
				animationOffsetPlayable.SetInputWeight(0, 1f);
				playable = animationOffsetPlayable;
			}
			return playable;
		}

		private static bool ShouldApplyOffset(AppliedOffsetMode mode, AnimationClip clip)
		{
			if (mode == AppliedOffsetMode.NoRootTransform || mode == AppliedOffsetMode.SceneOffsetLegacy)
			{
				return false;
			}
			return HasRootTransforms(clip);
		}

		private static bool ShouldApplyScaleRemove(AppliedOffsetMode mode)
		{
			if (mode != AppliedOffsetMode.SceneOffsetLegacyEditor && mode != AppliedOffsetMode.SceneOffsetLegacy)
			{
				return mode == AppliedOffsetMode.TransformOffsetLegacy;
			}
			return true;
		}

		public void ResetOffsets()
		{
			position = Vector3.zero;
			eulerAngles = Vector3.zero;
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			driver.AddFromClip(m_Clip);
		}

		internal static bool HasRootTransforms(AnimationClip clip)
		{
			if (clip == null || clip.empty)
			{
				return false;
			}
			if (!clip.hasRootMotion && !clip.hasGenericRootTransform && !clip.hasMotionCurves)
			{
				return clip.hasRootCurves;
			}
			return true;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Version = k_LatestVersion;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (m_Version < k_LatestVersion)
			{
				OnUpgradeFromVersion(m_Version);
			}
		}

		private void OnUpgradeFromVersion(int oldVersion)
		{
			if (oldVersion < 1)
			{
				AnimationPlayableAssetUpgrade.ConvertRotationToEuler(this);
			}
		}
	}
}
