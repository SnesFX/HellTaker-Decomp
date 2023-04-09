using System.Collections;
using UnityEngine;

public class sinChainSummon : MonoBehaviour
{
	public float[] summons;

	public GameObject chainVer;

	public GameObject chainHor;

	private bool summoned;

	public bool reverse;

	public float[] allSummons;

	public Transform[] gigachains;

	public Animator[] giganimators;

	public Vector3[] chainRoot;

	public bool final;

	public GameObject bossHP;

	public bool looping;

	public chainLoop[] chn;

	public float delay = 1f;

	public float delayInit = 1f;

	public GoalSprite dialogue;

	public GameObject nextLoop;

	public AudioClip chainblinkSound1;

	public AudioClip chainblinkSound2;

	public AudioClip chainblinkSound3;

	public AudioClip chainblinkSound4;

	public AudioClip gigachainSound1;

	public AudioClip gigachainSound2;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!(other.tag == "SIN"))
		{
			return;
		}
		if (looping)
		{
			StartCoroutine(Loop());
		}
		else if (summoned)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			summoned = true;
			if (reverse)
			{
				StartCoroutine(ChainBlink());
				for (int i = 0; i < allSummons.Length; i++)
				{
					if (allSummons[i] != summons[0] && allSummons[i] != summons[1])
					{
						Summon(allSummons[i]);
					}
				}
			}
			else
			{
				StartCoroutine(ChainBlink());
				for (int j = 0; j < summons.Length; j++)
				{
					Summon(summons[j]);
				}
			}
		}
		if (final)
		{
			Manager.instance.RandomizeSfx(false, gigachainSound1, gigachainSound2);
			bossHP.SetActive(value: true);
			for (int k = 0; k < gigachains.Length; k++)
			{
				gigachains[k].position = chainRoot[k];
				giganimators[k].SetTrigger("chain_hit");
			}
		}
	}

	public IEnumerator Loop()
	{
		yield return new WaitForSeconds(delayInit);
		while (!summoned)
		{
			for (int a = 0; a < chn.Length; a++)
			{
				if (reverse)
				{
					StartCoroutine(ChainBlink());
					for (int i = 0; i < allSummons.Length; i++)
					{
						if (allSummons[i] != chn[a].summons[0] && allSummons[i] != chn[a].summons[1])
						{
							Summon(allSummons[i]);
						}
					}
				}
				else
				{
					StartCoroutine(ChainBlink());
					for (int j = 0; j < chn[a].summons.Length; j++)
					{
						Summon(chn[a].summons[j]);
					}
				}
				yield return new WaitForSeconds(delay);
			}
			if (nextLoop != null)
			{
				nextLoop.transform.position = base.transform.position;
				base.gameObject.SetActive(value: false);
				yield return new WaitForSeconds(delay);
			}
		}
	}

	public IEnumerator ChainBlink()
	{
		yield return new WaitForSeconds(0.5f);
		Manager.instance.RandomizeSfx(chainblinkSound1, chainblinkSound2, chainblinkSound3, chainblinkSound4);
	}

	public void Summon(float cord)
	{
		if (cord > 0f)
		{
			Object.Instantiate(chainVer, new Vector3(cord - 1f, base.transform.position.y + 2f), chainVer.transform.rotation);
		}
		else
		{
			Object.Instantiate(chainHor, new Vector3(base.transform.position.x + 3f, cord + base.transform.position.y + 5f), chainHor.transform.rotation);
		}
	}
}
