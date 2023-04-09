using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class secret_V : MonoBehaviour
{
	public Player player;

	public GameObject willTxt;

	public GameObject chapterTxt;

	public GameObject frames;

	public GameObject menu;

	public Image statue1;

	public Image statue2;

	public SpriteRenderer blackout;

	public Text threat1;

	public Text threat2;

	private Color shadow = new Color(1f, 1f, 1f, 0.6f);

	private Color textAlph = new Color(0f, 0f, 0f, 0.05f);

	private bool active;

	public string[] txt;

	private void Start()
	{
		txt = File.ReadAllLines(Manager.instance.directory + "\\local\\9.json");
		threat1.text = txt[1];
		threat2.text = txt[2];
	}

	private void Update()
	{
		if (player.will == 1 && active && Manager.instance.playerTurn && Input.GetAxisRaw("Vertical") > 0f)
		{
			player.will += 200;
			menu.SetActive(value: false);
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
		if (other.tag == "player" && !active)
		{
			if (player.will != 666)
			{
				active = true;
				return;
			}
			player.will = 200;
			menu.SetActive(value: false);
		}
	}

	public IEnumerator Ending()
	{
		yield return new WaitForSeconds(4f);
		while (threat1.color != Color.white)
		{
			threat1.color += textAlph;
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(4f);
		while (threat2.color != Color.white)
		{
			threat2.color += textAlph;
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(3f);
		Text text = threat1;
		string text3 = (threat2.text = "");
		text.text = text3;
		player.DialogueRestart(-1);
	}
}
