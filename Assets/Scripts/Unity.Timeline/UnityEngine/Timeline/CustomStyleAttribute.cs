using System;

namespace UnityEngine.Timeline
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomStyleAttribute : Attribute
	{
		public readonly string ussStyle;

		public CustomStyleAttribute(string ussStyle)
		{
			this.ussStyle = ussStyle;
		}
	}
}
