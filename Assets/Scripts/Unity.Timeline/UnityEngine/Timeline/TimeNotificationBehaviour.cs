using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class TimeNotificationBehaviour : PlayableBehaviour
	{
		private struct NotificationEntry
		{
			public double time;

			public INotification payload;

			public bool notificationFired;

			public NotificationFlags flags;

			public bool triggerInEditor => (flags & NotificationFlags.TriggerInEditMode) != 0;

			public bool prewarm => (flags & NotificationFlags.Retroactive) != 0;

			public bool triggerOnce => (flags & NotificationFlags.TriggerOnce) != 0;
		}

		private readonly List<NotificationEntry> m_Notifications = new List<NotificationEntry>();

		private double m_PreviousTime;

		private bool m_NeedSortNotifications;

		private Playable m_TimeSource;

		public Playable timeSource
		{
			set
			{
				m_TimeSource = value;
			}
		}

		public static ScriptPlayable<TimeNotificationBehaviour> Create(PlayableGraph graph, double duration, DirectorWrapMode loopMode)
		{
			ScriptPlayable<TimeNotificationBehaviour> scriptPlayable = ScriptPlayable<TimeNotificationBehaviour>.Create(graph);
			scriptPlayable.SetDuration(duration);
			scriptPlayable.SetTimeWrapMode(loopMode);
			scriptPlayable.SetPropagateSetTime(value: true);
			return scriptPlayable;
		}

		public void AddNotification(double time, INotification payload, NotificationFlags flags = NotificationFlags.Retroactive)
		{
			m_Notifications.Add(new NotificationEntry
			{
				time = time,
				payload = payload,
				flags = flags
			});
			m_NeedSortNotifications = true;
		}

		public override void OnGraphStart(Playable playable)
		{
			SortNotifications();
			for (int i = 0; i < m_Notifications.Count; i++)
			{
				NotificationEntry value = m_Notifications[i];
				value.notificationFired = false;
				m_Notifications[i] = value;
			}
			m_PreviousTime = playable.GetTime();
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!playable.IsDone())
			{
				return;
			}
			SortNotifications();
			for (int i = 0; i < m_Notifications.Count; i++)
			{
				NotificationEntry e = m_Notifications[i];
				if (!e.notificationFired)
				{
					double duration = playable.GetDuration();
					if (m_PreviousTime <= e.time && e.time <= duration)
					{
						Trigger_internal(playable, info.output, ref e);
						m_Notifications[i] = e;
					}
				}
			}
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			if (info.evaluationType == FrameData.EvaluationType.Evaluate)
			{
				return;
			}
			SyncDurationWithExternalSource(playable);
			SortNotifications();
			double time = playable.GetTime();
			if (info.timeLooped)
			{
				double duration = playable.GetDuration();
				TriggerNotificationsInRange(m_PreviousTime, duration, info, playable, checkState: true);
				double num = playable.GetDuration() - m_PreviousTime;
				int num2 = (int)(((double)(info.deltaTime * info.effectiveSpeed) - num) / playable.GetDuration());
				for (int i = 0; i < num2; i++)
				{
					TriggerNotificationsInRange(0.0, duration, info, playable, checkState: false);
				}
				TriggerNotificationsInRange(0.0, time, info, playable, checkState: false);
			}
			else
			{
				double time2 = playable.GetTime();
				TriggerNotificationsInRange(m_PreviousTime, time2, info, playable, checkState: true);
			}
			for (int j = 0; j < m_Notifications.Count; j++)
			{
				NotificationEntry e = m_Notifications[j];
				if (e.notificationFired && CanRestoreNotification(e, info, time, m_PreviousTime))
				{
					Restore_internal(ref e);
					m_Notifications[j] = e;
				}
			}
			m_PreviousTime = playable.GetTime();
		}

		private void SortNotifications()
		{
			if (m_NeedSortNotifications)
			{
				m_Notifications.Sort((NotificationEntry x, NotificationEntry y) => x.time.CompareTo(y.time));
				m_NeedSortNotifications = false;
			}
		}

		private static bool CanRestoreNotification(NotificationEntry e, FrameData info, double currentTime, double previousTime)
		{
			if (e.triggerOnce)
			{
				return false;
			}
			if (info.timeLooped)
			{
				return true;
			}
			if (previousTime > currentTime)
			{
				return currentTime <= e.time;
			}
			return false;
		}

		private void TriggerNotificationsInRange(double start, double end, FrameData info, Playable playable, bool checkState)
		{
			if (!(start <= end))
			{
				return;
			}
			bool isPlaying = Application.isPlaying;
			for (int i = 0; i < m_Notifications.Count; i++)
			{
				NotificationEntry e = m_Notifications[i];
				if (!e.notificationFired || (!checkState && !e.triggerOnce))
				{
					double time = e.time;
					if (e.prewarm && time < end && (e.triggerInEditor || isPlaying))
					{
						Trigger_internal(playable, info.output, ref e);
						m_Notifications[i] = e;
					}
					else if (!(time < start) && !(time > end) && (e.triggerInEditor || isPlaying))
					{
						Trigger_internal(playable, info.output, ref e);
						m_Notifications[i] = e;
					}
				}
			}
		}

		private void SyncDurationWithExternalSource(Playable playable)
		{
			if (m_TimeSource.IsValid())
			{
				playable.SetDuration(m_TimeSource.GetDuration());
				playable.SetTimeWrapMode(m_TimeSource.GetTimeWrapMode());
			}
		}

		private static void Trigger_internal(Playable playable, PlayableOutput output, ref NotificationEntry e)
		{
			output.PushNotification(playable, e.payload);
			e.notificationFired = true;
		}

		private static void Restore_internal(ref NotificationEntry e)
		{
			e.notificationFired = false;
		}
	}
}
