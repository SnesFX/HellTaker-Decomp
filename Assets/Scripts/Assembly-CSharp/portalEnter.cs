using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class portalEnter : MonoBehaviour
{
	public Player playerScript;

	public SpriteRenderer blackoutMask;

	private void OnTriggerEnter2D(Collider2D player)
	{
		if (player.tag == "player")
		{
			playerScript.unMenuable = (playerScript.frozen = true);
			Manager.instance.musicSource.clip = null;
			blackoutMask.color = new Color(blackoutMask.color.r, blackoutMask.color.g, blackoutMask.color.b, 1f);
			StartCoroutine(NextLvl());
		}
	}

	private IEnumerator NextLvl()
	{
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
	}
}
