using System.Collections;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
	public int mode;

	public Animator animator;

	public Transform spriteTransform;

	public Transform orbTransform;

	public LineRenderer lineRender;

	public LineRenderer lineRenderB;

	public LineRenderer lineRenderA;

	private int layerMask = 512;

	private int layerMask2 = 1024;

	public Player playerScript;

	public GameObject orb;

	public GameObject orbB;

	public bool[] laserLoop;

	public AudioClip laserSound1;

	public AudioClip laserSound2;

	public AudioClip laserSound3;

	public AudioClip laserSound4;

	public bool audible;

	public bool boss;

	private void Start()
	{
		if (mode == 1)
		{
			StartCoroutine(Puzzle());
		}
		else if (mode == 2)
		{
			StartCoroutine(Active());
		}
	}

	private void Update()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(orbTransform.position, orbTransform.up, float.PositiveInfinity, layerMask);
		if ((bool)raycastHit2D)
		{
			lineRender.SetPosition(0, new Vector3(0f, 0f, 0f));
			lineRender.SetPosition(1, new Vector3(raycastHit2D.point.x - spriteTransform.position.x, raycastHit2D.point.y - spriteTransform.position.y - 0.3f, 0f));
			lineRenderB.SetPosition(0, new Vector3(0f, 0f, 0f));
			lineRenderB.SetPosition(1, new Vector3(raycastHit2D.point.x - spriteTransform.position.x, raycastHit2D.point.y - spriteTransform.position.y - 0.3f, 0f));
			if (mode != 0)
			{
				lineRenderA.SetPosition(0, new Vector3(0f, 0f, 0f));
				lineRenderA.SetPosition(1, new Vector3(raycastHit2D.point.x - spriteTransform.position.x, raycastHit2D.point.y - spriteTransform.position.y - 0.3f, 0f));
			}
		}
		if (mode == 0)
		{
			hitRay();
		}
	}

	public IEnumerator Puzzle()
	{
		mode = 0;
		animator.SetTrigger("puzzle");
		orb.SetActive(value: true);
		yield return new WaitForSeconds(0.13f);
		orbB.SetActive(value: true);
	}

	public IEnumerator Active()
	{
		animator.SetTrigger("activate");
		orb.SetActive(value: true);
		yield return new WaitForSeconds(0.13f);
		orbB.SetActive(value: true);
		while (!boss)
		{
			for (int i = 0; i < laserLoop.Length; i++)
			{
				yield return new WaitForSeconds(0.88f);
				if (laserLoop[i] && mode != 1)
				{
					animator.SetTrigger("shot");
				}
			}
		}
	}

	public void hitRay()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(orbTransform.position, orbTransform.up, float.PositiveInfinity, layerMask2);
		if (mode != 0 && audible)
		{
			Manager.instance.RandomizeSfx(laserSound1, laserSound2, laserSound3, laserSound4);
		}
		if ((bool)raycastHit2D && raycastHit2D.transform.tag == "player" && !playerScript.protection)
		{
			playerScript.LaserDeath();
		}
	}

	public void Deactivate()
	{
		mode = 1;
		animator.SetTrigger("deactivate");
	}

	public void Activate()
	{
		boss = true;
		mode = 1;
		StartCoroutine(Active());
	}

	public void ShotCommand()
	{
		animator.SetTrigger("shot");
	}
}
