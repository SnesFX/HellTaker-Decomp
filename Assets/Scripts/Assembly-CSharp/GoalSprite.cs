using System.Collections;
using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class GoalSprite : MonoBehaviour
{
	public Sprite backgroundSprite;

	public SpriteRenderer background;

	public GameObject widget;

	public string chapterNumber;

	public int maxIndex;

	public Player playerScript;

	private bool waitingForInput;

	private bool waitingForChoice;

	private int nextPart;

	private GameObject currentTalker;

	private bool keyDown;

	private bool submitBlock;

	public bool firstDialogue = true;

	private bool widgetExit;

	public Text nameText;

	public Text dialogueText;

	public GameObject booper;

	public GameObject bgMask;

	public SpriteRenderer blackoutMask;

	public Animator textAnimator;

	private GameObject success;

	public Image statue01;

	public Image statue02;

	public Text willText;

	public Text chapterText;

	private Animator UIanimator;

	public Animator[] buttonAnimatorArray;

	public Text[] buttonTextArray;

	public int[] buttonEffectArray;

	private int dialogueIndex;

	private int tempIndex;

	public Demon[] demons;

	public SpriteRenderer demonAlpha;

	public GameObject demonLanding;

	public dialogueElement[] dialogueArray;

	public string[] txt;

	public string txtDir;

	private bool triggered;

	public AudioClip pickSound;

	public AudioClip confirmSound;

	public AudioClip successSound;

	public AudioClip startSound;

	public AudioClip boopSound;

	public AudioClip heartbreakSound;

	public AudioClip abbysTrack;

	private bool confirmSoundneeded;

	public GameObject nextDialogue;

	public int epilogueNumber;

	public string ritualNumber;

	private bool songBegins;

	private bool achiev;

	public bool started;

	public bool cheatproof = true;

	private void Start()
	{
		txt = File.ReadAllLines(Manager.instance.directory + txtDir);
		keyDown = (submitBlock = false);
		waitingForInput = false;
		waitingForChoice = false;
		if (chapterNumber != "")
		{
			chapterText.text = chapterNumber;
		}
		GetComponent<SpriteRenderer>().enabled = false;
		UIanimator = playerScript.perishableUI.GetComponent<Animator>();
	}

	private void Update()
	{
		if (!playerScript.inMenu)
		{
			if (waitingForInput)
			{
				if (Input.GetButton("Submit"))
				{
					if (submitBlock)
					{
						return;
					}
					submitBlock = true;
					waitingForInput = false;
					if (success != null)
					{
						success.SetActive(value: false);
						success = null;
					}
					if (nextPart < 100)
					{
						StartCoroutine(Dialogue(nextPart));
						return;
					}
					if (nextPart > 1000)
					{
						songBegins = true;
						StartCoroutine(Dialogue(nextPart - 1000));
						return;
					}
					Manager.instance.RandomizeSfx(false, boopSound);
					if (nextPart == 101)
					{
						playerScript.DialogueRestart(1);
						return;
					}
					if (nextPart == 102)
					{
						playerScript.DialogueRestart(0);
						return;
					}
					if (nextPart == 103)
					{
						Application.Quit();
						return;
					}
					if (nextPart == 104)
					{
						playerScript.DialogueRestart(-1);
						return;
					}
					if (nextPart == 105)
					{
						playerScript.DialogueRestart(19);
						return;
					}
					if (nextPart == 666)
					{
						playerScript.DialogueRestart();
						return;
					}
					if (nextPart == 555)
					{
						widget.SetActive(value: true);
						booper.SetActive(value: false);
						return;
					}
					dialogueText.text = null;
					nameText.text = null;
					if (currentTalker != null)
					{
						Object.Destroy(currentTalker);
					}
					booper.SetActive(value: false);
					StartCoroutine(OpenMask(0f));
				}
				else
				{
					submitBlock = false;
				}
			}
			else
			{
				if (!waitingForChoice)
				{
					return;
				}
				if (Input.GetButton("Submit"))
				{
					if (!submitBlock)
					{
						confirmSoundneeded = true;
						submitBlock = true;
						waitingForChoice = false;
						for (int i = 0; i < buttonAnimatorArray.Length; i++)
						{
							buttonAnimatorArray[i].SetTrigger("hidButton");
						}
						if (buttonEffectArray[dialogueIndex] < 100)
						{
							StartCoroutine(Dialogue(buttonEffectArray[dialogueIndex]));
						}
						else if (buttonEffectArray[dialogueIndex] == 101)
						{
							widget.SetActive(value: true);
							Manager.instance.RandomizeSfx(false, confirmSound);
							confirmSoundneeded = false;
						}
					}
				}
				else if (Input.GetAxis("Vertical") != 0f)
				{
					if (keyDown)
					{
						return;
					}
					if (Input.GetAxis("Vertical") < 0f)
					{
						if (tempIndex < maxIndex)
						{
							dialogueIndex++;
						}
						else
						{
							dialogueIndex = 0;
						}
						ChoiceChange();
					}
					else
					{
						if (tempIndex > 0)
						{
							dialogueIndex--;
						}
						else
						{
							dialogueIndex = maxIndex;
						}
						ChoiceChange();
					}
				}
				else
				{
					keyDown = (submitBlock = false);
				}
			}
		}
		else
		{
			submitBlock = true;
		}
	}

	private void ChoiceChange()
	{
		Manager.instance.RandomizeSfx(false, pickSound);
		buttonAnimatorArray[tempIndex].SetBool("activeAnim", value: false);
		buttonAnimatorArray[dialogueIndex].SetBool("activeAnim", value: true);
		tempIndex = dialogueIndex;
		keyDown = true;
	}

	private void OnTriggerEnter2D(Collider2D player)
	{
		if (player.tag == "player" && !triggered)
		{
			triggered = true;
			if (dialogueArray[0].animationTime == 5.55f)
			{
				Manager.instance.mute = 0.1f;
				Manager.instance.VolumeChange(0, PlayerPrefs.GetInt("musicVol", 2));
			}
			playerScript.frozen = true;
			background.sprite = backgroundSprite;
			StartCoroutine(OpenMask(1f));
			Manager.instance.RandomizeSfx(false, startSound);
			firstDialogue = true;
			if (chapterNumber == "x")
			{
				StartCoroutine(Dialogue(8));
			}
			else if (chapterNumber == "xxx")
			{
				StartCoroutine(Dialogue(10));
			}
			else if (chapterNumber == "dlcIntro" && PlayerPrefs.GetInt("dlcProgress", 0) != 0)
			{
				StartCoroutine(Dialogue(2));
			}
			else
			{
				StartCoroutine(Dialogue(0));
			}
		}
	}

	private IEnumerator OpenMask(float end)
	{
		if (cheatproof)
		{
			playerScript.cheater = false;
		}
		if (epilogueNumber == 1 && end == 0f)
		{
			playerScript.EndingCancel();
		}
		else if (nextPart == 888 && end == 0f)
		{
			base.transform.position += new Vector3(20f, 0f, 0f);
		}
		Vector3 goal = new Vector3(1f, end, 1f);
		Color goalAlpha = new Color(blackoutMask.color.r, blackoutMask.color.g, blackoutMask.color.b, end);
		float sqrRemainingDistance = (bgMask.transform.localScale - goal).sqrMagnitude;
		if (UIanimator != null && UIanimator.gameObject.activeSelf)
		{
			if (end == 0f)
			{
				UIanimator.SetTrigger("bring");
			}
			else
			{
				UIanimator.SetTrigger("clear");
			}
		}
		while (sqrRemainingDistance > 0.0001f)
		{
			float t = 0.15f * Time.deltaTime * 60f;
			Vector3 localScale = Vector3.Lerp(bgMask.transform.localScale, goal, t);
			Color color = Color.Lerp(blackoutMask.color, goalAlpha, t);
			bgMask.transform.localScale = localScale;
			blackoutMask.color = color;
			sqrRemainingDistance = (bgMask.transform.localScale - goal).sqrMagnitude;
			yield return null;
		}
		bgMask.transform.localScale = goal;
		blackoutMask.color = goalAlpha;
		triggered = false;
		if (nextPart == 777)
		{
			for (int i = 0; i < demons.Length; i++)
			{
				demons[i].Get();
			}
			if (Manager.instance.steam && SteamManager.Initialized)
			{
				SteamUserStats.SetAchievement("achiev_00");
				SteamUserStats.StoreStats();
			}
			playerScript.Victory();
		}
		else if (nextPart == 888 && end == 0f)
		{
			if (nextDialogue != null)
			{
				nextDialogue.transform.position = base.transform.position - new Vector3(20f, 0f, 0f);
			}
			if (epilogueNumber == 666)
			{
				PlayerPrefs.SetInt(ritualNumber, 6);
			}
			if (nextDialogue == null)
			{
				playerScript.frozen = false;
			}
		}
		else if (demons.Length != 0 && epilogueNumber != 1)
		{
			for (int j = 0; j < demons.Length; j++)
			{
				demons[j].sign.SetActive(value: false);
			}
		}
	}

	private IEnumerator Dialogue(int a)
	{
		if (firstDialogue)
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (dialogueArray[a].success != null)
		{
			Manager.instance.RandomizeSfx(false, successSound);
		}
		else if (confirmSoundneeded)
		{
			Manager.instance.RandomizeSfx(false, confirmSound);
		}
		else if (dialogueArray[a].name == 666)
		{
			Manager.instance.RandomizeSfx(false, heartbreakSound);
		}
		else if (!firstDialogue && !widgetExit)
		{
			Manager.instance.RandomizeSfx(false, boopSound);
		}
		confirmSoundneeded = false;
		widgetExit = false;
		firstDialogue = false;
		if (dialogueArray[a].achiev != "" && !playerScript.cheater && Manager.instance.steam && SteamManager.Initialized)
		{
			SteamUserStats.SetAchievement(dialogueArray[a].achiev);
			SteamUserStats.StoreStats();
		}
		if (dialogueArray[a].newChara != null)
		{
			if (dialogueArray[a].name == 666)
			{
				playerScript.restartable = false;
				playerScript.cam.Shakedown(0.04f, 0.3f);
				dialogueText.fontStyle = FontStyle.Bold;
				dialogueText.color = new Color32(206, 20, 16, byte.MaxValue);
				booper.SetActive(value: false);
			}
			dialogueText.text = null;
			nameText.text = null;
			if (currentTalker != null)
			{
				Object.Destroy(currentTalker);
			}
			currentTalker = Object.Instantiate(dialogueArray[a].newChara);
			currentTalker.GetComponent<SpriteRenderer>().sprite = dialogueArray[a].newSprite;
			if ((double)dialogueArray[a].animationTime > 0.1)
			{
				if (booper.activeSelf)
				{
					booper.SetActive(value: false);
				}
				yield return new WaitForSeconds(dialogueArray[a].animationTime);
			}
			else if (a == 0)
			{
				yield return new WaitForSeconds(0.2f);
			}
		}
		else if (dialogueArray[a].newSprite != null)
		{
			currentTalker.GetComponent<SpriteRenderer>().sprite = dialogueArray[a].newSprite;
		}
		if (dialogueArray[a].animationTime == 0.01f)
		{
			Manager.instance.musicSource.clip = null;
		}
		else if (dialogueArray[a].animationTime == 0.02f)
		{
			playerScript.cam.Shakedown(0.14f, 0.3f);
		}
		if (dialogueArray[a].talkText[0] != -1)
		{
			if (dialogueArray[a].name != 666)
			{
				if (!booper.activeSelf)
				{
					booper.SetActive(value: true);
				}
				else
				{
					booper.GetComponent<Animator>().SetTrigger("clickBooper");
				}
				if (dialogueArray[a].name != -1 && dialogueArray[a].name != 666)
				{
					nameText.text = txt[dialogueArray[a].name];
				}
			}
			if (dialogueArray[a].success != null)
			{
				success = dialogueArray[a].success;
				success.SetActive(value: true);
			}
			if (dialogueArray[a].talkText.Length > 1)
			{
				dialogueText.text = txt[dialogueArray[a].talkText[0]] + "\n" + txt[dialogueArray[a].talkText[1]];
			}
			else
			{
				dialogueText.text = txt[dialogueArray[a].talkText[0]];
			}
			textAnimator.SetTrigger("textChange");
			nextPart = dialogueArray[a].outcome[0];
			yield return new WaitForSeconds(0.2f);
			waitingForInput = true;
		}
		else
		{
			booper.SetActive(value: false);
			if (buttonAnimatorArray.Length > 2)
			{
				dialogueText.text = "";
				nameText.text = txt[dialogueArray[a].name];
			}
			dialogueIndex = 0;
			tempIndex = 0;
			for (int i = 0; i < buttonAnimatorArray.Length; i++)
			{
				buttonAnimatorArray[i].SetTrigger("startAnim");
				buttonAnimatorArray[i].SetBool("activeAnim", value: false);
				buttonTextArray[i].text = txt[dialogueArray[a].oneliner[i]];
				buttonEffectArray[i] = dialogueArray[a].outcome[i];
			}
			buttonAnimatorArray[0].SetBool("activeAnim", value: true);
			yield return new WaitForSeconds(0.4f);
			waitingForChoice = true;
		}
		if (songBegins)
		{
			Manager.instance.SongChange(abbysTrack);
			songBegins = false;
		}
		yield return null;
	}

	public void BackFromWidget(int i)
	{
		widgetExit = true;
		StartCoroutine(Dialogue(i));
	}

	public void AfterChains(float i = 0.5f)
	{
		StartCoroutine(BrokenChains(i));
	}

	public IEnumerator BrokenChains(float i)
	{
		yield return new WaitForSeconds(i);
		demonLanding.SetActive(value: true);
		yield return new WaitForSeconds(i);
		if (!playerScript.frozen)
		{
			base.transform.position = playerScript.rb2D.position;
		}
		yield return new WaitForSeconds(i);
		demonAlpha.color = new Color(1f, 1f, 1f, 1f);
		demonLanding.SetActive(value: false);
	}

	public void AfterPillars(float i = 0.5f)
	{
		StartCoroutine(BrokenPillars(i));
	}

	public IEnumerator BrokenPillars(float i)
	{
		if (!playerScript.frozen)
		{
			playerScript.frozen = true;
			yield return new WaitForSeconds(i);
			base.transform.position = playerScript.rb2D.position;
		}
	}

	public void AfterAMD()
	{
		StartCoroutine(BrokenAMD());
		Manager.instance.SongChange(null);
	}

	public IEnumerator BrokenAMD()
	{
		yield return new WaitForSeconds(5f);
		if (!playerScript.frozen)
		{
			base.transform.position = playerScript.rb2D.position;
			Manager.instance.SongChange(abbysTrack);
		}
	}
}
