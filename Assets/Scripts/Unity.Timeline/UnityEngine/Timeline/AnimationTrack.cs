using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityEngine.Timeline
{
	[Serializable]
	[TrackClipType(typeof(AnimationPlayableAsset), false)]
	[TrackBindingType(typeof(Animator))]
	[SupportsChildTracks(typeof(AnimationTrack), 1)]
	public class AnimationTrack : TrackAsset
	{
		private static class AnimationTrackUpgrade
		{
			public static void ConvertRotationsToEuler(AnimationTrack track)
			{
				track.m_EulerAngles = track.m_Rotation.eulerAngles;
				track.m_InfiniteClipOffsetEulerAngles = track.m_OpenClipOffsetRotation.eulerAngles;
			}

			public static void ConvertRootMotion(AnimationTrack track)
			{
				track.m_TrackOffset = TrackOffset.Auto;
				if (!track.m_ApplyOffsets)
				{
					track.m_Position = Vector3.zero;
					track.m_EulerAngles = Vector3.zero;
				}
			}

			public static void ConvertInfiniteTrack(AnimationTrack track)
			{
				track.m_InfiniteClip = track.m_AnimClip;
				track.m_AnimClip = null;
			}
		}

		private const string k_DefaultInfiniteClipName = "Recorded";

		private const string k_DefaultRecordableClipName = "Recorded";

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipPreExtrapolation")]
		private TimelineClip.ClipExtrapolation m_InfiniteClipPreExtrapolation;

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipPostExtrapolation")]
		private TimelineClip.ClipExtrapolation m_InfiniteClipPostExtrapolation;

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipOffsetPosition")]
		private Vector3 m_InfiniteClipOffsetPosition = Vector3.zero;

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipOffsetEulerAngles")]
		private Vector3 m_InfiniteClipOffsetEulerAngles = Vector3.zero;

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipTimeOffset")]
		private double m_InfiniteClipTimeOffset;

		[SerializeField]
		[FormerlySerializedAs("m_OpenClipRemoveOffset")]
		private bool m_InfiniteClipRemoveOffset;

		[SerializeField]
		private bool m_InfiniteClipApplyFootIK = true;

		[SerializeField]
		[HideInInspector]
		private AnimationPlayableAsset.LoopMode mInfiniteClipLoop;

		[SerializeField]
		private MatchTargetFields m_MatchTargetFields = MatchTargetFieldConstants.All;

		[SerializeField]
		private Vector3 m_Position = Vector3.zero;

		[SerializeField]
		private Vector3 m_EulerAngles = Vector3.zero;

		[SerializeField]
		private AvatarMask m_AvatarMask;

		[SerializeField]
		private bool m_ApplyAvatarMask = true;

		[SerializeField]
		private TrackOffset m_TrackOffset;

		[SerializeField]
		[HideInInspector]
		private AnimationClip m_InfiniteClip;

		[SerializeField]
		[Obsolete("Use m_InfiniteClipOffsetEulerAngles Instead", false)]
		[HideInInspector]
		private Quaternion m_OpenClipOffsetRotation = Quaternion.identity;

		[SerializeField]
		[Obsolete("Use m_RotationEuler Instead", false)]
		[HideInInspector]
		private Quaternion m_Rotation = Quaternion.identity;

		[SerializeField]
		[Obsolete("Use m_RootTransformOffsetMode", false)]
		[HideInInspector]
		private bool m_ApplyOffsets;

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

		[Obsolete("applyOffset is deprecated. Use trackOffset instead", true)]
		public bool applyOffsets
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public TrackOffset trackOffset
		{
			get
			{
				return m_TrackOffset;
			}
			set
			{
				m_TrackOffset = value;
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
				m_MatchTargetFields = value & MatchTargetFieldConstants.All;
			}
		}

		public AnimationClip infiniteClip
		{
			get
			{
				return m_InfiniteClip;
			}
			internal set
			{
				m_InfiniteClip = value;
			}
		}

		internal bool infiniteClipRemoveOffset
		{
			get
			{
				return m_InfiniteClipRemoveOffset;
			}
			set
			{
				m_InfiniteClipRemoveOffset = value;
			}
		}

		public AvatarMask avatarMask
		{
			get
			{
				return m_AvatarMask;
			}
			set
			{
				m_AvatarMask = value;
			}
		}

		public bool applyAvatarMask
		{
			get
			{
				return m_ApplyAvatarMask;
			}
			set
			{
				m_ApplyAvatarMask = value;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				yield return AnimationPlayableBinding.Create(base.name, this);
			}
		}

		public bool inClipMode
		{
			get
			{
				if (base.clips != null)
				{
					return base.clips.Length != 0;
				}
				return false;
			}
		}

		public Vector3 infiniteClipOffsetPosition
		{
			get
			{
				return m_InfiniteClipOffsetPosition;
			}
			set
			{
				m_InfiniteClipOffsetPosition = value;
			}
		}

		public Quaternion infiniteClipOffsetRotation
		{
			get
			{
				return Quaternion.Euler(m_InfiniteClipOffsetEulerAngles);
			}
			set
			{
				m_InfiniteClipOffsetEulerAngles = value.eulerAngles;
			}
		}

		public Vector3 infiniteClipOffsetEulerAngles
		{
			get
			{
				return m_InfiniteClipOffsetEulerAngles;
			}
			set
			{
				m_InfiniteClipOffsetEulerAngles = value;
			}
		}

		internal bool infiniteClipApplyFootIK
		{
			get
			{
				return m_InfiniteClipApplyFootIK;
			}
			set
			{
				m_InfiniteClipApplyFootIK = value;
			}
		}

		internal double infiniteClipTimeOffset
		{
			get
			{
				return m_InfiniteClipTimeOffset;
			}
			set
			{
				m_InfiniteClipTimeOffset = value;
			}
		}

		public TimelineClip.ClipExtrapolation infiniteClipPreExtrapolation
		{
			get
			{
				return m_InfiniteClipPreExtrapolation;
			}
			set
			{
				m_InfiniteClipPreExtrapolation = value;
			}
		}

		public TimelineClip.ClipExtrapolation infiniteClipPostExtrapolation
		{
			get
			{
				return m_InfiniteClipPostExtrapolation;
			}
			set
			{
				m_InfiniteClipPostExtrapolation = value;
			}
		}

		internal AnimationPlayableAsset.LoopMode infiniteClipLoop
		{
			get
			{
				return mInfiniteClipLoop;
			}
			set
			{
				mInfiniteClipLoop = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("openClipOffsetPosition has been deprecated. Use infiniteClipOffsetPosition instead. (UnityUpgradable) -> infiniteClipOffsetPosition", true)]
		public Vector3 openClipOffsetPosition
		{
			get
			{
				return infiniteClipOffsetPosition;
			}
			set
			{
				infiniteClipOffsetPosition = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("openClipOffsetRotation has been deprecated. Use infiniteClipOffsetRotation instead. (UnityUpgradable) -> infiniteClipOffsetRotation", true)]
		public Quaternion openClipOffsetRotation
		{
			get
			{
				return infiniteClipOffsetRotation;
			}
			set
			{
				infiniteClipOffsetRotation = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("openClipOffsetEulerAngles has been deprecated. Use infiniteClipOffsetEulerAngles instead. (UnityUpgradable) -> infiniteClipOffsetEulerAngles", true)]
		public Vector3 openClipOffsetEulerAngles
		{
			get
			{
				return infiniteClipOffsetEulerAngles;
			}
			set
			{
				infiniteClipOffsetEulerAngles = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("openClipPreExtrapolation has been deprecated. Use infiniteClipPreExtrapolation instead. (UnityUpgradable) -> infiniteClipPreExtrapolation", true)]
		public TimelineClip.ClipExtrapolation openClipPreExtrapolation
		{
			get
			{
				return infiniteClipPreExtrapolation;
			}
			set
			{
				infiniteClipPreExtrapolation = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("openClipPostExtrapolation has been deprecated. Use infiniteClipPostExtrapolation instead. (UnityUpgradable) -> infiniteClipPostExtrapolation", true)]
		public TimelineClip.ClipExtrapolation openClipPostExtrapolation
		{
			get
			{
				return infiniteClipPostExtrapolation;
			}
			set
			{
				infiniteClipPostExtrapolation = value;
			}
		}

		internal override bool CanCompileClips()
		{
			if (!base.muted)
			{
				if (m_Clips.Count <= 0)
				{
					if (m_InfiniteClip != null)
					{
						return !m_InfiniteClip.empty;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		[ContextMenu("Reset Offsets")]
		private void ResetOffsets()
		{
			m_Position = Vector3.zero;
			m_EulerAngles = Vector3.zero;
			UpdateClipOffsets();
		}

		public TimelineClip CreateClip(AnimationClip clip)
		{
			if (clip == null)
			{
				return null;
			}
			TimelineClip timelineClip = CreateClip<AnimationPlayableAsset>();
			AssignAnimationClip(timelineClip, clip);
			return timelineClip;
		}

		public void CreateInfiniteClip(string infiniteClipName)
		{
			if (inClipMode)
			{
				Debug.LogWarning("CreateInfiniteClip cannot create an infinite clip for an AnimationTrack that contains one or more Timeline Clips.");
			}
			else if (!(m_InfiniteClip != null))
			{
				m_InfiniteClip = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(infiniteClipName) ? "Recorded" : infiniteClipName, this, isLegacy: false);
			}
		}

		public TimelineClip CreateRecordableClip(string animClipName)
		{
			AnimationClip clip = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(animClipName) ? "Recorded" : animClipName, this, isLegacy: false);
			TimelineClip timelineClip = CreateClip(clip);
			timelineClip.displayName = animClipName;
			timelineClip.recordable = true;
			timelineClip.start = 0.0;
			timelineClip.duration = 1.0;
			AnimationPlayableAsset animationPlayableAsset = timelineClip.asset as AnimationPlayableAsset;
			if (animationPlayableAsset != null)
			{
				animationPlayableAsset.removeStartOffset = false;
			}
			return timelineClip;
		}

		protected override void OnCreateClip(TimelineClip clip)
		{
			TimelineClip.ClipExtrapolation clipExtrapolation = TimelineClip.ClipExtrapolation.None;
			if (!base.isSubTrack)
			{
				clipExtrapolation = TimelineClip.ClipExtrapolation.Hold;
			}
			clip.preExtrapolationMode = clipExtrapolation;
			clip.postExtrapolationMode = clipExtrapolation;
		}

		protected internal override int CalculateItemsHash()
		{
			return TrackAsset.GetAnimationClipHash(m_InfiniteClip).CombineHash(base.CalculateItemsHash());
		}

		internal void UpdateClipOffsets()
		{
		}

		private Playable CompileTrackPlayable(PlayableGraph graph, TrackAsset track, GameObject go, IntervalTree<RuntimeElement> tree, AppliedOffsetMode mode)
		{
			AnimationMixerPlayable animationMixerPlayable = AnimationMixerPlayable.Create(graph, track.clips.Length);
			for (int i = 0; i < track.clips.Length; i++)
			{
				TimelineClip timelineClip = track.clips[i];
				PlayableAsset playableAsset = timelineClip.asset as PlayableAsset;
				if (!(playableAsset == null))
				{
					AnimationPlayableAsset animationPlayableAsset = playableAsset as AnimationPlayableAsset;
					if (animationPlayableAsset != null)
					{
						animationPlayableAsset.appliedOffsetMode = mode;
					}
					Playable playable = playableAsset.CreatePlayable(graph, go);
					if (playable.IsValid())
					{
						RuntimeClip item = new RuntimeClip(timelineClip, playable, animationMixerPlayable);
						tree.Add(item);
						graph.Connect(playable, 0, animationMixerPlayable, i);
						animationMixerPlayable.SetInputWeight(i, 0f);
					}
				}
			}
			return ApplyTrackOffset(graph, animationMixerPlayable, go, mode);
		}

		internal override Playable OnCreateClipPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			if (base.isSubTrack)
			{
				throw new InvalidOperationException("Nested animation tracks should never be asked to create a graph directly");
			}
			List<AnimationTrack> list = new List<AnimationTrack>();
			if (CanCompileClips())
			{
				list.Add(this);
			}
			bool flag = AnimatesRootTransform();
			foreach (TrackAsset childTrack in GetChildTracks())
			{
				AnimationTrack animationTrack = childTrack as AnimationTrack;
				if (animationTrack != null && animationTrack.CanCompileClips())
				{
					flag |= animationTrack.AnimatesRootTransform();
					list.Add(animationTrack);
				}
			}
			AppliedOffsetMode offsetMode = GetOffsetMode(go, flag);
			AnimationLayerMixerPlayable animationLayerMixerPlayable = CreateGroupMixer(graph, go, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				Playable source = (list[i].inClipMode ? CompileTrackPlayable(graph, list[i], go, tree, offsetMode) : list[i].CreateInfiniteTrackPlayable(graph, go, tree, offsetMode));
				graph.Connect(source, 0, animationLayerMixerPlayable, i);
				animationLayerMixerPlayable.SetInputWeight(i, (!list[i].inClipMode) ? 1 : 0);
				if (list[i].applyAvatarMask && list[i].avatarMask != null)
				{
					animationLayerMixerPlayable.SetLayerMaskFromAvatarMask((uint)i, list[i].avatarMask);
				}
			}
			Playable playable = animationLayerMixerPlayable;
			if (!RequiresMotionXPlayable(offsetMode, go))
			{
				return playable;
			}
			AnimationMotionXToDeltaPlayable animationMotionXToDeltaPlayable = AnimationMotionXToDeltaPlayable.Create(graph);
			graph.Connect(playable, 0, animationMotionXToDeltaPlayable, 0);
			animationMotionXToDeltaPlayable.SetInputWeight(0, 1f);
			animationMotionXToDeltaPlayable.SetAbsoluteMotion(UsesAbsoluteMotion(offsetMode));
			return animationMotionXToDeltaPlayable;
		}

		private bool RequiresMotionXPlayable(AppliedOffsetMode mode, GameObject gameObject)
		{
			switch (mode)
			{
			case AppliedOffsetMode.NoRootTransform:
				return false;
			case AppliedOffsetMode.SceneOffsetLegacy:
			{
				Animator binding = GetBinding((gameObject != null) ? gameObject.GetComponent<PlayableDirector>() : null);
				if (binding != null)
				{
					return binding.hasRootMotion;
				}
				return false;
			}
			default:
				return true;
			}
		}

		private static bool UsesAbsoluteMotion(AppliedOffsetMode mode)
		{
			if (mode != AppliedOffsetMode.SceneOffset)
			{
				return mode != AppliedOffsetMode.SceneOffsetLegacy;
			}
			return false;
		}

		private bool HasController(GameObject gameObject)
		{
			Animator binding = GetBinding((gameObject != null) ? gameObject.GetComponent<PlayableDirector>() : null);
			if (binding != null)
			{
				return binding.runtimeAnimatorController != null;
			}
			return false;
		}

		internal Animator GetBinding(PlayableDirector director)
		{
			if (director == null)
			{
				return null;
			}
			Object key = this;
			if (base.isSubTrack)
			{
				key = base.parent;
			}
			Object @object = null;
			if (director != null)
			{
				@object = director.GetGenericBinding(key);
			}
			Animator animator = null;
			if (@object != null)
			{
				animator = @object as Animator;
				GameObject gameObject = @object as GameObject;
				if (animator == null && gameObject != null)
				{
					animator = gameObject.GetComponent<Animator>();
				}
			}
			return animator;
		}

		private static AnimationLayerMixerPlayable CreateGroupMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return AnimationLayerMixerPlayable.Create(graph, inputCount);
		}

		private Playable CreateInfiniteTrackPlayable(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, AppliedOffsetMode mode)
		{
			if (m_InfiniteClip == null)
			{
				return Playable.Null;
			}
			AnimationMixerPlayable animationMixerPlayable = AnimationMixerPlayable.Create(graph, 1);
			Playable playable = AnimationPlayableAsset.CreatePlayable(graph, m_InfiniteClip, m_InfiniteClipOffsetPosition, m_InfiniteClipOffsetEulerAngles, removeStartOffset: false, mode, infiniteClipApplyFootIK, AnimationPlayableAsset.LoopMode.Off);
			if (playable.IsValid())
			{
				tree.Add(new InfiniteRuntimeClip(playable));
				graph.Connect(playable, 0, animationMixerPlayable, 0);
				animationMixerPlayable.SetInputWeight(0, 1f);
			}
			return ApplyTrackOffset(graph, animationMixerPlayable, go, mode);
		}

		private Playable ApplyTrackOffset(PlayableGraph graph, Playable root, GameObject go, AppliedOffsetMode mode)
		{
			if (mode == AppliedOffsetMode.SceneOffsetLegacy || mode == AppliedOffsetMode.SceneOffset || mode == AppliedOffsetMode.NoRootTransform || !AnimatesRootTransform())
			{
				return root;
			}
			Vector3 vector = position;
			Quaternion quaternion = rotation;
			AnimationOffsetPlayable animationOffsetPlayable = AnimationOffsetPlayable.Create(graph, vector, quaternion, 1);
			graph.Connect(root, 0, animationOffsetPlayable, 0);
			animationOffsetPlayable.SetInputWeight(0, 1f);
			return animationOffsetPlayable;
		}

		internal override void GetEvaluationTime(out double outStart, out double outDuration)
		{
			if (inClipMode)
			{
				base.GetEvaluationTime(out outStart, out outDuration);
				return;
			}
			outStart = 0.0;
			outDuration = TimelineClip.kMaxTimeValue;
		}

		internal override void GetSequenceTime(out double outStart, out double outDuration)
		{
			if (inClipMode)
			{
				base.GetSequenceTime(out outStart, out outDuration);
				return;
			}
			outStart = 0.0;
			outDuration = Math.Max(GetNotificationDuration(), TimeUtility.GetAnimationClipLength(m_InfiniteClip));
		}

		private void AssignAnimationClip(TimelineClip clip, AnimationClip animClip)
		{
			if (clip == null || animClip == null)
			{
				return;
			}
			if (animClip.legacy)
			{
				throw new InvalidOperationException("Legacy Animation Clips are not supported");
			}
			AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
			if (animationPlayableAsset != null)
			{
				animationPlayableAsset.clip = animClip;
				animationPlayableAsset.name = animClip.name;
				double num = animationPlayableAsset.duration;
				if (!double.IsInfinity(num) && num >= TimelineClip.kMinDuration && num < TimelineClip.kMaxTimeValue)
				{
					clip.duration = num;
				}
			}
			clip.displayName = animClip.name;
		}

		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
		}

		private void GetAnimationClips(List<AnimationClip> animClips)
		{
			TimelineClip[] array = base.clips;
			for (int i = 0; i < array.Length; i++)
			{
				AnimationPlayableAsset animationPlayableAsset = array[i].asset as AnimationPlayableAsset;
				if (animationPlayableAsset != null && animationPlayableAsset.clip != null)
				{
					animClips.Add(animationPlayableAsset.clip);
				}
			}
			if (m_InfiniteClip != null)
			{
				animClips.Add(m_InfiniteClip);
			}
			foreach (TrackAsset childTrack in GetChildTracks())
			{
				AnimationTrack animationTrack = childTrack as AnimationTrack;
				if (animationTrack != null)
				{
					animationTrack.GetAnimationClips(animClips);
				}
			}
		}

		private AppliedOffsetMode GetOffsetMode(GameObject go, bool animatesRootTransform)
		{
			if (!animatesRootTransform)
			{
				return AppliedOffsetMode.NoRootTransform;
			}
			if (m_TrackOffset == TrackOffset.ApplyTransformOffsets)
			{
				return AppliedOffsetMode.TransformOffset;
			}
			if (m_TrackOffset == TrackOffset.ApplySceneOffsets)
			{
				if (!Application.isPlaying)
				{
					return AppliedOffsetMode.SceneOffsetEditor;
				}
				return AppliedOffsetMode.SceneOffset;
			}
			if (HasController(go))
			{
				if (!Application.isPlaying)
				{
					return AppliedOffsetMode.SceneOffsetLegacyEditor;
				}
				return AppliedOffsetMode.SceneOffsetLegacy;
			}
			return AppliedOffsetMode.TransformOffsetLegacy;
		}

		internal bool AnimatesRootTransform()
		{
			if (AnimationPlayableAsset.HasRootTransforms(m_InfiniteClip))
			{
				return true;
			}
			foreach (TimelineClip clip in GetClips())
			{
				AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
				if (animationPlayableAsset != null && animationPlayableAsset.hasRootTransforms)
				{
					return true;
				}
			}
			return false;
		}

		internal override void OnUpgradeFromVersion(int oldVersion)
		{
			if (oldVersion < 1)
			{
				AnimationTrackUpgrade.ConvertRotationsToEuler(this);
			}
			if (oldVersion < 2)
			{
				AnimationTrackUpgrade.ConvertRootMotion(this);
			}
			if (oldVersion < 3)
			{
				AnimationTrackUpgrade.ConvertInfiniteTrack(this);
			}
		}
	}
}
