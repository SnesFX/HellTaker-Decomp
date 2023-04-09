using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class TimeControlPlayable : PlayableBehaviour
	{
		private ITimeControl m_timeControl;

		private bool m_started;

		public static ScriptPlayable<TimeControlPlayable> Create(PlayableGraph graph, ITimeControl timeControl)
		{
			if (timeControl == null)
			{
				return ScriptPlayable<TimeControlPlayable>.Null;
			}
			ScriptPlayable<TimeControlPlayable> result = ScriptPlayable<TimeControlPlayable>.Create(graph);
			result.GetBehaviour().Initialize(timeControl);
			return result;
		}

		public void Initialize(ITimeControl timeControl)
		{
			m_timeControl = timeControl;
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			if (m_timeControl != null)
			{
				m_timeControl.SetTime(playable.GetTime());
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (m_timeControl != null && !m_started)
			{
				m_timeControl.OnControlTimeStart();
				m_started = true;
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (m_timeControl != null && m_started)
			{
				m_timeControl.OnControlTimeStop();
				m_started = false;
			}
		}
	}
}
