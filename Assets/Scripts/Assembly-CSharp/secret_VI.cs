using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class secret_VI : MonoBehaviour
{
	public Player player;

	public Transform playerTransf;

	public GameObject willTxt;

	public GameObject chapterTxt;

	public GameObject frames;

	public GameObject menu;

	public Image statue1;

	public Image statue2;

	public SpriteRenderer blackout;

	private Color shadow = new Color(1f, 1f, 1f, 0.6f);

	private Color textAlph = new Color(0f, 0f, 0f, 0.05f);

	public string[] txt;

	public GameObject finisher;

	public GameObject finisherMove;

	private void Start()
	{
		txt = File.ReadAllLines(Manager.instance.directory + "\\local\\9.json");
	}

	private void Update()
	{
		if (finisherMove != null)
		{
			finisherMove.transform.position = playerTransf.position;
		}
		if (player.will == 199 && frames.activeSelf)
		{
			Manager.instance.musicSource.clip = null;
			willTxt.SetActive(value: false);
			chapterTxt.SetActive(value: false);
			frames.SetActive(value: false);
			statue1.color = shadow;
			statue2.color = shadow;
			blackout.color += new Color(blackout.color.r, blackout.color.g, blackout.color.b, 0.2f);
			StartCoroutine(Ending());
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		player.frozen = false;
		player.will = 200;
		menu.SetActive(value: false);
	}

	public IEnumerator Ending()
	{
		while (true)
		{
			yield return new WaitForSeconds(3f);
			finisherMove = Object.Instantiate(finisher, new Vector3(playerTransf.position.x, playerTransf.position.y), finisher.transform.rotation);
			yield return new WaitForSeconds(2f);
		}
	}
}
