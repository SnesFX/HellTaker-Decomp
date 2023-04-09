using System.Collections;
using UnityEngine;

public class Demon : MonoBehaviour
{
	public GameObject sign;

	public GameObject lovePlosion;

	public GameObject softGlow;

	public GameObject heartPlosion;

	public GameObject starPlosion;

	public GameObject starBuildup;

	private Animator animator;

	public void Get()
	{
		StartCoroutine(GetVfx());
	}

	public IEnumerator GetVfx()
	{
		Object.Instantiate(starBuildup, new Vector3(base.transform.position.x, base.transform.position.y + 0.2f, base.transform.position.z), base.transform.rotation);
		Object.Instantiate(softGlow, new Vector3(base.transform.position.x, base.transform.position.y + 0.2f, base.transform.position.z), base.transform.rotation).transform.parent = base.gameObject.transform;
		yield return new WaitForSeconds(0.5f);
		animator = GetComponent<Animator>();
		animator.SetTrigger("demonDestroy");
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length - 0.01f);
		Object.Instantiate(lovePlosion, new Vector3(base.transform.position.x, base.transform.position.y + 0.2f, base.transform.position.z), Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f)));
		Object.Instantiate(heartPlosion, new Vector3(base.transform.position.x, base.transform.position.y + 0.2f, base.transform.position.z), base.transform.rotation);
		Object.Instantiate(starPlosion, new Vector3(base.transform.position.x, base.transform.position.y + 0.2f, base.transform.position.z), base.transform.rotation);
		Object.Destroy(base.gameObject);
	}
}
