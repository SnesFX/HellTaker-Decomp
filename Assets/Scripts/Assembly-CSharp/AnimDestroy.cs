using UnityEngine;

public class AnimDestroy : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.02f);
	}
}
