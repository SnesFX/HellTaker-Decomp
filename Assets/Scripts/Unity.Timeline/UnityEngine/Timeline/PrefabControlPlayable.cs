using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class PrefabControlPlayable : PlayableBehaviour
	{
		private GameObject m_Instance;

		public GameObject prefabInstance => m_Instance;

		public static ScriptPlayable<PrefabControlPlayable> Create(PlayableGraph graph, GameObject prefabGameObject, Transform parentTransform)
		{
			if (prefabGameObject == null)
			{
				return ScriptPlayable<PrefabControlPlayable>.Null;
			}
			ScriptPlayable<PrefabControlPlayable> result = ScriptPlayable<PrefabControlPlayable>.Create(graph);
			result.GetBehaviour().Initialize(prefabGameObject, parentTransform);
			return result;
		}

		public GameObject Initialize(GameObject prefabGameObject, Transform parentTransform)
		{
			if (prefabGameObject == null)
			{
				throw new ArgumentNullException("Prefab cannot be null");
			}
			if (m_Instance != null)
			{
				Debug.LogWarningFormat("Prefab Control Playable ({0}) has already been initialized with a Prefab ({1}).", prefabGameObject.name, m_Instance.name);
			}
			else
			{
				m_Instance = Object.Instantiate(prefabGameObject, parentTransform, worldPositionStays: false);
				m_Instance.name = prefabGameObject.name + " [Timeline]";
				m_Instance.SetActive(value: false);
				SetHideFlagsRecursive(m_Instance);
			}
			return m_Instance;
		}

		public override void OnPlayableDestroy(Playable playable)
		{
			if ((bool)m_Instance)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(m_Instance);
				}
				else
				{
					Object.DestroyImmediate(m_Instance);
				}
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (!(m_Instance == null))
			{
				m_Instance.SetActive(value: true);
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (m_Instance != null && info.effectivePlayState == PlayState.Paused)
			{
				m_Instance.SetActive(value: false);
			}
		}

		private static void SetHideFlagsRecursive(GameObject gameObject)
		{
			gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			if (!Application.isPlaying)
			{
				gameObject.hideFlags |= HideFlags.HideInHierarchy;
			}
			foreach (Transform item in gameObject.transform)
			{
				SetHideFlagsRecursive(item.gameObject);
			}
		}
	}
}
