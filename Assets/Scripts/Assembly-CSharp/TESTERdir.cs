using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TESTERdir : MonoBehaviour
{
	public Text[] testers;

	public GameObject testerBase;

	private bool keyDown;

	private void Update()
	{
		if (Input.GetButton("Fire3"))
		{
			if (!keyDown)
			{
				keyDown = true;
				if (!testerBase.activeSelf)
				{
					testerBase.SetActive(value: true);
					testers[0].text = Application.dataPath;
					testers[1].text = Application.persistentDataPath;
					testers[2].text = Directory.GetCurrentDirectory();
					testers[3].text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					testers[4].text = Path.GetDirectoryName(Application.dataPath);
					testers[5].text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
				}
				else
				{
					testerBase.SetActive(value: false);
				}
			}
		}
		else
		{
			keyDown = false;
		}
	}
}
