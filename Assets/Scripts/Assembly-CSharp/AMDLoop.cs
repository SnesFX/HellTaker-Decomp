using System.Collections;
using UnityEngine;

public class AMDLoop : MonoBehaviour
{
	public int type;

	public bool activation;

	public LaserBeam[] laserScripts;

	public int[] laserShots;

	public string attack;

	public Animator amdAnimator;

	public Transform amdTransform;

	public AMDBody AMDBodyScript;

	public int[] bombs;

	public GameObject bombard;

	public AudioClip[] bombSound;

	public Animator rightTurret;

	public Animator leftTurret;

	public bool pomps;

	public Animator rightPomp;

	public Animator leftPomp;

	public GoalSprite goalSprite;

	public GameObject hpBar1;

	public GameObject hpBar2;

	public GameObject hpBar3;

	public int hpLeft;

	public GameObject fullHpBar;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!(collision.tag == "skull"))
		{
			return;
		}
		if (type == 0)
		{
			Manager.instance.LoopAMD();
			fullHpBar.SetActive(value: true);
			if (hpLeft < 3)
			{
				hpBar3.SetActive(value: false);
			}
			if (hpLeft < 2)
			{
				hpBar2.SetActive(value: false);
			}
			rightPomp.SetTrigger("active");
			leftPomp.SetTrigger("active");
			if (goalSprite.started)
			{
				amdAnimator.SetTrigger("dmg");
				return;
			}
			amdAnimator.SetTrigger("start");
			goalSprite.started = true;
			for (int i = 0; i < laserShots.Length; i++)
			{
				laserScripts[laserShots[i]].Activate();
			}
			if (activation)
			{
				rightTurret.SetTrigger("fastActive");
				leftTurret.SetTrigger("fastActive");
			}
		}
		else if (type == 1)
		{
			for (int j = 0; j < laserShots.Length; j++)
			{
				if (activation)
				{
					laserScripts[laserShots[j]].Activate();
				}
				else
				{
					laserScripts[laserShots[j]].ShotCommand();
				}
			}
		}
		else if (type == 2)
		{
			amdAnimator.SetTrigger(attack);
			if (attack == "right")
			{
				AMDBodyScript.igniterNumber = 0;
			}
			else if (attack == "left")
			{
				AMDBodyScript.igniterNumber = 1;
			}
			else
			{
				AMDBodyScript.igniterNumber = 2;
			}
		}
		else if (type == 3)
		{
			StartCoroutine(BombSound());
			for (int k = 0; k < bombs.Length; k++)
			{
				if (bombs[k] > 16)
				{
					Object.Instantiate(bombard, new Vector3((float)bombs[k] - 17.5f, -0.5f, 0f), base.transform.rotation);
				}
				else if (bombs[k] > 8)
				{
					Object.Instantiate(bombard, new Vector3((float)bombs[k] - 9.5f, 0.5f, 0f), base.transform.rotation);
				}
				else
				{
					Object.Instantiate(bombard, new Vector3((float)bombs[k] - 1.5f, 1.5f, 0f), base.transform.rotation);
				}
			}
		}
		else if (type == 4)
		{
			if (activation)
			{
				rightTurret.SetTrigger("active");
				leftTurret.SetTrigger("active");
				Object.Destroy(base.gameObject);
			}
			else
			{
				rightTurret.SetTrigger("bombard");
				leftTurret.SetTrigger("bombard");
			}
		}
		else if (type == 5)
		{
			fullHpBar.SetActive(value: false);
			amdAnimator.SetTrigger("skip");
			for (int l = 0; l < laserScripts.Length; l++)
			{
				laserScripts[l].Deactivate();
			}
			rightPomp.SetTrigger("destroy");
			leftPomp.SetTrigger("destroy");
			goalSprite.AfterAMD();
		}
	}

	public IEnumerator BombSound()
	{
		yield return new WaitForSeconds(0.6f);
		Manager.instance.RandomizeSfx(pitchBool: true, bombSound);
	}
}
