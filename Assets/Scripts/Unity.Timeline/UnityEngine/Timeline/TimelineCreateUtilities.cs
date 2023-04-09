using System;
using System.Collections.Generic;

namespace UnityEngine.Timeline
{
	internal static class TimelineCreateUtilities
	{
		public static string GenerateUniqueActorName(List<ScriptableObject> tracks, string name)
		{
			if (!tracks.Exists((ScriptableObject x) => (object)x != null && x.name == name))
			{
				return name;
			}
			int result2 = 0;
			string text = name;
			if (!string.IsNullOrEmpty(name) && name[name.Length - 1] == ')')
			{
				int num = name.LastIndexOf('(');
				if (num > 0 && int.TryParse(name.Substring(num + 1, name.Length - num - 2), out result2))
				{
					result2++;
					text = name.Substring(0, num);
				}
			}
			text = text.TrimEnd();
			for (int i = result2; i < result2 + 5000; i++)
			{
				if (i > 0)
				{
					string result = $"{text} ({i})";
					if (!tracks.Exists((ScriptableObject x) => (object)x != null && x.name == result))
					{
						return result;
					}
				}
			}
			return name;
		}

		public static void SaveAssetIntoObject(Object childAsset, Object masterAsset)
		{
			if (!(childAsset == null) && !(masterAsset == null))
			{
				if ((masterAsset.hideFlags & HideFlags.DontSave) != 0)
				{
					childAsset.hideFlags |= HideFlags.DontSave;
				}
				else
				{
					childAsset.hideFlags |= HideFlags.HideInHierarchy;
				}
			}
		}

		public static AnimationClip CreateAnimationClipForTrack(string name, TrackAsset track, bool isLegacy)
		{
			TimelineAsset timelineAsset = ((track != null) ? track.timelineAsset : null);
			HideFlags hideFlags = ((track != null) ? track.hideFlags : HideFlags.None);
			AnimationClip obj = new AnimationClip
			{
				legacy = isLegacy,
				name = name,
				frameRate = ((timelineAsset == null) ? TimelineAsset.EditorSettings.kDefaultFps : timelineAsset.editorSettings.fps)
			};
			SaveAssetIntoObject(obj, timelineAsset);
			obj.hideFlags = hideFlags & ~HideFlags.HideInHierarchy;
			return obj;
		}

		internal static bool ValidateParentTrack(TrackAsset parent, Type childType)
		{
			if (childType == null || !typeof(TrackAsset).IsAssignableFrom(childType))
			{
				return false;
			}
			if (parent == null)
			{
				return true;
			}
			if (!(Attribute.GetCustomAttribute(parent.GetType(), typeof(SupportsChildTracksAttribute)) is SupportsChildTracksAttribute supportsChildTracksAttribute))
			{
				return false;
			}
			if (supportsChildTracksAttribute.childType == null)
			{
				return true;
			}
			if (childType == supportsChildTracksAttribute.childType)
			{
				int num = 0;
				TrackAsset trackAsset = parent;
				while (trackAsset != null && trackAsset.isSubTrack)
				{
					num++;
					trackAsset = trackAsset.parent as TrackAsset;
				}
				return num < supportsChildTracksAttribute.levels;
			}
			return false;
		}
	}
}
