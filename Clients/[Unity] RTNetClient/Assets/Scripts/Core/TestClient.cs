using UnityEngine;
using System.Collections;
using RTNet;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(RTNetView))]
public class TestClient : MonoBehaviour
{
	public string ip = "127.0.0.1";
	public int port = 4434;

	public bool showGUI = true;

	private string myMessage = "";

    public string pos;

	//temp
	[SerializeField]
	List<string> messages = new List<string>();

    public GameObject player;

	void Start()
	{
		RTNetView.Client.Connect(ip, port);
	}

    void Update()
    {
        if(player != null)
         pos = player.transform.position.x.ToString() + "," + player.transform.position.y.ToString() + "," + player.transform.position.z.ToString();
    }

    public void SendMyPosition()
    {
        GetComponent<RTNetView>().RPC("UpdateMyPos", RTReceiver.All, pos);

    }

    void ShowMessage(string message)
	{
		Debug.Log("GOT MESSAGE - " + message);
		messages.Add(message);
	}

	Vector2 scroll;
    void OnGUI()
    {
		GUI.Box(new Rect(Screen.width - 140, 10, 130, 25), RTNetView.Client.Connected ? "<color=green>Connected</color>" : "<color=red>Disconnected</color>");
        if(showGUI && RTNetView.Client.Connected)
        {
			if(GUILayout.Button("Instantiate"))
				RTNetView.NetworkInstantiate("Rectangle Thingy");

            if (GUILayout.Button("Sync Pos"))
                SendMyPosition();
            myMessage = GUILayout.TextField(myMessage);
            if (GUILayout.Button("Send", GUILayout.Width(Screen.width / 6)))
            {
                if (!string.IsNullOrEmpty(myMessage))
                {
					GetComponent<RTNetView>().RPC("ShowMessage", RTReceiver.All, myMessage);
					myMessage = "";
                }
            }

			//temp
			if (messages.Count > 10)
				messages.RemoveAt(0);
			if (messages.Count > 0)
			{
				scroll = GUILayout.BeginScrollView(scroll);
				for (int i = 0; i < messages.Count; i++)
					GUILayout.Box(messages[i]);
				GUILayout.EndScrollView();
			}
			else
				GUILayout.Label("No Messages");
        }
    }

    void UpdateMyPos(string message)
    {
        Debug.LogWarning("[UPDATE POSITION]" + message);

        string[] split = message.Split(',');
        float x = float.Parse(split[0]);
        float y = float.Parse(split[1]);
        float z = float.Parse(split[2]);

        Vector3 posi = player.transform.position;
        posi.x = x;
        posi.y = y;
        posi.z = z;
        player.transform.position = posi;

        Debug.Log("[POS] Position Updated " + pos);
    }
}