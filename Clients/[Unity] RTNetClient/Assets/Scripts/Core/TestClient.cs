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
	public int bufferSize = 512;

	public bool showGUI = true;

	private string myMessage = "";

    public string pos;

	//temp
	[SerializeField]
	List<string> messages = new List<string>();

	void Start()
	{
		GetComponent<RTNetView>().BufferSize = bufferSize;
		GetComponent<RTNetView>().Connect(ip, port);
	}

    void ShowMessage(string message)
	{
		Debug.Log("GOT MESSAGE - " + message);
		messages.Add(message);
	}

	Vector2 scroll;
    void OnGUI()
    {
		GUI.Box(new Rect(Screen.width - 140, 10, 130, 25), GetComponent<RTNetView>().Connected ? "<color=green>Connected</color>" : "<color=red>Disconnected</color>");
        if(showGUI && GetComponent<RTNetView>().Connected)
        {
			if(GUILayout.Button("Instantiate"))
				GetComponent<RTNetView>().NetworkInstantiate("Rectangle Thingy");

            myMessage = GUILayout.TextField(myMessage);
            if (GUILayout.Button("Send", GUILayout.Width(Screen.width / 6)))
            {
                if (!string.IsNullOrEmpty(myMessage))
                {
                    GetComponent<RTNetView>().RPC("ShowMessage", RTReceiver.All, myMessage);
                    myMessage = "";
                }
                else Debug.LogError("My Message Was Null");
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
}