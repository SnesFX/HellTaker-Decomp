using UnityEngine;
using UnityEngine.UI;

public class bossHp : MonoBehaviour
{
	public Text title;

	public int currentLevel;

	public float multiplayer;

	public RectTransform bar;

	public GoalSprite dialogue;

	public GameObject loop;

	public float def = 1f;

	private void Start()
	{
		title.text = Manager.instance.menuTxt[currentLevel];
	}

	public void Dmg()
	{
		float x = bar.anchoredPosition.x;
		bar.anchoredPosition = new Vector3(x - 11f * (multiplayer * def), bar.anchoredPosition.y);
		bar.localScale -= new Vector3(1f * (multiplayer * def), 0f);
		if (bar.localScale.x <= 0f)
		{
			dialogue.AfterChains(1f);
			loop.SetActive(value: false);
			base.gameObject.SetActive(value: false);
		}
	}
}
