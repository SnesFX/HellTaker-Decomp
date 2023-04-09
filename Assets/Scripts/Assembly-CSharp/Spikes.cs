using System.Collections;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	private Animator animator;

	public BoxCollider2D trigger;

	public GameObject victim;

	public bool spikeUp;

	public bool spikeStatic;

	private void Start()
	{
		trigger = GetComponent<BoxCollider2D>();
		animator = GetComponent<Animator>();
		if (!spikeUp)
		{
			animator.SetTrigger("spikeStartDown");
		}
	}

	public void Spike()
	{
		if (spikeUp)
		{
			if (!spikeStatic)
			{
				spikeUp = false;
				animator.SetTrigger("spikeDescend");
			}
			else
			{
				StartCoroutine(Hurt());
			}
		}
		else
		{
			spikeUp = true;
			animator.SetTrigger("spikeAscend");
			StartCoroutine(Hurt());
		}
	}

	public IEnumerator Hurt()
	{
		yield return new WaitForSeconds(0.14f);
		if (trigger.IsTouchingLayers(LayerMask.GetMask("blockingLayer")))
		{
			if (victim.tag == "breakable")
			{
				victim.GetComponent<Asset>().Broken();
			}
			else if (victim.tag == "player")
			{
				victim.GetComponent<Player>().Spiked();
			}
		}
	}
}
