using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class TimelineClip : ICurvesOwner, ISerializationCallbackReceiver
	{
		private enum Versions
		{
			Initial = 0,
			ClipInFromGlobalToLocal = 1
		}

		private static class TimelineClipUpgrade
		{
			public static void UpgradeClipInFromGlobalToLocal(TimelineClip clip)
			{
				if (clip.m_ClipIn > 0.0 && clip.m_TimeScale > 1.401298464324817E-45)
				{
					clip.m_ClipIn *= clip.m_TimeScale;
				}
			}
		}

		public enum ClipExtrapolation
		{
			None = 0,
			Hold = 1,
			Loop = 2,
			PingPong = 3,
			Continue = 4
		}

		public enum BlendCurveMode
		{
			Auto = 0,
			Manual = 1
		}

		private const int k_LatestVersion = 1;

		[SerializeField]
		[HideInInspector]
		private int m_Version;

		public static readonly ClipCaps kDefaultClipCaps = ClipCaps.Blending;

		public static readonly float kDefaultClipDurationInSeconds = 5f;

		public static readonly double kTimeScaleMin = 0.001;

		public static readonly double kTimeScaleMax = 1000.0;

		internal static readonly string kDefaultCurvesName = "Clip Parameters";

		internal static readonly double kMinDuration = 1.0 / 60.0;

		internal static readonly double kMaxTimeValue = 1000000.0;

		[SerializeField]
		private double m_Start;

		[SerializeField]
		private double m_ClipIn;

		[SerializeField]
		private Object m_Asset;

		[SerializeField]
		[FormerlySerializedAs("m_HackDuration")]
		private double m_Duration;

		[SerializeField]
		private double m_TimeScale = 1.0;

		[SerializeField]
		private TrackAsset m_ParentTrack;

		[SerializeField]
		private double m_EaseInDuration;

		[SerializeField]
		private double m_EaseOutDuration;

		[SerializeField]
		private double m_BlendInDuration = -1.0;

		[SerializeField]
		private double m_BlendOutDuration = -1.0;

		[SerializeField]
		private AnimationCurve m_MixInCurve;

		[SerializeField]
		private AnimationCurve m_MixOutCurve;

		[SerializeField]
		private BlendCurveMode m_BlendInCurveMode;

		[SerializeField]
		private BlendCurveMode m_BlendOutCurveMode;

		[SerializeField]
		private List<string> m_ExposedParameterNames;

		[SerializeField]
		private AnimationClip m_AnimationCurves;

		[SerializeField]
		private bool m_Recordable;

		[SerializeField]
		private ClipExtrapolation m_PostExtrapolationMode;

		[SerializeField]
		private ClipExtrapolation m_PreExtrapolationMode;

		[SerializeField]
		private double m_PostExtrapolationTime;

		[SerializeField]
		private double m_PreExtrapolationTime;

		[SerializeField]
		private string m_DisplayName;

		public bool hasPreExtrapolation
		{
			get
			{
				if (m_PreExtrapolationMode != 0)
				{
					return m_PreExtrapolationTime > 0.0;
				}
				return false;
			}
		}

		public bool hasPostExtrapolation
		{
			get
			{
				if (m_PostExtrapolationMode != 0)
				{
					return m_PostExtrapolationTime > 0.0;
				}
				return false;
			}
		}

		public double timeScale
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.SpeedMultiplier))
				{
					return 1.0;
				}
				return Math.Max(kTimeScaleMin, Math.Min(m_TimeScale, kTimeScaleMax));
			}
			set
			{
				UpdateDirty(m_TimeScale, value);
				m_TimeScale = (clipCaps.HasAny(ClipCaps.SpeedMultiplier) ? Math.Max(kTimeScaleMin, Math.Min(value, kTimeScaleMax)) : 1.0);
			}
		}

		public double start
		{
			get
			{
				return m_Start;
			}
			set
			{
				UpdateDirty(value, m_Start);
				double num = Math.Max(SanitizeTimeValue(value, m_Start), 0.0);
				if (m_ParentTrack != null && m_Start != num)
				{
					m_ParentTrack.OnClipMove();
				}
				m_Start = num;
			}
		}

		public double duration
		{
			get
			{
				return m_Duration;
			}
			set
			{
				UpdateDirty(m_Duration, value);
				m_Duration = Math.Max(SanitizeTimeValue(value, m_Duration), double.Epsilon);
			}
		}

		public double end => m_Start + m_Duration;

		public double clipIn
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.ClipIn))
				{
					return 0.0;
				}
				return m_ClipIn;
			}
			set
			{
				UpdateDirty(m_ClipIn, value);
				m_ClipIn = (clipCaps.HasAny(ClipCaps.ClipIn) ? Math.Max(Math.Min(SanitizeTimeValue(value, m_ClipIn), kMaxTimeValue), 0.0) : 0.0);
			}
		}

		public string displayName
		{
			get
			{
				return m_DisplayName;
			}
			set
			{
				m_DisplayName = value;
			}
		}

		public double clipAssetDuration
		{
			get
			{
				if (!(m_Asset is IPlayableAsset playableAsset))
				{
					return double.MaxValue;
				}
				return playableAsset.duration;
			}
		}

		public AnimationClip curves
		{
			get
			{
				return m_AnimationCurves;
			}
			internal set
			{
				m_AnimationCurves = value;
			}
		}

		string ICurvesOwner.defaultCurvesName => kDefaultCurvesName;

		public bool hasCurves
		{
			get
			{
				if (m_AnimationCurves != null)
				{
					return !m_AnimationCurves.empty;
				}
				return false;
			}
		}

		public Object asset
		{
			get
			{
				return m_Asset;
			}
			set
			{
				m_Asset = value;
			}
		}

		Object ICurvesOwner.assetOwner => parentTrack;

		TrackAsset ICurvesOwner.targetTrack => parentTrack;

		[Obsolete("underlyingAsset property is obsolete. Use asset property instead", true)]
		public Object underlyingAsset
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public TrackAsset parentTrack
		{
			get
			{
				return m_ParentTrack;
			}
			set
			{
				if (!(m_ParentTrack == value))
				{
					if (m_ParentTrack != null)
					{
						m_ParentTrack.RemoveClip(this);
					}
					m_ParentTrack = value;
					if (m_ParentTrack != null)
					{
						m_ParentTrack.AddClip(this);
					}
				}
			}
		}

		public double easeInDuration
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Blending))
				{
					return 0.0;
				}
				return Math.Min(Math.Max(m_EaseInDuration, 0.0), duration * 0.49);
			}
			set
			{
				m_EaseInDuration = (clipCaps.HasAny(ClipCaps.Blending) ? Math.Max(0.0, Math.Min(SanitizeTimeValue(value, m_EaseInDuration), duration * 0.49)) : 0.0);
			}
		}

		public double easeOutDuration
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Blending))
				{
					return 0.0;
				}
				return Math.Min(Math.Max(m_EaseOutDuration, 0.0), duration * 0.49);
			}
			set
			{
				m_EaseOutDuration = (clipCaps.HasAny(ClipCaps.Blending) ? Math.Max(0.0, Math.Min(SanitizeTimeValue(value, m_EaseOutDuration), duration * 0.49)) : 0.0);
			}
		}

		[Obsolete("Use easeOutTime instead (UnityUpgradable) -> easeOutTime", true)]
		public double eastOutTime => duration - easeOutDuration + m_Start;

		public double easeOutTime => duration - easeOutDuration + m_Start;

		public double blendInDuration
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Blending))
				{
					return 0.0;
				}
				return m_BlendInDuration;
			}
			set
			{
				m_BlendInDuration = (clipCaps.HasAny(ClipCaps.Blending) ? SanitizeTimeValue(value, m_BlendInDuration) : 0.0);
			}
		}

		public double blendOutDuration
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Blending))
				{
					return 0.0;
				}
				return m_BlendOutDuration;
			}
			set
			{
				m_BlendOutDuration = (clipCaps.HasAny(ClipCaps.Blending) ? SanitizeTimeValue(value, m_BlendOutDuration) : 0.0);
			}
		}

		public BlendCurveMode blendInCurveMode
		{
			get
			{
				return m_BlendInCurveMode;
			}
			set
			{
				m_BlendInCurveMode = value;
			}
		}

		public BlendCurveMode blendOutCurveMode
		{
			get
			{
				return m_BlendOutCurveMode;
			}
			set
			{
				m_BlendOutCurveMode = value;
			}
		}

		public bool hasBlendIn
		{
			get
			{
				if (clipCaps.HasAny(ClipCaps.Blending))
				{
					return m_BlendInDuration > 0.0;
				}
				return false;
			}
		}

		public bool hasBlendOut
		{
			get
			{
				if (clipCaps.HasAny(ClipCaps.Blending))
				{
					return m_BlendOutDuration > 0.0;
				}
				return false;
			}
		}

		public AnimationCurve mixInCurve
		{
			get
			{
				if (m_MixInCurve == null || m_MixInCurve.length < 2)
				{
					m_MixInCurve = GetDefaultMixInCurve();
				}
				return m_MixInCurve;
			}
			set
			{
				m_MixInCurve = value;
			}
		}

		public float mixInPercentage => (float)(mixInDuration / duration);

		public double mixInDuration
		{
			get
			{
				if (!hasBlendIn)
				{
					return easeInDuration;
				}
				return blendInDuration;
			}
		}

		public AnimationCurve mixOutCurve
		{
			get
			{
				if (m_MixOutCurve == null || m_MixOutCurve.length < 2)
				{
					m_MixOutCurve = GetDefaultMixOutCurve();
				}
				return m_MixOutCurve;
			}
			set
			{
				m_MixOutCurve = value;
			}
		}

		public double mixOutTime => duration - mixOutDuration + m_Start;

		public double mixOutDuration
		{
			get
			{
				if (!hasBlendOut)
				{
					return easeOutDuration;
				}
				return blendOutDuration;
			}
		}

		public float mixOutPercentage => (float)(mixOutDuration / duration);

		public bool recordable
		{
			get
			{
				return m_Recordable;
			}
			internal set
			{
				m_Recordable = value;
			}
		}

		[Obsolete("exposedParameter is deprecated and will be removed in a future release", true)]
		public List<string> exposedParameters => m_ExposedParameterNames ?? (m_ExposedParameterNames = new List<string>());

		public ClipCaps clipCaps
		{
			get
			{
				if (!(asset is ITimelineClipAsset timelineClipAsset))
				{
					return kDefaultClipCaps;
				}
				return timelineClipAsset.clipCaps;
			}
		}

		public AnimationClip animationClip
		{
			get
			{
				if (m_Asset == null)
				{
					return null;
				}
				AnimationPlayableAsset animationPlayableAsset = m_Asset as AnimationPlayableAsset;
				if (!(animationPlayableAsset != null))
				{
					return null;
				}
				return animationPlayableAsset.clip;
			}
		}

		public ClipExtrapolation postExtrapolationMode
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Extrapolation))
				{
					return ClipExtrapolation.None;
				}
				return m_PostExtrapolationMode;
			}
			internal set
			{
				m_PostExtrapolationMode = (clipCaps.HasAny(ClipCaps.Extrapolation) ? value : ClipExtrapolation.None);
			}
		}

		public ClipExtrapolation preExtrapolationMode
		{
			get
			{
				if (!clipCaps.HasAny(ClipCaps.Extrapolation))
				{
					return ClipExtrapolation.None;
				}
				return m_PreExtrapolationMode;
			}
			internal set
			{
				m_PreExtrapolationMode = (clipCaps.HasAny(ClipCaps.Extrapolation) ? value : ClipExtrapolation.None);
			}
		}

		public double extrapolatedStart
		{
			get
			{
				if (m_PreExtrapolationMode != 0)
				{
					return m_Start - m_PreExtrapolationTime;
				}
				return m_Start;
			}
		}

		public double extrapolatedDuration
		{
			get
			{
				double num = m_Duration;
				if (m_PostExtrapolationMode != 0)
				{
					num += Math.Min(m_PostExtrapolationTime, kMaxTimeValue);
				}
				if (m_PreExtrapolationMode != 0)
				{
					num += m_PreExtrapolationTime;
				}
				return num;
			}
		}

		private void UpgradeToLatestVersion()
		{
			if (m_Version < 1)
			{
				TimelineClipUpgrade.UpgradeClipInFromGlobalToLocal(this);
			}
		}

		internal TimelineClip(TrackAsset parent)
		{
			parentTrack = parent;
		}

		internal int Hash()
		{
			int hashCode = m_Start.GetHashCode();
			int hashCode2 = m_Duration.GetHashCode();
			int hashCode3 = m_TimeScale.GetHashCode();
			int hashCode4 = m_ClipIn.GetHashCode();
			int num = (int)m_PreExtrapolationMode;
			int hashCode5 = num.GetHashCode();
			num = (int)m_PostExtrapolationMode;
			return HashUtility.CombineHash(hashCode, hashCode2, hashCode3, hashCode4, hashCode5, num.GetHashCode());
		}

		public float EvaluateMixOut(double time)
		{
			if (!clipCaps.HasAny(ClipCaps.Blending))
			{
				return 1f;
			}
			if (mixOutDuration > (double)Mathf.Epsilon)
			{
				float time2 = (float)(time - mixOutTime) / (float)mixOutDuration;
				return Mathf.Clamp01(mixOutCurve.Evaluate(time2));
			}
			return 1f;
		}

		public float EvaluateMixIn(double time)
		{
			if (!clipCaps.HasAny(ClipCaps.Blending))
			{
				return 1f;
			}
			if (mixInDuration > (double)Mathf.Epsilon)
			{
				float time2 = (float)(time - m_Start) / (float)mixInDuration;
				return Mathf.Clamp01(mixInCurve.Evaluate(time2));
			}
			return 1f;
		}

		private static AnimationCurve GetDefaultMixInCurve()
		{
			return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}

		private static AnimationCurve GetDefaultMixOutCurve()
		{
			return AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
		}

		public double ToLocalTime(double time)
		{
			if (time < 0.0)
			{
				return time;
			}
			time = (IsPreExtrapolatedTime(time) ? GetExtrapolatedTime(time - m_Start, m_PreExtrapolationMode, m_Duration) : ((!IsPostExtrapolatedTime(time)) ? (time - m_Start) : GetExtrapolatedTime(time - m_Start, m_PostExtrapolationMode, m_Duration)));
			time *= timeScale;
			time += clipIn;
			return time;
		}

		public double ToLocalTimeUnbound(double time)
		{
			return (time - m_Start) * timeScale + clipIn;
		}

		internal double FromLocalTimeUnbound(double time)
		{
			return (time - clipIn) / timeScale + m_Start;
		}

		private static double SanitizeTimeValue(double value, double defaultValue)
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				Debug.LogError("Invalid time value assigned");
				return defaultValue;
			}
			return Math.Max(0.0 - kMaxTimeValue, Math.Min(kMaxTimeValue, value));
		}

		internal void SetPostExtrapolationTime(double time)
		{
			m_PostExtrapolationTime = time;
		}

		internal void SetPreExtrapolationTime(double time)
		{
			m_PreExtrapolationTime = time;
		}

		public bool IsExtrapolatedTime(double sequenceTime)
		{
			if (!IsPreExtrapolatedTime(sequenceTime))
			{
				return IsPostExtrapolatedTime(sequenceTime);
			}
			return true;
		}

		public bool IsPreExtrapolatedTime(double sequenceTime)
		{
			if (preExtrapolationMode != 0 && sequenceTime < m_Start)
			{
				return sequenceTime >= m_Start - m_PreExtrapolationTime;
			}
			return false;
		}

		public bool IsPostExtrapolatedTime(double sequenceTime)
		{
			if (postExtrapolationMode != 0 && sequenceTime > end)
			{
				return sequenceTime - end < m_PostExtrapolationTime;
			}
			return false;
		}

		private static double GetExtrapolatedTime(double time, ClipExtrapolation mode, double duration)
		{
			if (duration == 0.0)
			{
				return 0.0;
			}
			switch (mode)
			{
			case ClipExtrapolation.Loop:
				if (time < 0.0)
				{
					time = duration - (0.0 - time) % duration;
				}
				else if (time > duration)
				{
					time %= duration;
				}
				break;
			case ClipExtrapolation.Hold:
				if (time < 0.0)
				{
					return 0.0;
				}
				if (time > duration)
				{
					return duration;
				}
				break;
			case ClipExtrapolation.PingPong:
				if (time < 0.0)
				{
					time = duration * 2.0 - (0.0 - time) % (duration * 2.0);
					time = duration - Math.Abs(time - duration);
				}
				else
				{
					time %= duration * 2.0;
					time = duration - Math.Abs(time - duration);
				}
				break;
			}
			return time;
		}

		public void CreateCurves(string curvesClipName)
		{
			if (!(m_AnimationCurves != null))
			{
				m_AnimationCurves = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(curvesClipName) ? kDefaultCurvesName : curvesClipName, parentTrack, isLegacy: true);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Version = 1;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (m_Version < 1)
			{
				UpgradeToLatestVersion();
			}
		}

		public override string ToString()
		{
			return UnityString.Format("{0} ({1:F2}, {2:F2}):{3:F2} | {4}", displayName, start, end, clipIn, parentTrack);
		}

		private void UpdateDirty(double oldValue, double newValue)
		{
		}
	}
}
