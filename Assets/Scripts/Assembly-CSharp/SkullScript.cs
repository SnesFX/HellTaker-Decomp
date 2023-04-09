using UnityEngine;

public class SkullScript : MonoBehaviour
{
	public AudioClip skullSound;

	public GameObject breakVfx;

	public LabScript labScript;

	protected void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			Manager.instance.RandomizeSfx(false, skullSound);
		}
	}

	public void Explode()
	{
		Object.Instantiate(breakVfx, base.gameObject.transform.position, base.gameObject.transform.rotation);
		base.gameObject.SetActive(value: false);
	}
}
