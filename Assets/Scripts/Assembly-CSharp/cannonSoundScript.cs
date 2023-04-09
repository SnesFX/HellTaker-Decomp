using UnityEngine;

public class cannonSoundScript : MonoBehaviour
{
	public bool audible;

	public AudioClip emerge;

	public AudioClip[] cannonSalvo;

	private int cannonOrder;

	public void Emerge()
	{
		if (audible)
		{
			Manager.instance.PlayAMD(emerge);
		}
	}

	public void Salvo()
	{
		if (audible)
		{
			Manager.instance.PlayAMD(cannonSalvo[cannonOrder]);
			cannonOrder++;
		}
	}
}
