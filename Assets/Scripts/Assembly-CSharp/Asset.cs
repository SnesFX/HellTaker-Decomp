using UnityEngine;

public class Asset : Moving
{
	public GameObject boneSplash;

	public bossHp bossHP;

	private float shake;

	private Vector3 basePosition;

	private Vector3 shakePosition;

	public bool voidSpace;

	public int breakPoints;

	public AudioClip kickedSound1;

	public AudioClip kickedSound2;

	public AudioClip kickedSound3;

	public AudioClip boneSound1;

	public AudioClip boneSound2;

	public AudioClip boneSound3;

	public LabScript labScript;

	public Animator pillarAnimator;

	public bool archMecha;

	protected override void Start()
	{
		base.Start();
	}

	private void Update()
	{
		if (shake > 0f)
		{
			base.transform.position = basePosition;
			shake -= 0.02f;
			Shaker();
		}
	}

	public void OnKicked(int xDir, int yDir)
	{
		Manager.instance.RandomizeSfx(kickedSound1, kickedSound2, kickedSound3);
		if (base.tag == "wall")
		{
			PreShaker();
		}
		else if (base.tag == "gigachain")
		{
			animator.SetTrigger("chain_hit");
			breakPoints--;
			bossHP.Dmg();
			if (breakPoints < 1)
			{
				boxCollider.enabled = false;
				cam.Shakedown(0.2f);
				animator.SetTrigger("chain_break");
				Manager.instance.RandomizeSfx(false, boneSound1);
			}
			else
			{
				PreShaker(gigachain: true);
			}
		}
		else
		{
			AttemptMove(xDir, yDir);
		}
	}

	protected override void AttemptMove(int xDir, int yDir)
	{
		base.AttemptMove(xDir, yDir);
		if (!lastMove)
		{
			return;
		}
		Manager.instance.RandomizeVfx(hitVfx, base.transform.position, random: true);
		if (base.tag == "breakable")
		{
			if (xDir > 0 && facing == face.right)
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
				facing = face.left;
			}
			if (xDir < 0 && facing == face.left)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
				facing = face.right;
			}
			animator.SetTrigger("assetKick");
		}
	}

	protected override void OnCantMove(Asset kicked, int xDir, int yDir)
	{
		if (base.tag == "breakable" || base.tag == "pillar")
		{
			Broken();
		}
		else
		{
			PreShaker();
		}
	}

	private void PreShaker(bool gigachain = false)
	{
		if (gigachain)
		{
			Manager.instance.RandomizeVfx(hitVfx, base.transform.position, random: true);
		}
		else
		{
			Manager.instance.RandomizeVfx(smallHitVfx, base.transform.position, random: true);
		}
		basePosition = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		shake = 0.1f;
		Shaker();
	}

	private void Shaker()
	{
		shakePosition = new Vector3(Random.Range(0f - shake, shake), Random.Range(0f - shake, shake), 0f);
		base.transform.position = basePosition + shakePosition;
	}

	public void Broken()
	{
		Manager.instance.RandomizeSfx(false, boneSound1, boneSound2, boneSound3);
		Manager.instance.RandomizeVfx(hitVfx, base.transform.position, random: true);
		if (!archMecha)
		{
			Object.Instantiate(boneSplash, base.transform.position, boneSplash.transform.rotation);
		}
		if (base.tag == "pillar")
		{
			cam.Shakedown(0.14f);
			if (archMecha)
			{
				labScript.AMDhit();
			}
			else
			{
				labScript.PillarBrk();
				pillarAnimator.SetTrigger("break");
			}
		}
		base.gameObject.SetActive(value: false);
	}
}
