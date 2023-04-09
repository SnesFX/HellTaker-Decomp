using UnityEngine;

public class ritualScript : MonoBehaviour
{
	public GameObject next;

	public GameObject parent;

	private void OnTriggerEnter2D(Collider2D player)
	{
		if (player.tag == "player")
		{
			next.SetActive(value: true);
			parent.SetActive(value: false);
		}
	}
}
