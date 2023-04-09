using System;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class PlayableTrack : TrackAsset
	{
		protected override void OnCreateClip(TimelineClip clip)
		{
			if (clip.asset != null)
			{
				clip.displayName = clip.asset.GetType().Name;
			}
		}
	}
}
