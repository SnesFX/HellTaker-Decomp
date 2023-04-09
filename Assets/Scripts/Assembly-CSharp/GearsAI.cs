using UnityEngine;

public class GearsAI : MonoBehaviour
{
	public int currentSpin;

	public Animator spinner;

	public void SpinCounter()
	{
		currentSpin++;
		if (currentSpin == 75)
		{
			spinner.SetTrigger("finish");
		}
	}
}
