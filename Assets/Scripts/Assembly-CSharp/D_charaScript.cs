using System.Collections;
using UnityEngine;

public class D_charaScript : MonoBehaviour
{
	private SpriteRenderer charaSprite;

	public GameObject slave;

	public GameObject maid;

	private void Start()
	{
		charaSprite = GetComponent<SpriteRenderer>();
		StartCoroutine(SmoothShowup());
		if (slave != null)
		{
			StartCoroutine(SlaveSummon());
		}
		if (maid != null)
		{
			Object.Instantiate(maid, new Vector3(base.transform.position.x - 4.75f, base.transform.position.y), base.transform.rotation, base.transform);
		}
	}

	private IEnumerator SmoothShowup()
	{
		Vector3 goal = new Vector3(base.transform.position.x - 2f, base.transform.position.y, base.transform.position.z);
		Color goalAlpha = new Color(charaSprite.color.r, charaSprite.color.g, charaSprite.color.b, 1f);
		float sqrRemainingDistance = (base.transform.position - goal).sqrMagnitude;
		while (sqrRemainingDistance > 0.0002f)
		{
			float num = 0.2f * Time.deltaTime * 60f;
			Vector3 position = Vector3.Lerp(base.transform.position, goal, num);
			Color color = Color.Lerp(charaSprite.color, goalAlpha, num / 2f);
			base.transform.position = position;
			charaSprite.color = color;
			sqrRemainingDistance = (base.transform.position - goal).sqrMagnitude;
			yield return null;
		}
		base.transform.position = goal;
		charaSprite.color = goalAlpha;
	}

	private IEnumerator SlaveSummon()
	{
		yield return new WaitForSeconds(3.2f);
		Object.Instantiate(slave, new Vector3(base.transform.position.x - 2f, base.transform.position.y), base.transform.rotation, base.transform);
		yield return new WaitForSeconds(0.2f);
		Object.Instantiate(slave, new Vector3(base.transform.position.x + 6f, base.transform.position.y), base.transform.rotation, base.transform);
		yield return new WaitForSeconds(2.16f);
		GetComponent<Animator>().enabled = false;
	}
}
