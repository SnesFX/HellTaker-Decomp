using UnityEngine;

public class fireIgniter : MonoBehaviour
{
	public Animator torch0;

	public Animator torch1;

	private void OnTriggerEnter2D(Collider2D other)
	{
		torch0.SetBool("lit", value: true);
		torch1.SetBool("lit", value: true);
	}
}
