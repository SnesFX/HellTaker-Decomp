namespace UnityEngine.Timeline
{
	internal static class TimelineClipCapsExtensions
	{
		public static bool SupportsLooping(this TimelineClip clip)
		{
			if (clip != null)
			{
				return (clip.clipCaps & ClipCaps.Looping) != 0;
			}
			return false;
		}

		public static bool SupportsExtrapolation(this TimelineClip clip)
		{
			if (clip != null)
			{
				return (clip.clipCaps & ClipCaps.Extrapolation) != 0;
			}
			return false;
		}

		public static bool SupportsClipIn(this TimelineClip clip)
		{
			if (clip != null)
			{
				return (clip.clipCaps & ClipCaps.ClipIn) != 0;
			}
			return false;
		}

		public static bool SupportsSpeedMultiplier(this TimelineClip clip)
		{
			if (clip != null)
			{
				return (clip.clipCaps & ClipCaps.SpeedMultiplier) != 0;
			}
			return false;
		}

		public static bool SupportsBlending(this TimelineClip clip)
		{
			if (clip != null)
			{
				return (clip.clipCaps & ClipCaps.Blending) != 0;
			}
			return false;
		}

		public static bool HasAll(this ClipCaps caps, ClipCaps flags)
		{
			return (caps & flags) == flags;
		}

		public static bool HasAny(this ClipCaps caps, ClipCaps flags)
		{
			return (caps & flags) != 0;
		}
	}
}
