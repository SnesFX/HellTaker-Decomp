using System.Collections;
using UnityEngine;

public class sinSpikeScript : MonoBehaviour
{
	public GameObject[] spikeCollider;

	public Transform[] spikeTransform;

	public Animator[] spikeAnimator;

	private int killer;

	private int opener;

	private int closer;

	private void Start()
	{
		spikeAnimator[1].SetBool("ascending", value: true);
		spikeAnimator[2].SetBool("ascending", value: true);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "SIN")
		{
			StartCoroutine(SpikeChange());
		}
	}

	public IEnumerator SpikeChange()
	{
		base.transform.position -= new Vector3(0f, 1f);
		spikeCollider[killer].SetActive(value: false);
		spikeCollider[killer].transform.position -= new Vector3(0f, 2f);
		spikeTransform[closer].position -= new Vector3(0f, 3f);
		opener = closer;
		closer++;
		if (closer > 2)
		{
			closer = 0;
		}
		spikeAnimator[opener].SetBool("ascending", value: true);
		spikeAnimator[closer].SetBool("ascending", value: false);
		yield return new WaitForSeconds(0.3f);
		spikeCollider[killer].SetActive(value: true);
		killer++;
		if (killer > 1)
		{
			killer = 0;
		}
	}
}
