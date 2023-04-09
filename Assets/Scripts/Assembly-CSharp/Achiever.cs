using Steamworks;
using UnityEngine;

public class Achiever : MonoBehaviour
{
	public string achievement;

	private void OnTriggerEnter2D(Collider2D player)
	{
		if (player.tag == "player" && Manager.instance.steam && SteamManager.Initialized)
		{
			SteamUserStats.SetAchievement(achievement);
			SteamUserStats.StoreStats();
		}
	}
}
