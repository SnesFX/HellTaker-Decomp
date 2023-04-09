using UnityEngine;

public class WaveSpike : MonoBehaviour
{
	public bool harm;

	public bool danger;

	public BoxCollider2D boxCollider2D;

	public Animator animator;

	public Player playerScript;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "spikeIgniter")
		{
			animator.SetTrigger("spike");
		}
	}
}
