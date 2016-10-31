using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class GameManager : MonoBehaviour
{
	public enum PlayerState { Menu, Dead, Alive, RespawnTimer }

	public PlayerState state = PlayerState.Menu;
	public int respawnSeconds = 10;

	public string playerPrefab;

	private int respawnSecondsLeft = 0;

	void Start()
	{
		state = PlayerState.Menu;
	}

	void OnGUI()
	{ 
		switch(state)
		{
			case PlayerState.Dead:
				if (GUI.Button(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 12.5f, 150, 25), "Spawn"))
					SpawnLocal();
				break;
			case PlayerState.RespawnTimer:
				GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 12.5f, 150, 25), "Spawning in " + respawnSecondsLeft + "s");
				break;
			default:
			case PlayerState.Menu:
			case PlayerState.Alive:
				break;
		}
	}

	IEnumerator RespawnTimer()
	{
		respawnSecondsLeft = respawnSeconds;
		while(respawnSecondsLeft > 0)
		{
			respawnSecondsLeft--;
			yield return new WaitForSeconds(1);
		}
		state = PlayerState.Dead;
	}

	void SpawnLocal()
	{
		// todo: spawn at random spot
		GameObject go = GetComponent<RTNetView>().NetworkInstantiate(playerPrefab);
		go.name = "Local Player";
		state = PlayerState.Alive;
	}
}
