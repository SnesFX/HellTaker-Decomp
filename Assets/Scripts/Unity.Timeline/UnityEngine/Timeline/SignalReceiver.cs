using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class SignalReceiver : MonoBehaviour, INotificationReceiver
	{
		[Serializable]
		private class EventKeyValue
		{
			[SerializeField]
			private List<SignalAsset> m_Signals = new List<SignalAsset>();

			[SerializeField]
			[CustomSignalEventDrawer]
			private List<UnityEvent> m_Events = new List<UnityEvent>();

			public List<SignalAsset> signals => m_Signals;

			public List<UnityEvent> events => m_Events;

			public bool TryGetValue(SignalAsset key, out UnityEvent value)
			{
				int num = m_Signals.IndexOf(key);
				if (num != -1)
				{
					value = m_Events[num];
					return true;
				}
				value = null;
				return false;
			}

			public void Append(SignalAsset key, UnityEvent value)
			{
				m_Signals.Add(key);
				m_Events.Add(value);
			}

			public void Remove(int idx)
			{
				if (idx != -1)
				{
					m_Signals.RemoveAt(idx);
					m_Events.RemoveAt(idx);
				}
			}

			public void Remove(SignalAsset key)
			{
				int num = m_Signals.IndexOf(key);
				if (num != -1)
				{
					m_Signals.RemoveAt(num);
					m_Events.RemoveAt(num);
				}
			}
		}

		[SerializeField]
		private EventKeyValue m_Events = new EventKeyValue();

		public void OnNotify(Playable origin, INotification notification, object context)
		{
			SignalEmitter signalEmitter = notification as SignalEmitter;
			if (signalEmitter != null && signalEmitter.asset != null && m_Events.TryGetValue(signalEmitter.asset, out var value))
			{
				value?.Invoke();
			}
		}

		public void AddReaction(SignalAsset asset, UnityEvent reaction)
		{
			if (asset == null)
			{
				throw new ArgumentNullException("asset");
			}
			if (m_Events.signals.Contains(asset))
			{
				throw new ArgumentException("SignalAsset already used.");
			}
			m_Events.Append(asset, reaction);
		}

		public int AddEmptyReaction(UnityEvent reaction)
		{
			m_Events.Append(null, reaction);
			return m_Events.events.Count - 1;
		}

		public void Remove(SignalAsset asset)
		{
			if (!m_Events.signals.Contains(asset))
			{
				throw new ArgumentException("The SignalAsset is not registered with this receiver.");
			}
			m_Events.Remove(asset);
		}

		public IEnumerable<SignalAsset> GetRegisteredSignals()
		{
			return m_Events.signals;
		}

		public UnityEvent GetReaction(SignalAsset key)
		{
			if (m_Events.TryGetValue(key, out var value))
			{
				return value;
			}
			return null;
		}

		public int Count()
		{
			return m_Events.signals.Count;
		}

		public void ChangeSignalAtIndex(int idx, SignalAsset newKey)
		{
			if (idx < 0 || idx > m_Events.signals.Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			if (!(m_Events.signals[idx] == newKey))
			{
				bool flag = m_Events.signals.Contains(newKey);
				if (newKey == null || m_Events.signals[idx] == null || !flag)
				{
					m_Events.signals[idx] = newKey;
				}
				if (flag)
				{
					throw new ArgumentException("SignalAsset already used.");
				}
			}
		}

		public void RemoveAtIndex(int idx)
		{
			if (idx < 0 || idx > m_Events.signals.Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			m_Events.Remove(idx);
		}

		public void ChangeReactionAtIndex(int idx, UnityEvent reaction)
		{
			if (idx < 0 || idx > m_Events.events.Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			m_Events.events[idx] = reaction;
		}

		public UnityEvent GetReactionAtIndex(int idx)
		{
			if (idx < 0 || idx > m_Events.events.Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			return m_Events.events[idx];
		}

		public SignalAsset GetSignalAssetAtIndex(int idx)
		{
			if (idx < 0 || idx > m_Events.signals.Count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			return m_Events.signals[idx];
		}

		private void OnEnable()
		{
		}
	}
}
