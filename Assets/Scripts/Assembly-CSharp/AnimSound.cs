using UnityEngine;

public class AnimSound : MonoBehaviour
{
	public AudioClip singleClip;

	public AudioClip singleClip2;

	public AudioClip singleClip3;

	public AudioClip fourClip1;

	public AudioClip fourClip2;

	public AudioClip fourClip3;

	public AudioClip fourClip4;

	public AudioClip twoClips1;

	public AudioClip twoClips2;

	public bool operational = true;

	public void singlePlay(int i = 0)
	{
		switch (i)
		{
		case 0:
			Manager.instance.RandomizeSfx(false, singleClip);
			break;
		case 2:
			Manager.instance.RandomizeSfx(false, singleClip2);
			break;
		case 3:
			Manager.instance.RandomizeSfx(false, singleClip3);
			break;
		}
	}

	public void twoPlay()
	{
		Manager.instance.RandomizeSfx(false, twoClips1, twoClips2);
	}

	public void fourPlay()
	{
		Manager.instance.RandomizeSfx(fourClip1, fourClip2, fourClip3, fourClip4);
	}

	public void sinMachineOn()
	{
		if (operational)
		{
			Manager.instance.PlaySin(fourClip1);
		}
	}

	public void sinMachineOff()
	{
		if (operational)
		{
			Manager.instance.PlaySin(fourClip2);
		}
	}
}
