using System.Collections;
using UnityEngine;

public class sinChainScript : MonoBehaviour
{
	public GameObject chainCollider;

	public Transform chainTransform;

	public bool isHorizontal;

	public bool real = true;

	private void Start()
	{
		if (isHorizontal)
		{
			chainTransform.position += new Vector3(0f, 0.2f);
		}
		StartCoroutine(Spike());
	}

	public IEnumerator Spike()
	{
		if (real)
		{
			yield return new WaitForSeconds(0.6f);
			chainCollider.SetActive(value: true);
			yield return new WaitForSeconds(0.02f);
			chainCollider.SetActive(value: false);
			yield return new WaitForSeconds(0.3f);
			Object.Destroy(base.gameObject);
		}
		else
		{
			yield return new WaitForSeconds(0.45f);
			Object.Destroy(base.gameObject);
		}
	}
}
