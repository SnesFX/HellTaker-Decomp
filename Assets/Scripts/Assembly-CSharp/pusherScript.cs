using UnityEngine;

public class pusherScript : MonoBehaviour
{
	public GameObject player;

	private void Update()
	{
		base.transform.position = new Vector3(base.transform.position.x, player.transform.position.y, base.transform.position.z);
	}
}
