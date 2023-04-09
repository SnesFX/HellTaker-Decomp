using UnityEngine;
using UnityEngine.UI;

public class pauseMenu : MonoBehaviour
{
	public Player player;

	public GameObject playerObj;

	public GameObject menu;

	public Image black;

	public GameObject hint;

	public GameObject goal;

	private GoalSprite goalScript;

	private float timescale;

	private bool keyDown;

	private bool keyDown2;

	private bool keyDown3;

	public Image[] bracketL;

	public Image[] bracketR;

	public Text[] buttonText;

	public int tempIndex;

	public int maxIndex;

	public int currentIndex;

	private Color32 greyText = new Color32(212, 185, 191, byte.MaxValue);

	private Color32 greyBracket = new Color32(101, 61, 72, byte.MaxValue);

	private Color32 redBracket = new Color32(230, 77, 81, byte.MaxValue);

	public Text[] volumeText;

	private int[] vol = new int[2];

	public GameObject restart;

	public GameObject advice;

	public Text restartText;

	public Text adviceText;

	private bool start;

	private bool adviceable = true;

	public Image code01;

	public Image code02;

	public Image code03;

	public Sprite doneCode01;

	public Sprite doneCode02;

	public Sprite doneCode03;

	public Sprite emptyCode;

	public AudioClip pickSound;

	public AudioClip confirmSound;

	public bool DLC;

	public Asset[] pillars;

	public LabScript bossHPscript;

	public int DLCChapter;

	private void Start()
	{
		if (DLCChapter > PlayerPrefs.GetInt("dlcProgress", 0))
		{
			PlayerPrefs.SetInt("dlcProgress", DLCChapter);
		}
		goalScript = goal.GetComponent<GoalSprite>();
		keyDown = (keyDown2 = (keyDown3 = false));
		currentIndex = 3;
		ButtonFeedback(currentIndex);
		menu.GetComponent<Text>().text = Manager.instance.menuTxt[4];
		if (restart.activeSelf)
		{
			restartText.text = Manager.instance.menuTxt[2];
			adviceText.text = Manager.instance.menuTxt[3];
		}
		else
		{
			adviceable = false;
		}
		for (int i = 0; i < buttonText.Length; i++)
		{
			buttonText[i].text = Manager.instance.menuTxt[i + 5];
		}
		buttonText[4].text = Manager.instance.menuTxt[9];
		if (goalScript.chapterNumber == "XX")
		{
			buttonText[4].text = Manager.instance.dlcTxt[1];
		}
		else if (bossHPscript != null)
		{
			buttonText[4].text = Manager.instance.dlcTxt[2];
		}
		vol[0] = PlayerPrefs.GetInt("musicVol", 3);
		vol[1] = PlayerPrefs.GetInt("efxVol", 3);
		volumeText[0].text = Manager.instance.menuTxt[14 + vol[0]];
		volumeText[1].text = Manager.instance.menuTxt[14 + vol[1]];
	}

