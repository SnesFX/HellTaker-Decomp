using System.Collections;
using System.IO;
using UnityEngine;

public class Manager : MonoBehaviour
{
	public GameObject vfxSource;

	public AudioSource[] efxSourceArray;

	private int currentSource;

	private AudioSource efxSource;

	public AudioSource musicSource;

	public AudioSource sinLoopSource;

	public AudioSource sinSource;

	public AudioSource amdSource;

	public static Manager instance;

	public float lowPitchRange = 0.95f;

	public float highPitchRange = 1.05f;

	public bool playerTurn;

	public string directory;

	public string[] menuTxt;

	public string[] dlcTxt;

	public bool deadSound;

	public float mute = 1f;

	public bool doorsClosing;

	public bool machineStatus;

	public bool first = true;

	public int AMDphase;

	public bool dlcTransition;

	public bool amdStatus;

	public AudioClip amdLoopTrack;

	public AudioClip sinLoopTrack;

	public bool steam;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
		Cursor.visible = false;
		directory = Directory.GetCurrentDirectory();
		menuTxt = File.ReadAllLines(directory + "/local/m.json");
		dlcTxt = File.ReadAllLines(directory + "/localHM/hm_m.json");
		VolumeChange(0, PlayerPrefs.GetInt("musicVol", 2));
		VolumeChange(1, PlayerPrefs.GetInt("efxVol", 3));
	}

	public void PlaySingle(AudioClip clip)
	{
		efxSource.clip = clip;
		efxSource.Play();
	}

	public void RandomizeSfx(bool pitchBool = true, params AudioClip[] clips)
	{
		if (!deadSound || doorsClosing)
		{
			doorsClosing = false;
			efxSource = efxSourceArray[currentSource];
			currentSource++;
			if (currentSource == efxSourceArray.Length)
			{
				currentSource = 0;
			}
			int num = Random.Range(0, clips.Length);
			if (pitchBool)
			{
				float pitch = Random.Range(lowPitchRange, highPitchRange);
				efxSource.pitch = pitch;
			}
			else
			{
				efxSource.pitch = 1f;
			}
			efxSource.clip = clips[num];
			efxSource.Play();
		}
	}

	public void PlaySin(AudioClip sinClip)
	{
		if (!deadSound)
		{
			StartCoroutine(SinChanger());
			sinSource.clip = sinClip;
			sinSource.Play();
		}
	}

	public void RandomizeVfx(GameObject[] clips, Vector3 place, bool random)
	{
		int num = Random.Range(0, clips.Length);
		vfxSource = clips[num];
		if (random)
		{
			place = new Vector3(place.x + Random.Range(-0.1f, 0.1f), place.y + Random.Range(-0.1f, 0.1f), place.z);
		}
		Object.Instantiate(vfxSource, place, vfxSource.transform.rotation);
	}

	public void VolumeChange(int type, int lvl)
	{
		float num = (float)lvl / 3f;
		num *= mute;
		if (lvl == 2 || lvl == 1)
		{
			num += 0.04f;
		}
		if (type == 1)
		{
			sinLoopSource.volume = num;
			sinSource.volume = num;
			amdSource.volume = num;
			for (int i = 0; i < efxSourceArray.Length; i++)
			{
				efxSourceArray[i].volume = num;
			}
			PlayerPrefs.SetInt("efxVol", lvl);
		}
		else
		{
			musicSource.volume = num;
			PlayerPrefs.SetInt("musicVol", lvl);
		}
	}

	public void SongChange(AudioClip track)
	{
		StartCoroutine(SongChanger(track));
	}

	public IEnumerator SongChanger(AudioClip track)
	{
		float tempVol = musicSource.volume;
		while (musicSource.volume > 0f)
		{
			musicSource.volume -= 0.04f;
			yield return new WaitForSeconds(0.01f);
		}
		musicSource.volume = tempVol;
		musicSource.clip = track;
		musicSource.Play();
	}

	public void SinMuter(bool absolute = false)
	{
		if (absolute)
		{
			sinLoopSource.mute = true;
			machineStatus = false;
		}
		else if (machineStatus || amdStatus)
		{
			sinLoopSource.mute = !sinLoopSource.mute;
		}
		if (sinSource.isPlaying)
		{
			sinSource.Pause();
		}
		else
		{
			sinSource.UnPause();
		}
		if (amdSource.isPlaying)
		{
			amdSource.Pause();
		}
		else
		{
			amdSource.UnPause();
		}
	}

	public IEnumerator SinChanger()
	{
		if (!machineStatus)
		{
			machineStatus = true;
			sinLoopSource.mute = false;
			yield return new WaitForSeconds(0.1f);
		}
		else
		{
			machineStatus = false;
			sinLoopSource.mute = true;
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void PlayAMD(AudioClip amdClip)
	{
		if (!deadSound)
		{
			sinSource.clip = amdClip;
			sinSource.Play();
		}
	}

	public void PlayHands(AudioClip handClip)
	{
		if (!deadSound)
		{
			amdSource.clip = handClip;
			amdSource.Play();
		}
	}

	public void LoopAMD()
	{
		sinLoopSource.clip = amdLoopTrack;
		sinLoopSource.Play();
		sinLoopSource.mute = false;
		amdStatus = true;
	}

	public void LoopMute()
	{
		sinLoopSource.clip = sinLoopTrack;
		sinLoopSource.Play();
		sinLoopSource.mute = true;
		amdStatus = false;
	}
}
