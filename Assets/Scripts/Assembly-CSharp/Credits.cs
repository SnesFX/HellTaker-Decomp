using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
	public Text creditsText;

	public int txtNumber;

	public bool dlc;

	private void Start()
	{
		if (!dlc)
		{
			creditsText.text = Manager.instance.menuTxt[txtNumber];
		}
		else
		{
			creditsText.text = Manager.instance.dlcTxt[txtNumber];
		}
	}
}
