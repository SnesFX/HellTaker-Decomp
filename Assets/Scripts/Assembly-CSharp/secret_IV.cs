using UnityEngine;

public class secret_IV : MonoBehaviour
{
	public Player player;

	public GameObject inscription;

	public GameObject flames;

	private bool active;

	private void Update()
	{
		if (player.will > 0 && active && Input.GetAxisRaw("Vertical") > 0f && Manager.instance.playerTurn)
		{
			active = false;
			inscription.transform.position += new Vector3(20f, 0f, 0f);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "unBreakable" && player.will > 1)
		{
			flames.SetActive(value: true);
			active = true;
		}
		if (other.tag == "player" && player.will > 2)
		{
			flames.SetActive(value: true);
			active = true;
		}
	}
}
