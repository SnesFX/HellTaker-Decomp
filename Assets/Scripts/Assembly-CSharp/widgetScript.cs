using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class widgetScript : MonoBehaviour
{
	public GoalSprite goalScript;

	public bool submitBlock;

	public bool keyDown;

	private int currentIndex = 666;

	private int tempIndex;

	public Text chapterTitle;

	public Animator[] chapterAnimator;

	public string[] chapterTxt;

	public AudioClip pickSound;

	public AudioClip confirmSound;

	private void OnEnable()
	{
		if (currentIndex == 666)
		{
			chapterTxt = File.ReadAllLines(Manager.instance.directory + "/local/m2.json");
		}
		goalScript.nameText.text = "";
		submitBlock = true;
		keyDown = true;
		currentIndex = 0;
		ChapterChange();
	}

	private void Update()
	{
		if (Input.GetButton("Cancel"))
		{
			goalScript.BackFromWidget(1);
			base.gameObject.SetActive(value: false);
		}
		if (Input.GetButton("Submit"))
		{
			if (!submitBlock)
			{
				submitBlock = true;
				Manager.instance.RandomizeSfx(false, confirmSound);
				if (currentIndex == 0)
				{
					goalScript.BackFromWidget(2);
					base.gameObject.SetActive(value: false);
				}
				else if (currentIndex < 9)
				{
					goalScript.playerScript.DialogueRestart(currentIndex + 1);
				}
				else if (currentIndex == 9)
				{
					goalScript.playerScript.DialogueRestart(currentIndex + 6);
				}
				else
				{
					goalScript.playerScript.DialogueRestart(currentIndex + 10);
				}
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
			if (!keyDown)
			{
				keyDown = true;
				Manager.instance.RandomizeSfx(false, pickSound);
				if (currentIndex > 0)
				{
					currentIndex--;
				}
				else
				{
					currentIndex = 10;
				}
				ChapterChange();
			}
		}
		else if (Input.GetAxis("Horizontal") > 0f && !keyDown)
		{
			keyDown = true;
			Manager.instance.RandomizeSfx(false, pickSound);
			if (currentIndex < 10)
			{
				currentIndex++;
			}
			else
			{
				currentIndex = 0;
			}
			ChapterChange();
		}
	}

	private void ChapterChange()
	{
		chapterAnimator[tempIndex].SetBool("activeAnim", value: false);
		chapterAnimator[currentIndex].SetBool("activeAnim", value: true);
		tempIndex = currentIndex;
		if (currentIndex != 10)
		{
			chapterTitle.text = chapterTxt[currentIndex];
			chapterTitle.color = new Color(1f, 1f, 1f);
		}
		else
		{
			chapterTitle.text = Manager.instance.dlcTxt[0];
			chapterTitle.color = new Color(0.9f, 0.3f, 0.3f);
		}
	}
}
