using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	internal struct MarkerList : ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private List<ScriptableObject> m_Objects;

		[NonSerialized]
		[HideInInspector]
		private List<IMarker> m_Cache;

		private bool m_CacheDirty;

		private bool m_HasNotifications;

		public List<IMarker> markers
		{
			get
			{
				BuildCache();
				return m_Cache;
			}
		}

		public int Count => markers.Count;

		public IMarker this[int idx] => markers[idx];

		public MarkerList(int capacity)
		{
			m_Objects = new List<ScriptableObject>(capacity);
			m_Cache = new List<IMarker>(capacity);
			m_CacheDirty = true;
			m_HasNotifications = false;
		}

		public void Add(ScriptableObject item)
		{
			if (!(item == null))
			{
				m_Objects.Add(item);
				m_CacheDirty = true;
			}
		}

		public bool Remove(IMarker item)
		{
			if (!(item is ScriptableObject))
			{
				throw new InvalidOperationException("Supplied type must be a ScriptableObject");
			}
			return Remove((ScriptableObject)item, item.parent.timelineAsset, item.parent);
		}

		public bool Remove(ScriptableObject item, TimelineAsset timelineAsset, PlayableAsset thingToDirty)
		{
			if (!m_Objects.Contains(item))
			{
				return false;
			}
			m_Objects.Remove(item);
			m_CacheDirty = true;
			TimelineUndo.PushDestroyUndo(timelineAsset, thingToDirty, item, "Delete Marker");
			return true;
		}

		public void Clear()
		{
			m_Objects.Clear();
			m_CacheDirty = true;
		}

		public bool Contains(ScriptableObject item)
		{
			return m_Objects.Contains(item);
		}

		public IEnumerable<IMarker> GetMarkers()
		{
			return markers;
		}

		public List<ScriptableObject> GetRawMarkerList()
		{
			return m_Objects;
		}

		public IMarker CreateMarker(Type type, double time, TrackAsset owner)
		{
			if (!typeof(ScriptableObject).IsAssignableFrom(type) || !typeof(IMarker).IsAssignableFrom(type))
			{
				throw new InvalidOperationException("The requested type needs to inherit from ScriptableObject and implement IMarker");
			}
			if (!owner.supportsNotifications && typeof(INotification).IsAssignableFrom(type))
			{
				throw new InvalidOperationException("Markers implementing the INotification interface cannot be added on tracks that do not support notifications");
			}
			ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);
			IMarker obj = (IMarker)scriptableObject;
			obj.time = time;
			TimelineCreateUtilities.SaveAssetIntoObject(scriptableObject, owner);
			Add(scriptableObject);
			obj.Initialize(owner);
			return obj;
		}

		public bool HasNotifications()
		{
			BuildCache();
			return m_HasNotifications;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			m_CacheDirty = true;
		}

		private void BuildCache()
		{
			if (!m_CacheDirty)
			{
				return;
			}
			m_Cache = new List<IMarker>(m_Objects.Count);
			m_HasNotifications = false;
			foreach (ScriptableObject @object in m_Objects)
			{
				if (@object != null)
				{
					m_Cache.Add(@object as IMarker);
					if (@object is INotification)
					{
						m_HasNotifications = true;
					}
				}
			}
			m_CacheDirty = false;
		}
	}
}
