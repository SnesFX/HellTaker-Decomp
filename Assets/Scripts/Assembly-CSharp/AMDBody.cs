using UnityEngine;

public class AMDBody : MonoBehaviour
{
	public CameraShake cam;

	public Animator[] igniterAnim;

	public int igniterNumber;

	public AudioClip hitSound1;

	public AudioClip hitSound2;

	public AudioClip hitSound3;

	public AudioClip hitSound4;

	public AudioClip hitSoundMAX;

	public AudioClip hitSoundULTIMATE;

	public AudioClip laughSound;

	public AudioClip explosionSound;

	public GameObject heart;

	public Animator loopAnimator;

	public GameObject sparks;

	public GameObject spikers;

	public Animator pomp1;

	public Animator pomp2;

	public AudioClip bossStart;

	public AudioClip bossDeath;

	public AudioClip bossFullDeath;

	public AudioClip[] bossHit;

	public AudioClip erasergunSound;

	public AudioClip erasergunSoundGreat;

	private bool audibleHands = true;

	public void Start()
	{
		Manager.instance.PlayHands(null);
	}

	public void ShakeSpawn()
	{
		if (igniterNumber != 2)
		{
			Manager.instance.RandomizeSfx(hitSound1, hitSound2, hitSound3, hitSound4);
		}
		igniterAnim[igniterNumber].SetTrigger("activate");
		cam.Shakedown(0.14f);
	}

	public void HandSound()
	{
		if (audibleHands)
		{
			Manager.instance.PlayHands(hitSoundMAX);
			audibleHands = false;
		}
		else
		{
			audibleHands = true;
		}
	}

	public void UltimateHandSound()
	{
		Manager.instance.PlayHands(hitSoundULTIMATE);
	}

	public void Laugh()
	{
		Manager.instance.PlayHands(laughSound);
	}

	public void Raise()
	{
	}

	public void EraserShake()
	{
		cam.Shakedown(0.15f);
	}

	public void EraserShakeHARD(float volume)
	{
		cam.Shakedown(volume);
	}

	public void WeakSpot()
	{
		heart.SetActive(value: true);
	}

	public void End()
	{
		loopAnimator.SetTrigger("end");
	}

	public void SparkSplash(int final = 0)
	{
		Object.Instantiate(sparks, new Vector3(2f, 4f, 0f), sparks.transform.rotation);
		Object.Instantiate(sparks, new Vector3(4f, 5f, 0f), sparks.transform.rotation);
		if (Manager.instance.AMDphase == 1)
		{
			Manager.instance.PlayAMD(bossStart);
		}
		else if (final == 1)
		{
			Manager.instance.PlayAMD(bossFullDeath);
		}
	}

	public void EraserGunSounder()
	{
		if (Manager.instance.AMDphase == 3)
		{
			Manager.instance.PlayHands(erasergunSoundGreat);
		}
		else
		{
			Manager.instance.PlayHands(erasergunSound);
		}
	}

	public void ScreenSparkSplash()
	{
		Object.Instantiate(sparks, new Vector3(1f, 7f, 0f), sparks.transform.rotation);
		Object.Instantiate(sparks, new Vector3(5f, 8f, 0f), sparks.transform.rotation);
	}

	public void Death()
	{
		Manager.instance.PlayAMD(bossDeath);
		Manager.instance.LoopMute();
		spikers.SetActive(value: false);
		Manager.instance.PlayHands(null);
	}

	public void Overdrive()
	{
		pomp1.SetTrigger("hyperActive");
		pomp2.SetTrigger("hyperActive");
		Object.Instantiate(sparks, new Vector3(-4f, 1f, 0f), sparks.transform.rotation);
		Object.Instantiate(sparks, new Vector3(10f, 0f, 0f), sparks.transform.rotation);
	}
}
