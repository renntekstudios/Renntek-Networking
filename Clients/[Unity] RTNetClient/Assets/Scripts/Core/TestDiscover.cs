using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class TestDiscover : MonoBehaviour
{
	private RTServerInfo info;

	void Start()
	{
		// RTNetView.Client.Connect("127.0.0.1", 4434);
		// info = GetComponent<RTNetView>().GetLocalServers();
	}

	void OnGUI()
	{
		GUILayout.Box(GetComponent<RTNetView>().Connected ? "<color=green>Connected!</color>" : "<color=red>Disconnected</color>");
		if (GUILayout.Button("Refresh"))
			info = GetComponent<RTNetView>().GetLocalServers();
		if (info == null)
			GUILayout.Box("<color=red>No local server!</color>");
		else
		{
			if (GUILayout.Button("Connect"))
				GetComponent<RTNetView>().Connect(info.IP, info.Port);
		}
		if(GetComponent<RTNetView>().Connected)
			if (GUILayout.Button("Disconnect"))
				GetComponent<RTNetView>().Disconnect();
	}
}
