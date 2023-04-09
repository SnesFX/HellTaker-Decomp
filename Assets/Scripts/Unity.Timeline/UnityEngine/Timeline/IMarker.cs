namespace UnityEngine.Timeline
{
	public interface IMarker
	{
		double time { get; set; }

		TrackAsset parent { get; }

		void Initialize(TrackAsset parent);
	}
}
