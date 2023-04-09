using UnityEngine;

public class DeathFade : MonoBehaviour
{
	private SpriteRenderer sprite;

	private void Start()
	{
		sprite = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		sprite.color -= new Color(0f, 0f, 0f, 0.05f);
	}
}
