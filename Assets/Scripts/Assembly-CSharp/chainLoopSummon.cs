using System.Collections;
using UnityEngine;

public class chainLoopSummon : MonoBehaviour
{
	public chainLoop[] chn;

	public GameObject chainVer;

	public GameObject chainHor;

	public bool reverse;

	public float[] allSummons;

	public float delay = 1f;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "SIN")
		{
			StartCoroutine(Loop());
		}
	}

	public IEnumerator Loop()
	{
		yield return new WaitForSeconds(0.1f);
		for (int a = 0; a < chn.Length; a++)
		{
			if (reverse)
			{
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
				for (int j = 0; j < chn[a].summons.Length; j++)
				{
					Summon(chn[a].summons[j]);
				}
			}
			yield return new WaitForSeconds(delay);
		}
	}

	public void Summon(float cord)
	{
		if (cord > 0f)
		{
			Object.Instantiate(chainVer, new Vector3(cord - 1f, base.transform.position.y + 2f), chainVer.transform.rotation);
		}
		else
		{
			Object.Instantiate(chainHor, new Vector3(base.transform.position.x - 1f, cord + base.transform.position.y + 5f), chainHor.transform.rotation);
		}
	}
}
