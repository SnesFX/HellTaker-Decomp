using UnityEngine;

public class BombardParticles : MonoBehaviour
{
	public GameObject[] dust;

	private int randomIndex;

	public void Impact()
	{
		for (int i = 0; i < 3; i++)
		{
			randomIndex = Random.Range(0, dust.Length);
			Object.Instantiate(dust[randomIndex], new Vector3(base.transform.position.x + (float)Random.Range(-5, 5) * 0.1f, base.transform.position.y + (float)Random.Range(-5, 5) * 0.1f, 0f), dust[randomIndex].transform.rotation);
		}
	}
}
