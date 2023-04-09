using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : Moving
{
	public Animator sinAnimator;

	public bool keyMaster;

	public LayerMask defaultLayer;

	public float restartLevelDelay = 1f;

	public Text willText;

	public AudioClip bloodSound1;

	public AudioClip bloodSound2;

	public AudioClip deathSound;

	public AudioClip keySound;

	public AudioClip lockSound;

	public AudioClip winSound;

	public AudioClip sinchainSound1;

	public AudioClip sinchainSound2;

	public AudioClip sinchainSound3;

	public GameObject spikeParent;

	public GameObject[] bloodSplash;

	public GameObject unlockVfx;

	public GameObject deathAnim;

	public GameObject perishableUI;

	public Animator door;

	public int will;

	private Spikes[] spikes;

	private SpriteRenderer playerSprite;

	private float textScale;

	public bool frozen;

	public bool unMenuable = true;

	public bool inMenu = true;

	public bool cutscene;

	private Coroutine locker;

	public GameObject[] hearts;

	public bool protection;

	public int nextHeart;

	public bool restartable = true;

	public bool cheater;

	public GameObject loveAura;

	public bool labScreen;

	public SpriteRenderer armorSprite;

	public bool armored;

	public GameObject breakVfx;

	public AudioClip breakSound;

	protected override void Start()
	{
		Manager.instance.playerTurn = false;
		playerSprite = GetComponent<SpriteRenderer>();
		spikes = spikeParent.GetComponentsInChildren<Spikes>();
		if (will > 50)
		{
			willText.text = "XX";
		}
		else
		{
			willText.text = will.ToString();
		}
		base.Start();
		StartCoroutine(Starter());
	}

	public IEnumerator Starter()
	{
		yield return new WaitForSeconds(0.5f);
		inMenu = false;
		if (!cutscene)
		{
			unMenuable = false;
			Manager.instance.playerTurn = true;
		}
	}

	private void Update()
	{
		if (willText.color != Color.white)
		{
			willText.color += new Color(0f, 0.04f, 0.05f);
			playerSprite.color += new Color(0f, 0.05f, 0.05f);
		}
		if (textScale > 0f)
		{
			willText.transform.localScale -= new Vector3(0.05f, 0.05f, 0f);
			textScale -= 0.05f;
		}
		if (!Manager.instance.playerTurn || frozen || inMenu)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		num = (int)Input.GetAxisRaw("Horizontal");
		num2 = (int)Input.GetAxisRaw("Vertical");
		if (num != 0)
		{
			num2 = 0;
		}
		if ((num != 0 || num2 != 0) && !inMovement)
		{
			Manager.instance.playerTurn = false;
			if (will == 0)
			{
				StartCoroutine(GameOver());
			}
			else
			{
				AttemptMove(num, num2);
			}
		}
	}

	public IEnumerator GameOver(int i = -2)
	{
		unMenuable = true;
		Manager.instance.LoopMute();
		if (!frozen)
		{
			Manager.instance.SinMuter(absolute: true);
			frozen = true;
			Manager.instance.RandomizeSfx(false, deathSound);
			Manager.instance.deadSound = true;
			Object.Instantiate(deathAnim, base.transform.position, deathAnim.transform.rotation);
			perishableUI.SetActive(value: false);
			cam.Shakedown(1f);
			yield return new WaitForSeconds(0.9f);
		}
		else
		{
			Manager.instance.SinMuter(absolute: true);
		}
		door.SetTrigger("gameOver");
		yield return new WaitForSeconds(1f);
		Advance(i);
	}

	protected override void AttemptMove(int xDir, int yDir)
	{
		if (xDir < 0 && facing == face.right)
		{
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
			facing = face.left;
		}
		if (xDir > 0 && facing == face.left)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			facing = face.right;
		}
		base.AttemptMove(xDir, yDir);
		if (lastMove)
		{
			animator.SetTrigger("playerMove");
			StartCoroutine(Waiter());
		}
	}

	protected override void OnCantMove(Asset kicked, int xDir, int yDir)
	{
		if (!kicked.voidSpace)
		{
			animator.SetTrigger("playerKick");
			kicked.OnKicked(xDir, yDir);
			StartCoroutine(Waiter());
		}
		else
		{
			Manager.instance.playerTurn = true;
		}
	}

	public IEnumerator Waiter()
	{
		Spikes[] array = spikes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Spike();
		}
		yield return new WaitForSeconds(0.14f);
		if (will > 0)
		{
			MinusWill();
		}
		if (textScale < 0.2f && !labScreen)
		{
			willText.transform.localScale = new Vector3(1.2f, 1.2f, 0f);
			textScale = 0.2f;
		}
		yield return new WaitForSeconds(0.01f);
		Manager.instance.playerTurn = true;
	}

	public void Spiked()
	{
		if (will > 0)
		{
			MinusWill();
		}
		willText.transform.localScale = new Vector3(1.5f, 1.5f, 0f);
		textScale = 0.5f;
		willText.color = new Color(1f, 0.2f, 0f);
		playerSprite.color = new Color(1f, 0f, 0f);
		cam.Shakedown(0.14f);
		Manager.instance.RandomizeVfx(bloodSplash, base.transform.position, random: false);
		Manager.instance.RandomizeSfx(bloodSound1, bloodSound2);
	}

	public void MinusWill()
	{
		if (will >= 50)
		{
			return;
		}
		will--;
		if (labScreen)
		{
			if (will < 10 && will != 0)
			{
				willText.text = "0" + will;
			}
			else if (will != 0)
			{
				willText.text = will.ToString();
			}
			else
			{
				willText.text = "XX";
			}
		}
		else if (will != 0)
		{
			willText.text = will.ToString();
		}
		else
		{
			willText.text = "X";
		}
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);
		if (other.tag == "lockbox" && keyMaster)
		{
			other.gameObject.transform.parent.gameObject.layer = 0;
			if (locker != null)
			{
				StopCoroutine(locker);
			}
		}
		else if (other.tag == "wall" || other.tag == "key")
		{
			if (other.tag == "wall")
			{
				Manager.instance.RandomizeSfx(false, lockSound);
			}
			else if (other.tag == "key")
			{
				Manager.instance.RandomizeSfx(false, keySound);
			}
			keyMaster = true;
			Object.Instantiate(unlockVfx, other.gameObject.transform.position, other.gameObject.transform.rotation);
			other.gameObject.SetActive(value: false);
		}
		else if (other.tag == "armor")
		{
			if (!armored)
			{
				Manager.instance.RandomizeSfx(false, keySound);
				Object.Instantiate(unlockVfx, other.gameObject.transform.position, other.gameObject.transform.rotation);
				armorSprite.color += new Color(0f, 0f, 0f, 1f);
				armored = true;
				other.gameObject.SetActive(value: false);
			}
		}
		else if (other.tag == "sinSpike")
		{
			protection = true;
			sinAnimator.enabled = false;
			StartCoroutine(GameOver());
		}
		else if (other.tag == "sinHurt")
		{
			if (protection || cheater)
			{
				return;
			}
			if (nextHeart > 2)
			{
				protection = true;
				if (nextHeart > 9)
				{
					StartCoroutine(GameOver(-1));
					return;
				}
				sinAnimator.enabled = false;
				StartCoroutine(GameOver());
			}
			else
			{
				StartCoroutine(ChainHurt());
			}
		}
		else if (other.tag == "skull" && !protection)
		{
			LaserDeath();
		}
	}

	public IEnumerator ChainHurt()
	{
		protection = true;
		hearts[nextHeart].SetActive(value: false);
		nextHeart++;
		willText.color = new Color(1f, 0.2f, 0f);
		playerSprite.color = new Color(1f, 0f, 0f);
		cam.Shakedown(0.14f);
		Manager.instance.RandomizeVfx(bloodSplash, base.transform.position, random: false);
		Manager.instance.RandomizeSfx(sinchainSound1, sinchainSound2, sinchainSound3);
		yield return new WaitForSeconds(0.4f);
		protection = false;
	}

	protected void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "lockbox")
		{
			locker = StartCoroutine(Locker(other));
		}
	}

	public IEnumerator Locker(Collider2D other)
	{
		yield return new WaitForSeconds(0.01f);
		other.gameObject.transform.parent.gameObject.layer = 8;
	}

	public IEnumerator Win()
	{
		unMenuable = true;
		animator.SetTrigger("playerWin");
		Manager.instance.RandomizeSfx(false, winSound);
		yield return new WaitForSeconds(1.5f);
		cam.Shakedown(0.14f);
		yield return new WaitForSeconds(0.2f);
		Time.timeScale = 0.2f;
		yield return new WaitForSeconds(0.1f);
		while (Time.timeScale != 1f)
		{
			Time.timeScale += 0.4f;
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(1.2f);
		door.SetTrigger("gameOver");
		yield return new WaitForSeconds(1f);
		Advance(-1);
	}

	public void Victory()
	{
		StartCoroutine(Win());
	}

	public void DialogueRestart(int i = -2)
	{
		StartCoroutine(GameOver(i));
	}

	private void Advance(int i)
	{
		if (i > -1)
		{
			SceneManager.LoadScene(i, LoadSceneMode.Single);
		}
		else
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2 + i, LoadSceneMode.Single);
		}
	}

	public void EndingCancel()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 1f);
	}

	public void PowerOfLove()
	{
		Object.Instantiate(loveAura, base.gameObject.transform.position, base.gameObject.transform.rotation, base.gameObject.transform);
	}

	public void LaserDeath()
	{
		if (armored)
		{
			StartCoroutine(ArmorBreak());
			return;
		}
		protection = true;
		StartCoroutine(GameOver());
	}

	public IEnumerator ArmorBreak()
	{
		protection = true;
		armorSprite.color += new Color(0f, 0f, 0f, -1f);
		Manager.instance.RandomizeSfx(false, breakSound);
		Object.Instantiate(breakVfx, base.gameObject.transform.position, base.gameObject.transform.rotation);
		armored = false;
		cam.Shakedown(0.14f);
		yield return new WaitForSeconds(0.6f);
		protection = false;
	}
}
