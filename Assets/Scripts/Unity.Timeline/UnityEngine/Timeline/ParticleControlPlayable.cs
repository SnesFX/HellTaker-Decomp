using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class ParticleControlPlayable : PlayableBehaviour
	{
		private const float kUnsetTime = -1f;

		private float m_LastTime = -1f;

		private uint m_RandomSeed = 1u;

		private float m_SystemTime;

		public ParticleSystem particleSystem { get; private set; }

		public static ScriptPlayable<ParticleControlPlayable> Create(PlayableGraph graph, ParticleSystem component, uint randomSeed)
		{
			if (component == null)
			{
				return ScriptPlayable<ParticleControlPlayable>.Null;
			}
			ScriptPlayable<ParticleControlPlayable> result = ScriptPlayable<ParticleControlPlayable>.Create(graph);
			result.GetBehaviour().Initialize(component, randomSeed);
			return result;
		}

		public void Initialize(ParticleSystem ps, uint randomSeed)
		{
			m_RandomSeed = Math.Max(1u, randomSeed);
			particleSystem = ps;
			m_SystemTime = 0f;
			SetRandomSeed();
		}

		private void SetRandomSeed()
		{
			this.particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ParticleSystem[] componentsInChildren = this.particleSystem.gameObject.GetComponentsInChildren<ParticleSystem>();
			uint num = m_RandomSeed;
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				if (particleSystem.useAutoRandomSeed)
				{
					particleSystem.useAutoRandomSeed = false;
					particleSystem.randomSeed = num;
					num++;
				}
			}
		}

		public override void PrepareFrame(Playable playable, FrameData data)
		{
			if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
			{
				return;
			}
			float num = (float)playable.GetTime();
			if (!Mathf.Approximately(m_LastTime, -1f) && Mathf.Approximately(m_LastTime, num))
			{
				return;
			}
			float num2 = Time.fixedDeltaTime * 0.5f;
			float num3 = num;
			float num4 = num3 - m_LastTime;
			float num5 = particleSystem.main.startDelay.Evaluate(particleSystem.randomSeed);
			float num6 = particleSystem.main.duration + num5;
			float num7 = ((num3 > num6) ? m_SystemTime : (m_SystemTime - num5));
			if (num3 < m_LastTime || num3 < num2 || Mathf.Approximately(m_LastTime, -1f) || num4 > particleSystem.main.duration || !(Mathf.Abs(num7 - particleSystem.time) < Time.maximumParticleDeltaTime))
			{
				particleSystem.Simulate(0f, withChildren: true, restart: true);
				particleSystem.Simulate(num3, withChildren: true, restart: false);
				m_SystemTime = num3;
			}
			else
			{
				float num8 = ((num3 > num6) ? particleSystem.main.duration : num6);
				float num9 = num3 % num8;
				float num10 = num9 - m_SystemTime;
				if (num10 < 0f - num2)
				{
					num10 = num9 + num6 - m_SystemTime;
				}
				particleSystem.Simulate(num10, withChildren: true, restart: false);
				m_SystemTime += num10;
			}
			m_LastTime = num;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			m_LastTime = -1f;
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			m_LastTime = -1f;
		}
	}
}
