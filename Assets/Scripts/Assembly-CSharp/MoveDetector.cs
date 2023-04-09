using System.Collections;
using UnityEngine;

public class MoveDetector : MonoBehaviour
{
	public int count;

	public Transform talkSprite1;

	public Transform talkSprite2;

	public Transform playerSprite;

	public GameObject healthBar;

	private void Start()
	{
		StartCoroutine(Countdown(20, talkSprite1));
	}

	public IEnumerator Countdown(int number, Transform talkSprite)
	{
		while (count < number)
		{
			count++;
			yield return new WaitForSeconds(1f);
		}
		talkSprite.position = playerSprite.position;
		healthBar.SetActive(value: false);
		count = 0;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		count = 0;
		if (collision.tag == "utility")
		{
			healthBar.SetActive(value: true);
			StartCoroutine(Countdown(15, talkSprite2));
		}
	}
}
