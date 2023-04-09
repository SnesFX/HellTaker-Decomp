using UnityEngine;
using UnityEngine.UI;

public class widgetScript_ch2 : MonoBehaviour
{
	public GoalSprite goalScript;

	public bool submitBlock;

	public bool keyDown;

	private int currentIndex;

	public Text score;

	public AudioClip pickSound;

	public AudioClip confirmSound;

	public bool dlc;

	public int dlcProgress;

	public string[] roman = new string[7] { "I", "II", "III", "IV", "V", "VI", "VII" };

	private void OnEnable()
	{
		submitBlock = true;
		keyDown = true;
		dlcProgress = PlayerPrefs.GetInt("dlcProgress", 0);
	}

	private void Update()
	{
		if (!goalScript.playerScript.inMenu)
		{
			if (Input.GetButton("Submit"))
			{
				if (!submitBlock)
				{
					Manager.instance.RandomizeSfx(false, confirmSound);
					if (dlc)
					{
						if (currentIndex != 0)
						{
							goalScript.playerScript.DialogueRestart(currentIndex + 21);
						}
						else
						{
							goalScript.BackFromWidget(3);
						}
					}
					else if (currentIndex == 9)
					{
						goalScript.BackFromWidget(6);
					}
					else
					{
						goalScript.BackFromWidget(5);
					}
					base.gameObject.SetActive(value: false);
				}
			}
			else
			{
				submitBlock = false;
			}
			if (Input.GetAxis("Horizontal") == 0f)
			{
				keyDown = false;
			}
			else if (Input.GetAxis("Horizontal") < 0f)
			{
				if (keyDown)
				{
					return;
				}
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown = true;
				if (!dlc)
				{
					if (currentIndex > 0)
					{
						currentIndex--;
					}
					else
					{
						currentIndex = 9;
					}
					score.text = (currentIndex + 1).ToString();
				}
				else
				{
					if (currentIndex > 0)
					{
						currentIndex--;
					}
					else
					{
						currentIndex = dlcProgress;
					}
					score.text = roman[currentIndex];
				}
			}
			else
			{
				if (!(Input.GetAxis("Horizontal") > 0f) || keyDown)
				{
					return;
				}
				Manager.instance.RandomizeSfx(false, pickSound);
				keyDown = true;
				if (!dlc)
				{
					if (currentIndex < 9)
					{
						currentIndex++;
					}
					else
					{
						currentIndex = 0;
					}
					score.text = (currentIndex + 1).ToString();
				}
				else
				{
					if (currentIndex < dlcProgress)
					{
						currentIndex++;
					}
					else
					{
						currentIndex = 0;
					}
					score.text = roman[currentIndex];
				}
			}
		}
		else
		{
			submitBlock = true;
		}
	}
}
