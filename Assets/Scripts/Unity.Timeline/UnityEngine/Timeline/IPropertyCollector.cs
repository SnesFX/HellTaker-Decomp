using System.Collections.Generic;

namespace UnityEngine.Timeline
{
	public interface IPropertyCollector
	{
		void PushActiveGameObject(GameObject gameObject);

		void PopActiveGameObject();

		void AddFromClip(AnimationClip clip);

		void AddFromClips(IEnumerable<AnimationClip> clips);

		void AddFromName<T>(string name) where T : Component;

		void AddFromName(string name);

		void AddFromClip(GameObject obj, AnimationClip clip);

		void AddFromClips(GameObject obj, IEnumerable<AnimationClip> clips);

		void AddFromName<T>(GameObject obj, string name) where T : Component;

		void AddFromName(GameObject obj, string name);

		void AddFromName(Component component, string name);

		void AddFromComponent(GameObject obj, Component component);

		void AddObjectProperties(Object obj, AnimationClip clip);
	}
}
