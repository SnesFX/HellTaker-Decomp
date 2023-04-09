using System;

namespace UnityEngine.Timeline
{
	public abstract class Marker : ScriptableObject, IMarker
	{
		[SerializeField]
		[TimeField(TimeFieldAttribute.UseEditMode.ApplyEditMode)]
		[Tooltip("Time for the marker")]
		private double m_Time;

		public TrackAsset parent { get; private set; }

		public double time
		{
			get
			{
				return m_Time;
			}
			set
			{
				m_Time = Math.Max(value, 0.0);
			}
		}

		void IMarker.Initialize(TrackAsset parentTrack)
		{
			if (parent == null)
			{
				parent = parentTrack;
				try
				{
					OnInitialize(parentTrack);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message, this);
				}
			}
		}

		public virtual void OnInitialize(TrackAsset aPent)
		{
		}
	}
}