	private void Update()
	{
		if (!start)
		{
			if (!player.inMenu)
			{
				start = true;
			}
		}
		else if (adviceable)
		{
			if (restart.activeSelf)
			{
				if (player.frozen || player.inMenu)
				{
					restart.SetActive(value: false);
					if (!DLC)
					{
						advice.SetActive(value: false);
					}
				}
			}
			else if (!restart.activeSelf && !player.frozen && !player.inMenu)
			{
				restart.SetActive(value: true);
				if (!DLC)
				{
					advice.SetActive(value: true);
				}
			}
		}
		if (!player.unMenuable)
		{
			if (Input.GetButton("Restart") && !player.inMenu && player.restartable)
			{
				player.frozen = true;
				player.DialogueRestart();
			}
			if (Input.GetButton("Advice") && !player.inMenu && adviceable && !player.frozen && !DLC)
			{
				hint.transform.position = playerObj.transform.position;
			}
			if (Input.GetButton("Cancel"))
			{
				if (!keyDown)
				{
					keyDown = true;
					if (!menu.activeSelf)
					{
						if (player.frozen || goalScript.chapterNumber == "VIII" || goalScript.chapterNumber == "XXI" || player.cheater)
						{
							maxIndex = 3;
						}
						else
						{
							maxIndex = 4;
						}
						ButtonFeedback(currentIndex);
						currentIndex = 3;
						tempIndex = 3;
						ButtonFeedback(currentIndex);
						if (PlayerPrefs.GetInt("Alfa", 0) == 6)
						{
							code01.sprite = doneCode01;
						}
						if (PlayerPrefs.GetInt("Beta", 0) == 6)
						{
							code02.sprite = doneCode02;
						}
						if (PlayerPrefs.GetInt("Gamma", 0) == 6)
						{
							code03.sprite = doneCode03;
						}
						player.inMenu = true;
						menu.SetActive(value: true);
						black.color += new Color(0f, 0f, 0f, 0.9f);
						timescale = Time.timeScale;
						Time.timeScale = 0f;
						Manager.instance.SinMuter();
					}
					else
					{
						Resume();
					}
				}
			}
			else
			{
				keyDown = false;
			}
		}
		if (!menu.activeSelf)
		{
			return;
		}
		if (Input.GetButton("Submit") && currentIndex > 1)
		{
			if (currentIndex == 2)
			{
				Resume(i: true);
				player.DialogueRestart(0);
			}
			else
			{
				Resume();
			}
			if (currentIndex == 4)
			{
				player.cheater = true;
				if (goalScript.chapterNumber == "XX")
				{
					player.PowerOfLove();
				}
				else if (bossHPscript != null)
				{
					bossHPscript.Skip();
				}
				else if (pillars.Length != 0)
				{
					Asset[] array = pillars;
					foreach (Asset asset in array)
					{
						if (asset.gameObject.activeSelf)
						{
							asset.Broken();
						}
					}
				}
				else
				{
					goal.transform.position = playerObj.transform.position;
				}
			}
		}
		if (Input.GetAxisRaw("Vertical") < 0f)
		{
			if (!keyDown2)
			{
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown2 = true;
				if (tempIndex < maxIndex)
				{
					currentIndex++;
				}
				else
				{
					currentIndex = 0;
				}
				ButtonFeedback(tempIndex);
				ButtonFeedback(currentIndex);
				tempIndex = currentIndex;
			}
		}
		else if (Input.GetAxisRaw("Vertical") > 0f)
		{
			if (!keyDown2)
			{
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown2 = true;
				if (tempIndex > 0)
				{
					currentIndex--;
				}
				else
				{
					currentIndex = maxIndex;
				}
				ButtonFeedback(tempIndex);
				ButtonFeedback(currentIndex);
				tempIndex = currentIndex;
			}
		}
		else
		{
			keyDown2 = false;
		}
		if (currentIndex > 1)
		{
			return;
		}
		if (Input.GetAxisRaw("Horizontal") < 0f)
		{
			if (!keyDown3)
			{
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown3 = true;
				if (vol[currentIndex] > 0)
				{
					vol[currentIndex]--;
				}
				else
				{
					vol[currentIndex] = 3;
				}
				Manager.instance.VolumeChange(currentIndex, vol[currentIndex]);
				volumeText[currentIndex].text = Manager.instance.menuTxt[14 + vol[currentIndex]];
			}
		}
		else if (Input.GetAxisRaw("Horizontal") > 0f)
		{
			if (!keyDown3)
			{
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown3 = true;
				if (vol[currentIndex] < 3)
				{
					vol[currentIndex]++;
				}
				else
				{
					vol[currentIndex] = 0;
				}
				Manager.instance.VolumeChange(currentIndex, vol[currentIndex]);
				volumeText[currentIndex].text = Manager.instance.menuTxt[14 + vol[currentIndex]];
			}
		}
		else
		{
			keyDown3 = false;
		}
	}

	public void ButtonFeedback(int i)
	{
		if (buttonText[i].color == Color.white)
		{
			if (i <= 1)
			{
				volumeText[i].color = greyText;
			}
			buttonText[i].color = greyText;
			bracketL[i].color = greyBracket;
			bracketL[i].transform.position -= new Vector3(20f, 0f, 0f);
			bracketR[i].color = greyBracket;
			bracketR[i].transform.position += new Vector3(20f, 0f, 0f);
		}
		else
		{
			if (i <= 1)
			{
				volumeText[i].color = Color.white;
			}
			buttonText[i].color = Color.white;
			bracketL[i].color = redBracket;
			bracketL[i].transform.position += new Vector3(20f, 0f, 0f);
			bracketR[i].color = redBracket;
			bracketR[i].transform.position -= new Vector3(20f, 0f, 0f);
		}
	}

	public void Resume(bool i = false)
	{
		Manager.instance.RandomizeSfx(false, confirmSound);
		menu.SetActive(value: false);
		black.color -= new Color(0f, 0f, 0f, 0.9f);
		Time.timeScale = timescale;
		player.inMenu = i;
		player.unMenuable = i;
		if (i)
		{
			player.frozen = i;
		}
		else
		{
			Manager.instance.SinMuter();
		}
	}
}
