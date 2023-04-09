using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	[TrackClipType(typeof(TrackAsset))]
	[SupportsChildTracks(null, int.MaxValue)]
	public class GroupTrack : TrackAsset
	{
		public override IEnumerable<PlayableBinding> outputs => PlayableBinding.None;

		internal override bool CanCompileClips()
		{
			return false;
		}
	}
}
