using UnityEngine;

public class BoneScatterDestroy : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject, 1f);
	}
}
