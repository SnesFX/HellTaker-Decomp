using System;

namespace UnityEngine.Timeline
{
	[Serializable]
	[Flags]
	public enum NotificationFlags : short
	{
		TriggerInEditMode = 1,
		Retroactive = 2,
		TriggerOnce = 4
	}
}
