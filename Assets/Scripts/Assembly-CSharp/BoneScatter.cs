using UnityEngine;

public class BoneScatter : MonoBehaviour
{
	public bool spark;

	private void Start()
	{
		if (!spark)
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-40f, 40f), Random.Range(40f, 120f)));
			GetComponent<Rigidbody2D>().AddTorque(300f);
		}
		else
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-40f, 40f), Random.Range(30f, 90f)));
			Object.Destroy(base.gameObject, Random.Range(0.4f, 0.6f));
		}
	}
}
