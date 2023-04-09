using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class ActivationControlPlayable : PlayableBehaviour
	{
		public enum PostPlaybackState
		{
			Active = 0,
			Inactive = 1,
			Revert = 2
		}

		private enum InitialState
		{
			Unset = 0,
			Active = 1,
			Inactive = 2
		}

		public GameObject gameObject;

		public PostPlaybackState postPlayback = PostPlaybackState.Revert;

		private InitialState m_InitialState;

		public static ScriptPlayable<ActivationControlPlayable> Create(PlayableGraph graph, GameObject gameObject, PostPlaybackState postPlaybackState)
		{
			if (gameObject == null)
			{
				return ScriptPlayable<ActivationControlPlayable>.Null;
			}
			ScriptPlayable<ActivationControlPlayable> result = ScriptPlayable<ActivationControlPlayable>.Create(graph);
			ActivationControlPlayable behaviour = result.GetBehaviour();
			behaviour.gameObject = gameObject;
			behaviour.postPlayback = postPlaybackState;
			return result;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: true);
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (gameObject != null && info.effectivePlayState == PlayState.Paused)
			{
				gameObject.SetActive(value: false);
			}
		}

		public override void ProcessFrame(Playable playable, FrameData info, object userData)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}

		public override void OnGraphStart(Playable playable)
		{
			if (gameObject != null && m_InitialState == InitialState.Unset)
			{
				m_InitialState = (gameObject.activeSelf ? InitialState.Active : InitialState.Inactive);
			}
		}

		public override void OnPlayableDestroy(Playable playable)
		{
			if (!(gameObject == null) && m_InitialState != 0)
			{
				switch (postPlayback)
				{
				case PostPlaybackState.Active:
					gameObject.SetActive(value: true);
					break;
				case PostPlaybackState.Inactive:
					gameObject.SetActive(value: false);
					break;
				case PostPlaybackState.Revert:
					gameObject.SetActive(m_InitialState == InitialState.Active);
					break;
				}
			}
		}
	}
}
