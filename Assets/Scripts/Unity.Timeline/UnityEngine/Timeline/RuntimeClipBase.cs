namespace UnityEngine.Timeline
{
	internal abstract class RuntimeClipBase : RuntimeElement
	{
		public abstract double start { get; }

		public abstract double duration { get; }

		public override long intervalStart => DiscreteTime.GetNearestTick(start);

		public override long intervalEnd => DiscreteTime.GetNearestTick(start + duration);
	}
}
