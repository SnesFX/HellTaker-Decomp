using System;

namespace UnityEngine.Timeline
{
	[AssetFileNameExtension("signal", new string[] { })]
	public class SignalAsset : ScriptableObject
	{
		internal static event Action<SignalAsset> OnEnableCallback;

		private void OnEnable()
		{
			if (SignalAsset.OnEnableCallback != null)
			{
				SignalAsset.OnEnableCallback(this);
			}
		}
	}
}
