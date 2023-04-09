using System;

namespace UnityEngine.Timeline
{
	[Flags]
	public enum MatchTargetFields
	{
		PositionX = 1,
		PositionY = 2,
		PositionZ = 4,
		RotationX = 8,
		RotationY = 0x10,
		RotationZ = 0x20
	}
}
