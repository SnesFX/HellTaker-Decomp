using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LabScript : MonoBehaviour
{
	public int pillarCount;

	public GoalSprite dialogue;

	public Animator holoPro;

	public GameObject laserParent;

	public GameObject skullParent;

	private LaserBeam[] lasers;

	private SkullScript[] skulls;

	public Text secs;

	public Text msecs;

	public int timeLimit;

	public Player playerScript;

	public Animator[] cogitators;

	public Animator[] aparatus;

	public AudioClip bossMusic;

	public AudioClip holoSound;

	public AudioClip genSfx;

	public Animator AMDloopAnimator;

	public Asset AMDcoreScript;

	private void Awake()
	{
		if (Manager.instance.AMDphase != 0)
		{
			AMDcoreScript.gameObject.SetActive(value: false);
			AMDloopAnimator.SetTrigger("start" + Manager.instance.AMDphase);
		}
	}

	private void Start()
	{
		lasers = laserParent.GetComponentsInChildren<LaserBeam>();
	}

	public void PillarBrk()
	{
		pillarCount--;
		Manager.instance.PlayAMD(genSfx);
		if (pillarCount == 0)
		{
			LaserBeam[] array = lasers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Deactivate();
			}
			Animator[] array2 = cogitators;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetTrigger("off");
			}
			array2 = aparatus;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
			holoPro.SetTrigger("start");
			dialogue.AfterPillars(2f);
			StartCoroutine(HoloSound());
		}
	}

	public void Skip()
	{
		AMDloopAnimator.SetTrigger("end");
		AMDcoreScript.gameObject.SetActive(value: false);
	}

	public void AMDhit()
	{
		dialogue.started = true;
		if (Manager.instance.AMDphase == 0)
		{
			Manager.instance.SongChange(bossMusic);
		}
		if (Manager.instance.AMDphase != 3)
		{
			Manager.instance.AMDphase++;
			AMDloopAnimator.SetTrigger("start" + Manager.instance.AMDphase);
		}
		else
		{
			AMDloopAnimator.SetTrigger("kill");
		}
	}

	public IEnumerator HoloSound()
	{
		yield return new WaitForSeconds(1.1f);
		Manager.instance.RandomizeSfx(false, holoSound);
	}
}
