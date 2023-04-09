using System.Collections;
using UnityEngine;

public abstract class Moving : MonoBehaviour
{
	protected enum face
	{
		left = 0,
		right = 1
	}

	public LayerMask blockingLayer;

	public bool lastMove = true;

	public Animator animator;

	public CameraShake cam;

	public GameObject[] hitVfx;

	public GameObject[] smallHitVfx;

	public GameObject[] dustVfx;

	public AudioClip movemntSound1;

	public AudioClip movemntSound2;

	public AudioClip movemntSound3;

	private float moveTime = 0.1f;

	public BoxCollider2D boxCollider;

	public Rigidbody2D rb2D;

	private float inverseMoveTime;

	protected face facing = face.right;

	public bool inMovement;

	protected virtual void Start()
	{
		animator = GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider2D>();
		rb2D = GetComponent<Rigidbody2D>();
		inverseMoveTime = 1f / moveTime;
	}

	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 vector = base.transform.position;
		Vector2 vector2 = vector + new Vector2(xDir, yDir);
		boxCollider.enabled = false;
		hit = Physics2D.Linecast(vector, vector2, blockingLayer);
		boxCollider.enabled = true;
		if (hit.transform == null)
		{
			StartCoroutine(SmoothMovement(vector2));
			return true;
		}
		return false;
	}

	protected IEnumerator SmoothMovement(Vector3 end)
	{
		inMovement = true;
		Manager.instance.RandomizeVfx(dustVfx, base.transform.position, random: false);
		float sqrRemainingDistance = (base.transform.position - end).sqrMagnitude;
		Manager.instance.RandomizeSfx(movemntSound1, movemntSound2, movemntSound3);
		while (sqrRemainingDistance > 0.01f)
		{
			Vector3 vector = Vector3.Lerp(rb2D.position, end, 0.5f * Time.deltaTime * 60f);
			rb2D.MovePosition(vector);
			sqrRemainingDistance = (base.transform.position - end).sqrMagnitude;
			yield return null;
		}
		rb2D.MovePosition(end);
		inMovement = false;
	}

	protected virtual void AttemptMove(int xDir, int yDir)
	{
		lastMove = Move(xDir, yDir, out var hit);
		if (!(hit.transform == null))
		{
			Asset component = hit.transform.GetComponent<Asset>();
			if (!lastMove)
			{
				OnCantMove(component, xDir, yDir);
			}
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "spikes")
		{
			other.gameObject.GetComponent<Spikes>().victim = base.gameObject;
		}
	}

	protected abstract void OnCantMove(Asset kicked, int xDir, int yDir);
}
