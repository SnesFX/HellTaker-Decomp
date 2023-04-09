using UnityEngine;

public class sinShaker : MonoBehaviour
{
	public CameraShake cam;

	public bool operational;

	public AudioClip sound;

	public void CamShaker()
	{
		if (operational)
		{
			cam.Shakedown(0.2f);
		}
	}

	public void SoundPlay()
	{
		Manager.instance.RandomizeSfx(false, sound);
	}
}
