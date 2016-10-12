using UnityEngine;
using System.Collections;
using RTNet;
using System;

[RequireComponent(typeof(RTNetView))]
public class TestClient : MonoBehaviour
{
	public string ip = "127.0.0.1";
	public int port = 4434;

	public bool showGUI = true;

	private string myMessage = "";

	void Start()
	{
		RTNetView.Client.Connect(ip, port);
	}

	void ShowMessage(string message)
	{
		Debug.Log("GOT MESSAGE - " + message);
	}

    void OnGUI()
    {
        if(showGUI && RTNetView.Client.Connected)
        {
            myMessage = GUILayout.TextField(myMessage);
            if (GUILayout.Button("Send"))
            {
                if (!string.IsNullOrEmpty(myMessage))
                {
					GetComponent<RTNetView>().RPC("ShowMessage", RTReceiver.All, myMessage);
					myMessage = "";
                }
            }
        }
    }
}