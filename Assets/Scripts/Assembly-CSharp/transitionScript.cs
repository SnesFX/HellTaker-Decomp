using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class transitionScript : MonoBehaviour
{
	private Image sprite;

	public AudioClip openingSound;

	public AudioClip closingSound;

	public GameObject dlcDoor;

	public AudioClip openingSoundALT;

	public AudioClip closingSoundALT;

	public bool dlcFirstLevel;

	public bool mainMenu;

	private void Start()
	{
		sprite = GetComponent<Image>();
		if (Manager.instance.dlcTransition)
		{
			sprite.enabled = false;
			dlcDoor.SetActive(value: true);
		}
		if (sprite.color.a == 0f)
		{
			StartCoroutine(Showup());
		}
	}

	public void Closing()
	{
		Manager.instance.LoopMute();
		Manager.instance.doorsClosing = true;
		if (!Manager.instance.dlcTransition)
		{
			Manager.instance.RandomizeSfx(false, closingSound);
		}
		else
		{
			Manager.instance.RandomizeSfx(false, closingSoundALT);
		}
	}

	public void Opening()
	{
		Manager.instance.PlayAMD(null);
		Manager.instance.PlayHands(null);
		if (sprite.color.a == 1f)
		{
			if (!Manager.instance.dlcTransition)
			{
				Manager.instance.RandomizeSfx(false, openingSound);
			}
			else
			{
				Manager.instance.RandomizeSfx(false, openingSoundALT);
			}
		}
	}

	private IEnumerator Showup()
	{
		yield return new WaitForSeconds(2f);
		sprite.color = new Color(1f, 1f, 1f, 1f);
	}

	public void DoorSwap()
	{
		if (dlcFirstLevel)
		{
			sprite.enabled = false;
			dlcDoor.SetActive(value: true);
			Manager.instance.dlcTransition = true;
		}
		else if (mainMenu)
		{
			sprite.enabled = true;
			dlcDoor.SetActive(value: false);
			Manager.instance.dlcTransition = false;
		}
	}
}
