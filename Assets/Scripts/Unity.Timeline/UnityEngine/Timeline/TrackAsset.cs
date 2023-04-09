using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityEngine.Timeline
{
	[Serializable]
	[IgnoreOnPlayableTrack]
	public abstract class TrackAsset : PlayableAsset, ISerializationCallbackReceiver, IPropertyPreview, ICurvesOwner
	{
		internal enum Versions
		{
			Initial = 0,
			RotationAsEuler = 1,
			RootMotionUpgrade = 2,
			AnimatedTrackProperties = 3
		}

		private static class TrackAssetUpgrade
		{
		}

		private const int k_LatestVersion = 3;

		[SerializeField]
		[HideInInspector]
		private int m_Version;

		[Obsolete("Please use m_InfiniteClip (on AnimationTrack) instead.", false)]
		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("m_animClip")]
		internal AnimationClip m_AnimClip;

		internal const string kDefaultCurvesName = "Track Parameters";

		[SerializeField]
		[HideInInspector]
		private bool m_Locked;

		[SerializeField]
		[HideInInspector]
		private bool m_Muted;

		[SerializeField]
		[HideInInspector]
		private string m_CustomPlayableFullTypename = string.Empty;

		[SerializeField]
		[HideInInspector]
		private AnimationClip m_Curves;

		[SerializeField]
		[HideInInspector]
		private PlayableAsset m_Parent;

		[SerializeField]
		[HideInInspector]
		private List<ScriptableObject> m_Children;

		[NonSerialized]
		private int m_ItemsHash;

		[NonSerialized]
		private TimelineClip[] m_ClipsCache;

		private DiscreteTime m_Start;

		private DiscreteTime m_End;

		private bool m_CacheSorted;

		private bool? m_SupportsNotifications;

		private static TrackAsset[] s_EmptyCache = new TrackAsset[0];

		private IEnumerable<TrackAsset> m_ChildTrackCache;

		private static Dictionary<Type, TrackBindingTypeAttribute> s_TrackBindingTypeAttributeCache = new Dictionary<Type, TrackBindingTypeAttribute>();

		[SerializeField]
		[HideInInspector]
		protected internal List<TimelineClip> m_Clips = new List<TimelineClip>();

		[SerializeField]
		[HideInInspector]
		private MarkerList m_Markers = new MarkerList(0);

		public double start
		{
			get
			{
				UpdateDuration();
				return (double)m_Start;
			}
		}

		public double end
		{
			get
			{
				UpdateDuration();
				return (double)m_End;
			}
		}

		public sealed override double duration
		{
			get
			{
				UpdateDuration();
				return (double)(m_End - m_Start);
			}
		}

		public bool muted
		{
			get
			{
				return m_Muted;
			}
			set
			{
				m_Muted = value;
			}
		}

		public bool mutedInHierarchy
		{
			get
			{
				if (muted)
				{
					return true;
				}
				TrackAsset trackAsset = this;
				while (trackAsset.parent as TrackAsset != null)
				{
					trackAsset = (TrackAsset)trackAsset.parent;
					if (trackAsset as GroupTrack != null)
					{
						return trackAsset.mutedInHierarchy;
					}
				}
				return false;
			}
		}

		public TimelineAsset timelineAsset
		{
			get
			{
				TrackAsset trackAsset = this;
				while (trackAsset != null)
				{
					if (trackAsset.parent == null)
					{
						return null;
					}
					TimelineAsset timelineAsset = trackAsset.parent as TimelineAsset;
					if (timelineAsset != null)
					{
						return timelineAsset;
					}
					trackAsset = trackAsset.parent as TrackAsset;
				}
				return null;
			}
		}

		public PlayableAsset parent
		{
			get
			{
				return m_Parent;
			}
			internal set
			{
				m_Parent = value;
			}
		}

		internal TimelineClip[] clips
		{
			get
			{
				if (m_Clips == null)
				{
					m_Clips = new List<TimelineClip>();
				}
				if (m_ClipsCache == null)
				{
					m_CacheSorted = false;
					m_ClipsCache = m_Clips.ToArray();
				}
				return m_ClipsCache;
			}
		}

		public virtual bool isEmpty
		{
			get
			{
				if (!hasClips && !hasCurves)
				{
					return GetMarkerCount() == 0;
				}
				return false;
			}
		}

		public bool hasClips
		{
			get
			{
				if (m_Clips != null)
				{
					return m_Clips.Count != 0;
				}
				return false;
			}
		}

		public bool hasCurves
		{
			get
			{
				if (m_Curves != null)
				{
					return !m_Curves.empty;
				}
				return false;
			}
		}

		public bool isSubTrack
		{
			get
			{
				TrackAsset trackAsset = parent as TrackAsset;
				if (trackAsset != null)
				{
					return trackAsset.GetType() == GetType();
				}
				return false;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				if (!s_TrackBindingTypeAttributeCache.TryGetValue(GetType(), out var value))
				{
					value = (TrackBindingTypeAttribute)Attribute.GetCustomAttribute(GetType(), typeof(TrackBindingTypeAttribute));
					s_TrackBindingTypeAttributeCache.Add(GetType(), value);
				}
				Type type = value?.type;
				yield return ScriptPlayableBinding.Create(base.name, this, type);
			}
		}

		internal string customPlayableTypename
		{
			get
			{
				return m_CustomPlayableFullTypename;
			}
			set
			{
				m_CustomPlayableFullTypename = value;
			}
		}

		public AnimationClip curves
		{
			get
			{
				return m_Curves;
			}
			internal set
			{
				m_Curves = value;
			}
		}

		string ICurvesOwner.defaultCurvesName => "Track Parameters";

		Object ICurvesOwner.asset => this;

		Object ICurvesOwner.assetOwner => timelineAsset;

		TrackAsset ICurvesOwner.targetTrack => this;

		internal List<ScriptableObject> subTracksObjects => m_Children;

		public bool locked
		{
			get
			{
				return m_Locked;
			}
			set
			{
				m_Locked = value;
			}
		}

		public bool lockedInHierarchy
		{
			get
			{
				if (!locked)
				{
					return parentLocked;
				}
				return true;
			}
		}

		internal bool parentLocked
		{
			get
			{
				TrackAsset trackAsset = parent as TrackAsset;
				if (trackAsset != null)
				{
					return trackAsset.lockedInHierarchy;
				}
				return false;
			}
		}

		public bool supportsNotifications
		{
			get
			{
				if (!m_SupportsNotifications.HasValue)
				{
					m_SupportsNotifications = NotificationUtilities.TrackTypeSupportsNotifications(GetType());
				}
				return m_SupportsNotifications.Value;
			}
		}

		internal static event Action<TimelineClip, GameObject, Playable> OnClipPlayableCreate;

		internal static event Action<TrackAsset, GameObject, Playable> OnTrackAnimationPlayableCreate;

		protected virtual void OnBeforeTrackSerialize()
		{
		}

		protected virtual void OnAfterTrackDeserialize()
		{
		}

		internal virtual void OnUpgradeFromVersion(int oldVersion)
		{
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Version = 3;
			if (m_Children != null)
			{
				for (int num = m_Children.Count - 1; num >= 0; num--)
				{
					TrackAsset trackAsset = m_Children[num] as TrackAsset;
					if (trackAsset != null && trackAsset.parent != this)
					{
						trackAsset.parent = this;
					}
				}
			}
			OnBeforeTrackSerialize();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			m_ClipsCache = null;
			Invalidate();
			if (m_Version < 3)
			{
				UpgradeToLatestVersion();
				OnUpgradeFromVersion(m_Version);
			}
			foreach (IMarker marker in GetMarkers())
			{
				marker.Initialize(this);
			}
			OnAfterTrackDeserialize();
		}

		private void UpgradeToLatestVersion()
		{
		}

		public IEnumerable<TimelineClip> GetClips()
		{
			return clips;
		}

		public IEnumerable<TrackAsset> GetChildTracks()
		{
			UpdateChildTrackCache();
			return m_ChildTrackCache;
		}

		private void __internalAwake()
		{
			if (m_Clips == null)
			{
				m_Clips = new List<TimelineClip>();
			}
			m_ChildTrackCache = null;
			if (m_Children == null)
			{
				m_Children = new List<ScriptableObject>();
			}
		}

		public void CreateCurves(string curvesClipName)
		{
			if (!(m_Curves != null))
			{
				m_Curves = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(curvesClipName) ? "Track Parameters" : curvesClipName, this, isLegacy: true);
			}
		}

		public virtual Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return Playable.Create(graph, inputCount);
		}

		public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			return Playable.Null;
		}

		public TimelineClip CreateDefaultClip()
		{
			object[] customAttributes = GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), inherit: true);
			Type type = null;
			object[] array = customAttributes;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is TrackClipTypeAttribute trackClipTypeAttribute && typeof(IPlayableAsset).IsAssignableFrom(trackClipTypeAttribute.inspectedType) && typeof(ScriptableObject).IsAssignableFrom(trackClipTypeAttribute.inspectedType))
				{
					type = trackClipTypeAttribute.inspectedType;
					break;
				}
			}
			if (type == null)
			{
				Debug.LogWarning("Cannot create a default clip for type " + GetType());
				return null;
			}
			return CreateAndAddNewClipOfType(type);
		}

		public TimelineClip CreateClip<T>() where T : ScriptableObject, IPlayableAsset
		{
			return CreateClip(typeof(T));
		}

		public IMarker CreateMarker(Type type, double time)
		{
			return m_Markers.CreateMarker(type, time, this);
		}

		public T CreateMarker<T>(double time) where T : ScriptableObject, IMarker
		{
			return (T)CreateMarker(typeof(T), time);
		}

		public bool DeleteMarker(IMarker marker)
		{
			return m_Markers.Remove(marker);
		}

		public IEnumerable<IMarker> GetMarkers()
		{
			return m_Markers.GetMarkers();
		}

		public int GetMarkerCount()
		{
			return m_Markers.Count;
		}

		public IMarker GetMarker(int idx)
		{
			return m_Markers[idx];
		}

		internal TimelineClip CreateClip(Type requestedType)
		{
			if (ValidateClipType(requestedType))
			{
				return CreateAndAddNewClipOfType(requestedType);
			}
			throw new InvalidOperationException(string.Concat("Clips of type ", requestedType, " are not permitted on tracks of type ", GetType()));
		}

		internal TimelineClip CreateAndAddNewClipOfType(Type requestedType)
		{
			TimelineClip timelineClip = CreateClipOfType(requestedType);
			AddClip(timelineClip);
			return timelineClip;
		}

		internal TimelineClip CreateClipOfType(Type requestedType)
		{
			if (!ValidateClipType(requestedType))
			{
				throw new InvalidOperationException(string.Concat("Clips of type ", requestedType, " are not permitted on tracks of type ", GetType()));
			}
			ScriptableObject scriptableObject = ScriptableObject.CreateInstance(requestedType);
			if (scriptableObject == null)
			{
				throw new InvalidOperationException("Could not create an instance of the ScriptableObject type " + requestedType.Name);
			}
			scriptableObject.name = requestedType.Name;
			TimelineCreateUtilities.SaveAssetIntoObject(scriptableObject, this);
			return CreateClipFromAsset(scriptableObject);
		}

		internal TimelineClip CreateClipFromPlayableAsset(IPlayableAsset asset)
		{
			if (asset == null)
			{
				throw new ArgumentNullException("asset");
			}
			if (asset as ScriptableObject == null)
			{
				throw new ArgumentException("CreateClipFromPlayableAsset  only supports ScriptableObject-derived Types");
			}
			if (!ValidateClipType(asset.GetType()))
			{
				throw new InvalidOperationException(string.Concat("Clips of type ", asset.GetType(), " are not permitted on tracks of type ", GetType()));
			}
			return CreateClipFromAsset(asset as ScriptableObject);
		}

		private TimelineClip CreateClipFromAsset(ScriptableObject playableAsset)
		{
			TimelineClip timelineClip = CreateNewClipContainerInternal();
			timelineClip.displayName = playableAsset.name;
			timelineClip.asset = playableAsset;
			if (playableAsset is IPlayableAsset playableAsset2)
			{
				double num = playableAsset2.duration;
				if (!double.IsInfinity(num) && num > 0.0)
				{
					timelineClip.duration = Math.Min(Math.Max(num, TimelineClip.kMinDuration), TimelineClip.kMaxTimeValue);
				}
			}
			try
			{
				OnCreateClip(timelineClip);
				return timelineClip;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message, playableAsset);
				return null;
			}
		}

		internal IEnumerable<ScriptableObject> GetMarkersRaw()
		{
			return m_Markers.GetRawMarkerList();
		}

		internal void ClearMarkers()
		{
			m_Markers.Clear();
		}

		internal void AddMarker(ScriptableObject e)
		{
			m_Markers.Add(e);
		}

		internal bool DeleteMarkerRaw(ScriptableObject marker)
		{
			return m_Markers.Remove(marker, timelineAsset, this);
		}

		private int GetTimeRangeHash()
		{
			double num = double.MaxValue;
			double num2 = double.MinValue;
			foreach (IMarker marker in GetMarkers())
			{
				if (marker is INotification)
				{
					if (marker.time < num)
					{
						num = marker.time;
					}
					if (marker.time > num2)
					{
						num2 = marker.time;
					}
				}
			}
			return num.GetHashCode().CombineHash(num2.GetHashCode());
		}

		internal void AddClip(TimelineClip newClip)
		{
			if (!m_Clips.Contains(newClip))
			{
				m_Clips.Add(newClip);
				m_ClipsCache = null;
			}
		}

		private Playable CreateNotificationsPlayable(PlayableGraph graph, Playable mixerPlayable, GameObject go, Playable timelinePlayable)
		{
			UpdateDuration();
			ScriptPlayable<TimeNotificationBehaviour> scriptPlayable = NotificationUtilities.CreateNotificationsPlayable(graph, GetMarkers(), go);
			if (scriptPlayable.IsValid())
			{
				scriptPlayable.GetBehaviour().timeSource = timelinePlayable;
				if (mixerPlayable.IsValid())
				{
					scriptPlayable.SetInputCount(1);
					graph.Connect(mixerPlayable, 0, scriptPlayable, 0);
					scriptPlayable.SetInputWeight(mixerPlayable, 1f);
				}
			}
			return scriptPlayable;
		}

		internal Playable CreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, Playable timelinePlayable)
		{
			Playable playable = Playable.Null;
			if (CanCompileClipsRecursive())
			{
				UpdateDuration();
				playable = OnCreateClipPlayableGraph(graph, go, tree);
			}
			Playable playable2 = CreateNotificationsPlayable(graph, playable, go, timelinePlayable);
			if (!playable2.IsValid() && !playable.IsValid())
			{
				Debug.LogErrorFormat("Track {0} of type {1} has no notifications and returns an invalid mixer Playable", base.name, GetType().FullName);
				return Playable.Create(graph);
			}
			Playable playable3 = (CanCompileNotifications() ? playable2 : playable);
			ConfigureTrackAnimation(tree, go, playable3);
			return playable3;
		}

		internal virtual Playable OnCreateClipPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			if (tree == null)
			{
				throw new ArgumentException("IntervalTree argument cannot be null", "tree");
			}
			if (go == null)
			{
				throw new ArgumentException("GameObject argument cannot be null", "go");
			}
			Playable playable = CreateTrackMixer(graph, go, clips.Length);
			for (int i = 0; i < clips.Length; i++)
			{
				Playable playable2 = CreatePlayable(graph, go, clips[i]);
				if (playable2.IsValid())
				{
					playable2.SetDuration(clips[i].duration);
					RuntimeClip item = new RuntimeClip(clips[i], playable2, playable);
					tree.Add(item);
					graph.Connect(playable2, 0, playable, i);
					playable.SetInputWeight(i, 0f);
				}
			}
			return playable;
		}

		private void ConfigureTrackAnimation(IntervalTree<RuntimeElement> tree, GameObject go, Playable blend)
		{
			if (hasCurves)
			{
				blend.SetAnimatedProperties(m_Curves);
				tree.Add(new InfiniteRuntimeClip(blend));
				if (TrackAsset.OnTrackAnimationPlayableCreate != null)
				{
					TrackAsset.OnTrackAnimationPlayableCreate(this, go, blend);
				}
			}
		}

		internal void SortClips()
		{
			_ = clips;
			if (!m_CacheSorted)
			{
				Array.Sort(clips, (TimelineClip clip1, TimelineClip clip2) => clip1.start.CompareTo(clip2.start));
				m_CacheSorted = true;
			}
		}

		internal void ClearClipsInternal()
		{
			m_Clips = new List<TimelineClip>();
			m_ClipsCache = null;
		}

		internal void ClearSubTracksInternal()
		{
			m_Children = new List<ScriptableObject>();
			Invalidate();
		}

		internal void OnClipMove()
		{
			m_CacheSorted = false;
		}

		internal TimelineClip CreateNewClipContainerInternal()
		{
			TimelineClip timelineClip = new TimelineClip(this);
			timelineClip.asset = null;
			double val = 0.0;
			for (int i = 0; i < m_Clips.Count - 1; i++)
			{
				double num = m_Clips[i].duration;
				if (double.IsInfinity(num))
				{
					num = TimelineClip.kDefaultClipDurationInSeconds;
				}
				val = Math.Max(val, m_Clips[i].start + num);
			}
			timelineClip.mixInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			timelineClip.mixOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			timelineClip.start = val;
			timelineClip.duration = TimelineClip.kDefaultClipDurationInSeconds;
			timelineClip.displayName = "untitled";
			return timelineClip;
		}

		internal void AddChild(TrackAsset child)
		{
			if (!(child == null))
			{
				m_Children.Add(child);
				child.parent = this;
				Invalidate();
			}
		}

		internal void MoveLastTrackBefore(TrackAsset asset)
		{
			if (m_Children == null || m_Children.Count < 2 || asset == null)
			{
				return;
			}
			ScriptableObject scriptableObject = m_Children[m_Children.Count - 1];
			if (scriptableObject == asset)
			{
				return;
			}
			for (int i = 0; i < m_Children.Count - 1; i++)
			{
				if (m_Children[i] == asset)
				{
					for (int num = m_Children.Count - 1; num > i; num--)
					{
						m_Children[num] = m_Children[num - 1];
					}
					m_Children[i] = scriptableObject;
					Invalidate();
					break;
				}
			}
		}

		internal bool RemoveSubTrack(TrackAsset child)
		{
			if (m_Children.Remove(child))
			{
				Invalidate();
				child.parent = null;
				return true;
			}
			return false;
		}

		internal void RemoveClip(TimelineClip clip)
		{
			m_Clips.Remove(clip);
			m_ClipsCache = null;
		}

		internal virtual void GetEvaluationTime(out double outStart, out double outDuration)
		{
			outStart = double.PositiveInfinity;
			double num = double.NegativeInfinity;
			if (hasCurves)
			{
				outStart = 0.0;
				num = TimeUtility.GetAnimationClipLength(curves);
			}
			TimelineClip[] array = clips;
			foreach (TimelineClip timelineClip in array)
			{
				outStart = Math.Min(timelineClip.start, outStart);
				num = Math.Max(timelineClip.end, num);
			}
			if (HasNotifications())
			{
				double notificationDuration = GetNotificationDuration();
				outStart = Math.Min(notificationDuration, outStart);
				num = Math.Max(notificationDuration, num);
			}
			if (double.IsInfinity(outStart) || double.IsInfinity(num))
			{
				outStart = (outDuration = 0.0);
			}
			else
			{
				outDuration = num - outStart;
			}
		}

		internal virtual void GetSequenceTime(out double outStart, out double outDuration)
		{
			GetEvaluationTime(out outStart, out outDuration);
		}

		public virtual void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			GameObject gameObjectBinding = GetGameObjectBinding(director);
			if (gameObjectBinding != null)
			{
				driver.PushActiveGameObject(gameObjectBinding);
			}
			if (hasCurves)
			{
				driver.AddObjectProperties(this, m_Curves);
			}
			TimelineClip[] array = clips;
			foreach (TimelineClip timelineClip in array)
			{
				if (timelineClip.curves != null && timelineClip.asset != null)
				{
					driver.AddObjectProperties(timelineClip.asset, timelineClip.curves);
				}
				if (timelineClip.asset is IPropertyPreview propertyPreview)
				{
					propertyPreview.GatherProperties(director, driver);
				}
			}
			foreach (TrackAsset childTrack in GetChildTracks())
			{
				if (childTrack != null)
				{
					childTrack.GatherProperties(director, driver);
				}
			}
			if (gameObjectBinding != null)
			{
				driver.PopActiveGameObject();
			}
		}

		internal GameObject GetGameObjectBinding(PlayableDirector director)
		{
			if (director == null)
			{
				return null;
			}
			Object genericBinding = director.GetGenericBinding(this);
			GameObject gameObject = genericBinding as GameObject;
			if (gameObject != null)
			{
				return gameObject;
			}
			Component component = genericBinding as Component;
			if (component != null)
			{
				return component.gameObject;
			}
			return null;
		}

		internal bool ValidateClipType(Type clipType)
		{
			object[] customAttributes = GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), inherit: true);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((TrackClipTypeAttribute)customAttributes[i]).inspectedType.IsAssignableFrom(clipType))
				{
					return true;
				}
			}
			if (typeof(PlayableTrack).IsAssignableFrom(GetType()) && typeof(IPlayableAsset).IsAssignableFrom(clipType))
			{
				return typeof(ScriptableObject).IsAssignableFrom(clipType);
			}
			return false;
		}

		protected virtual void OnCreateClip(TimelineClip clip)
		{
		}

		private void UpdateDuration()
		{
			int num = CalculateItemsHash();
			if (num != m_ItemsHash)
			{
				m_ItemsHash = num;
				GetSequenceTime(out var outStart, out var outDuration);
				m_Start = (DiscreteTime)outStart;
				m_End = (DiscreteTime)(outStart + outDuration);
				this.CalculateExtrapolationTimes();
			}
		}

		protected internal virtual int CalculateItemsHash()
		{
			return HashUtility.CombineHash(GetClipsHash(), GetAnimationClipHash(m_Curves), GetTimeRangeHash());
		}

		protected virtual Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
		{
			if (!graph.IsValid())
			{
				throw new ArgumentException("graph must be a valid PlayableGraph");
			}
			if (clip == null)
			{
				throw new ArgumentNullException("clip");
			}
			if (clip.asset is IPlayableAsset playableAsset)
			{
				Playable playable = playableAsset.CreatePlayable(graph, gameObject);
				if (playable.IsValid())
				{
					playable.SetAnimatedProperties(clip.curves);
					playable.SetSpeed(clip.timeScale);
					if (TrackAsset.OnClipPlayableCreate != null)
					{
						TrackAsset.OnClipPlayableCreate(clip, gameObject, playable);
					}
				}
				return playable;
			}
			return Playable.Null;
		}

		internal void Invalidate()
		{
			m_ChildTrackCache = null;
			TimelineAsset timelineAsset = this.timelineAsset;
			if (timelineAsset != null)
			{
				timelineAsset.Invalidate();
			}
		}

		internal double GetNotificationDuration()
		{
			if (!supportsNotifications)
			{
				return 0.0;
			}
			double num = 0.0;
			foreach (IMarker marker in GetMarkers())
			{
				if (marker is INotification)
				{
					num = Math.Max(num, marker.time);
				}
			}
			return num;
		}

		internal virtual bool CanCompileClips()
		{
			if (!hasClips)
			{
				return hasCurves;
			}
			return true;
		}

		internal bool IsCompilable()
		{
			if (typeof(GroupTrack).IsAssignableFrom(GetType()))
			{
				return false;
			}
			bool flag = !mutedInHierarchy && (CanCompileClips() || CanCompileNotifications());
			if (!flag)
			{
				foreach (TrackAsset childTrack in GetChildTracks())
				{
					if (childTrack.IsCompilable())
					{
						return true;
					}
				}
				return flag;
			}
			return flag;
		}

		private void UpdateChildTrackCache()
		{
			if (m_ChildTrackCache != null)
			{
				return;
			}
			if (m_Children == null || m_Children.Count == 0)
			{
				m_ChildTrackCache = s_EmptyCache;
				return;
			}
			List<TrackAsset> list = new List<TrackAsset>(m_Children.Count);
			for (int i = 0; i < m_Children.Count; i++)
			{
				TrackAsset trackAsset = m_Children[i] as TrackAsset;
				if (trackAsset != null)
				{
					list.Add(trackAsset);
				}
			}
			m_ChildTrackCache = list;
		}

		internal virtual int Hash()
		{
			return clips.Length + (m_Markers.Count << 16);
		}

		private int GetClipsHash()
		{
			int num = 0;
			foreach (TimelineClip clip in m_Clips)
			{
				num = num.CombineHash(clip.Hash());
			}
			return num;
		}

		protected static int GetAnimationClipHash(AnimationClip clip)
		{
			int num = 0;
			if (clip != null && !clip.empty)
			{
				num = num.CombineHash(clip.frameRate.GetHashCode()).CombineHash(clip.length.GetHashCode());
			}
			return num;
		}

		private bool HasNotifications()
		{
			return m_Markers.HasNotifications();
		}

		private bool CanCompileNotifications()
		{
			if (supportsNotifications)
			{
				return m_Markers.HasNotifications();
			}
			return false;
		}

		private bool CanCompileClipsRecursive()
		{
			if (CanCompileClips())
			{
				return true;
			}
			foreach (TrackAsset childTrack in GetChildTracks())
			{
				if (childTrack.CanCompileClipsRecursive())
				{
					return true;
				}
			}
			return false;
		}
	}
}
