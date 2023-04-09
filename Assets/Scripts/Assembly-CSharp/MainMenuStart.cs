using UnityEngine;
using UnityEngine.UI;

public class MainMenuStart : MonoBehaviour
{
	public Image transitionDoor;

	public GoalSprite menuDialogue;

	private void Awake()
	{
		if (Manager.instance.first)
		{
			Manager.instance.first = false;
			return;
		}
		transitionDoor.color = new Color(1f, 1f, 1f, 1f);
		menuDialogue.chapterNumber = "xxx";
	}
}
