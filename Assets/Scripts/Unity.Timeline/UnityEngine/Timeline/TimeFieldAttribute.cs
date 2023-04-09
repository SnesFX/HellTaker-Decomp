namespace UnityEngine.Timeline
{
	internal class TimeFieldAttribute : PropertyAttribute
	{
		public enum UseEditMode
		{
			None = 0,
			ApplyEditMode = 1
		}

		public UseEditMode useEditMode { get; }

		public TimeFieldAttribute(UseEditMode useEditMode = UseEditMode.ApplyEditMode)
		{
			this.useEditMode = useEditMode;
		}
	}
}
