using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	[NotKeyable]
	public class ControlPlayableAsset : PlayableAsset, IPropertyPreview, ITimelineClipAsset
	{
		private const int k_MaxRandInt = 10000;

		private static readonly List<PlayableDirector> k_EmptyDirectorsList = new List<PlayableDirector>(0);

		private static readonly List<ParticleSystem> k_EmptyParticlesList = new List<ParticleSystem>(0);

		[SerializeField]
		public ExposedReference<GameObject> sourceGameObject;

		[SerializeField]
		public GameObject prefabGameObject;

		[SerializeField]
		public bool updateParticle = true;

		[SerializeField]
		public uint particleRandomSeed;

		[SerializeField]
		public bool updateDirector = true;

		[SerializeField]
		public bool updateITimeControl = true;

		[SerializeField]
		public bool searchHierarchy = true;

		[SerializeField]
		public bool active = true;

		[SerializeField]
		public ActivationControlPlayable.PostPlaybackState postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;

		private PlayableAsset m_ControlDirectorAsset;

		private double m_Duration = PlayableBinding.DefaultDuration;

		private bool m_SupportLoop;

		private static HashSet<PlayableDirector> s_ProcessedDirectors = new HashSet<PlayableDirector>();

		private static HashSet<GameObject> s_CreatedPrefabs = new HashSet<GameObject>();

		internal bool controllingDirectors { get; private set; }

		internal bool controllingParticles { get; private set; }

		public override double duration => m_Duration;

		public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | (m_SupportLoop ? ClipCaps.Looping : ClipCaps.None);

		public void OnEnable()
		{
			if (particleRandomSeed == 0)
			{
				particleRandomSeed = (uint)Random.Range(1, 10000);
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			if (prefabGameObject != null)
			{
				if (s_CreatedPrefabs.Contains(prefabGameObject))
				{
					Debug.LogWarningFormat("Control Track Clip ({0}) is causing a prefab to instantiate itself recursively. Aborting further instances.", base.name);
					return Playable.Create(graph);
				}
				s_CreatedPrefabs.Add(prefabGameObject);
			}
			Playable playable = Playable.Null;
			List<Playable> list = new List<Playable>();
			GameObject gameObject = sourceGameObject.Resolve(graph.GetResolver());
			if (prefabGameObject != null)
			{
				Transform parentTransform = ((gameObject != null) ? gameObject.transform : null);
				ScriptPlayable<PrefabControlPlayable> scriptPlayable = PrefabControlPlayable.Create(graph, prefabGameObject, parentTransform);
				gameObject = scriptPlayable.GetBehaviour().prefabInstance;
				list.Add(scriptPlayable);
			}
			m_Duration = PlayableBinding.DefaultDuration;
			m_SupportLoop = false;
			controllingParticles = false;
			controllingDirectors = false;
			if (gameObject != null)
			{
				IList<PlayableDirector> list3;
				if (!updateDirector)
				{
					IList<PlayableDirector> list2 = k_EmptyDirectorsList;
					list3 = list2;
				}
				else
				{
					list3 = GetComponent<PlayableDirector>(gameObject);
				}
				IList<PlayableDirector> directors = list3;
				IList<ParticleSystem> list5;
				if (!updateParticle)
				{
					IList<ParticleSystem> list4 = k_EmptyParticlesList;
					list5 = list4;
				}
				else
				{
					list5 = GetParticleSystemRoots(gameObject);
				}
				IList<ParticleSystem> particleSystems = list5;
				UpdateDurationAndLoopFlag(directors, particleSystems);
				PlayableDirector component = go.GetComponent<PlayableDirector>();
				if (component != null)
				{
					m_ControlDirectorAsset = component.playableAsset;
				}
				if (go == gameObject && prefabGameObject == null)
				{
					Debug.LogWarningFormat("Control Playable ({0}) is referencing the same PlayableDirector component than the one in which it is playing.", base.name);
					active = false;
					if (!searchHierarchy)
					{
						updateDirector = false;
					}
				}
				if (active)
				{
					CreateActivationPlayable(gameObject, graph, list);
				}
				if (updateDirector)
				{
					SearchHierarchyAndConnectDirector(directors, graph, list, prefabGameObject != null);
				}
				if (updateParticle)
				{
					SearchHiearchyAndConnectParticleSystem(particleSystems, graph, list);
				}
				if (updateITimeControl)
				{
					SearchHierarchyAndConnectControlableScripts(GetControlableScripts(gameObject), graph, list);
				}
				playable = ConnectPlayablesToMixer(graph, list);
			}
			if (prefabGameObject != null)
			{
				s_CreatedPrefabs.Remove(prefabGameObject);
			}
			if (!playable.IsValid())
			{
				playable = Playable.Create(graph);
			}
			return playable;
		}

		private static Playable ConnectPlayablesToMixer(PlayableGraph graph, List<Playable> playables)
		{
			Playable playable = Playable.Create(graph, playables.Count);
			for (int i = 0; i != playables.Count; i++)
			{
				ConnectMixerAndPlayable(graph, playable, playables[i], i);
			}
			playable.SetPropagateSetTime(value: true);
			return playable;
		}

		private void CreateActivationPlayable(GameObject root, PlayableGraph graph, List<Playable> outplayables)
		{
			ScriptPlayable<ActivationControlPlayable> scriptPlayable = ActivationControlPlayable.Create(graph, root, postPlayback);
			if (scriptPlayable.IsValid())
			{
				outplayables.Add(scriptPlayable);
			}
		}

		private void SearchHiearchyAndConnectParticleSystem(IEnumerable<ParticleSystem> particleSystems, PlayableGraph graph, List<Playable> outplayables)
		{
			foreach (ParticleSystem particleSystem in particleSystems)
			{
				if (particleSystem != null)
				{
					controllingParticles = true;
					outplayables.Add(ParticleControlPlayable.Create(graph, particleSystem, particleRandomSeed));
				}
			}
		}

		private void SearchHierarchyAndConnectDirector(IEnumerable<PlayableDirector> directors, PlayableGraph graph, List<Playable> outplayables, bool disableSelfReferences)
		{
			foreach (PlayableDirector director in directors)
			{
				if (director != null)
				{
					if (director.playableAsset != m_ControlDirectorAsset)
					{
						outplayables.Add(DirectorControlPlayable.Create(graph, director));
						controllingDirectors = true;
					}
					else if (disableSelfReferences)
					{
						director.enabled = false;
					}
				}
			}
		}

		private static void SearchHierarchyAndConnectControlableScripts(IEnumerable<MonoBehaviour> controlableScripts, PlayableGraph graph, List<Playable> outplayables)
		{
			foreach (MonoBehaviour controlableScript in controlableScripts)
			{
				outplayables.Add(TimeControlPlayable.Create(graph, (ITimeControl)controlableScript));
			}
		}

		private static void ConnectMixerAndPlayable(PlayableGraph graph, Playable mixer, Playable playable, int portIndex)
		{
			graph.Connect(playable, 0, mixer, portIndex);
			mixer.SetInputWeight(playable, 1f);
		}

		internal IList<T> GetComponent<T>(GameObject gameObject)
		{
			List<T> list = new List<T>();
			if (gameObject != null)
			{
				if (searchHierarchy)
				{
					gameObject.GetComponentsInChildren(includeInactive: true, list);
				}
				else
				{
					gameObject.GetComponents(list);
				}
			}
			return list;
		}

		private static IEnumerable<MonoBehaviour> GetControlableScripts(GameObject root)
		{
			if (root == null)
			{
				yield break;
			}
			MonoBehaviour[] componentsInChildren = root.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in componentsInChildren)
			{
				if (monoBehaviour is ITimeControl)
				{
					yield return monoBehaviour;
				}
			}
		}

		internal void UpdateDurationAndLoopFlag(IList<PlayableDirector> directors, IList<ParticleSystem> particleSystems)
		{
			if (directors.Count == 0 && particleSystems.Count == 0)
			{
				return;
			}
			double num = double.NegativeInfinity;
			bool flag = false;
			foreach (PlayableDirector director in directors)
			{
				if (director.playableAsset != null)
				{
					double num2 = director.playableAsset.duration;
					if (director.playableAsset is TimelineAsset && num2 > 0.0)
					{
						num2 = (double)((DiscreteTime)num2).OneTickAfter();
					}
					num = Math.Max(num, num2);
					flag = flag || director.extrapolationMode == DirectorWrapMode.Loop;
				}
			}
			foreach (ParticleSystem particleSystem in particleSystems)
			{
				num = Math.Max(num, particleSystem.main.duration);
				flag = flag || particleSystem.main.loop;
			}
			m_Duration = (double.IsNegativeInfinity(num) ? PlayableBinding.DefaultDuration : num);
			m_SupportLoop = flag;
		}

		private IList<ParticleSystem> GetParticleSystemRoots(GameObject go)
		{
			if (searchHierarchy)
			{
				List<ParticleSystem> list = new List<ParticleSystem>();
				GetParticleSystemRoots(go.transform, list);
				return list;
			}
			return GetComponent<ParticleSystem>(go);
		}

		private static void GetParticleSystemRoots(Transform t, ICollection<ParticleSystem> roots)
		{
			ParticleSystem component = t.GetComponent<ParticleSystem>();
			if (component != null)
			{
				roots.Add(component);
				return;
			}
			for (int i = 0; i < t.childCount; i++)
			{
				GetParticleSystemRoots(t.GetChild(i), roots);
			}
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			if (director == null || s_ProcessedDirectors.Contains(director))
			{
				return;
			}
			s_ProcessedDirectors.Add(director);
			GameObject gameObject = sourceGameObject.Resolve(director);
			if (gameObject != null)
			{
				if (updateParticle)
				{
					ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
					foreach (ParticleSystem particleSystem in componentsInChildren)
					{
						driver.AddFromName<ParticleSystem>(particleSystem.gameObject, "randomSeed");
						driver.AddFromName<ParticleSystem>(particleSystem.gameObject, "autoRandomSeed");
					}
				}
				if (active)
				{
					driver.AddFromName(gameObject, "m_IsActive");
				}
				if (updateITimeControl)
				{
					foreach (MonoBehaviour controlableScript in GetControlableScripts(gameObject))
					{
						if (controlableScript is IPropertyPreview propertyPreview)
						{
							propertyPreview.GatherProperties(director, driver);
						}
						else
						{
							driver.AddFromComponent(controlableScript.gameObject, controlableScript);
						}
					}
				}
				if (updateDirector)
				{
					foreach (PlayableDirector item in GetComponent<PlayableDirector>(gameObject))
					{
						if (!(item == null))
						{
							TimelineAsset timelineAsset = item.playableAsset as TimelineAsset;
							if (!(timelineAsset == null))
							{
								timelineAsset.GatherProperties(item, driver);
							}
						}
					}
				}
			}
			s_ProcessedDirectors.Remove(director);
		}
	}
}
