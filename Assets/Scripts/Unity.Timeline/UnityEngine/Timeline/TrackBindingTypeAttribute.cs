using System;

namespace UnityEngine.Timeline
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TrackBindingTypeAttribute : Attribute
	{
		public readonly Type type;

		public readonly TrackBindingFlags flags;

		public TrackBindingTypeAttribute(Type type)
		{
			this.type = type;
			flags = TrackBindingFlags.AllowCreateComponent;
		}

		public TrackBindingTypeAttribute(Type type, TrackBindingFlags flags)
		{
			this.type = type;
			this.flags = flags;
		}
	}
}
