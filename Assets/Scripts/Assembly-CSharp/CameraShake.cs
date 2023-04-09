using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public Transform camTransform;

	private float shakeDuration;

	private float shakeDurationInitial;

	private float shakeAmount;

	private Vector3 originalPos;

	public AudioClip levelTrack;

	private Coroutine currentShake;

	private int target = 60;

	public bool mainMenu;

	public bool AMD;

	private void Awake()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = target;
		camTransform = GetComponent<Transform>();
		if (mainMenu)
		{
			Manager.instance.AMDphase = 0;
			Manager.instance.LoopMute();
		}
	}

	private void Start()
	{
		if (Manager.instance.mute == 0.1f)
		{
			Manager.instance.mute = 1f;
			Manager.instance.VolumeChange(0, PlayerPrefs.GetInt("musicVol", 2));
		}
		if (Manager.instance.musicSource.clip != levelTrack)
		{
			if (AMD && Manager.instance.AMDphase == 0)
			{
				Manager.instance.SongChange(null);
			}
			else
			{
				Manager.instance.SongChange(levelTrack);
			}
		}
		Manager.instance.deadSound = false;
	}

	public void Shakedown(float dur, float am = 0.1f)
	{
		if (shakeDuration > 0f)
		{
			StopCoroutine(currentShake);
			camTransform.localPosition = originalPos;
		}
		originalPos = camTransform.localPosition;
		shakeAmount = am;
		shakeDuration = dur;
		shakeDurationInitial = dur;
		currentShake = StartCoroutine(Shake());
	}

	private void Update()
	{
		if (Application.targetFrameRate != target)
		{
			Application.targetFrameRate = target;
		}
	}

	public IEnumerator Shake()
	{
		while (shakeDuration > 0f)
		{
			camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
			shakeDuration -= Time.deltaTime;
			shakeAmount -= shakeAmount * Time.deltaTime / shakeDurationInitial;
			yield return new WaitForSeconds(Time.deltaTime);
			if (shakeDuration <= 0f)
			{
				camTransform.localPosition = originalPos;
			}
		}
	}
}
