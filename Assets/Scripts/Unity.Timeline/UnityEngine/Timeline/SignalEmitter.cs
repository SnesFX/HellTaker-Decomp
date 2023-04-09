using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	[CustomStyle("SignalEmitter")]
	public class SignalEmitter : Marker, INotification, INotificationOptionProvider
	{
		[SerializeField]
		private bool m_Retroactive;

		[SerializeField]
		private bool m_EmitOnce;

		[SerializeField]
		private SignalAsset m_Asset;

		public bool retroactive
		{
			get
			{
				return m_Retroactive;
			}
			set
			{
				m_Retroactive = value;
			}
		}

		public bool emitOnce
		{
			get
			{
				return m_EmitOnce;
			}
			set
			{
				m_EmitOnce = value;
			}
		}

		public SignalAsset asset
		{
			get
			{
				return m_Asset;
			}
			set
			{
				m_Asset = value;
			}
		}

		PropertyName INotification.id
		{
			get
			{
				if (m_Asset != null)
				{
					return new PropertyName(m_Asset.name);
				}
				return new PropertyName(string.Empty);
			}
		}

		NotificationFlags INotificationOptionProvider.flags => (retroactive ? NotificationFlags.Retroactive : ((NotificationFlags)0)) | (emitOnce ? NotificationFlags.TriggerOnce : ((NotificationFlags)0)) | NotificationFlags.TriggerInEditMode;
	}
}
