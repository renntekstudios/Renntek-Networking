using UnityEngine;
using System.Collections;
using RTNet;
using System;

public class TestClient : MonoBehaviour
{

    [SerializeField]
    exampleClient myClient;
    [SerializeField]
    string myMessage = "";
    [SerializeField]
    string ServerIP = "127.0.0.1";
    [SerializeField]
    int ServerPort = 4434;

    bool showGUI = false;

    void Start()
    {
        Connect(ServerIP, ServerPort);
        
    }

    void Connect(string ip, int port)
    {
       
        exampleClient client = new exampleClient();
        client.Timeout = false;
        client.Connect(ip, port);
        myClient = client;
        showGUI = true;
        DebugLog("DEBUG", "I Am Connecting with IP=" + ip + " And Port=" + port);
    }

    public static void DebugLog(string code, string message)
    {
        switch (code)
        {
            case "DEBUG":
                Debug.Log("DEBUG : " + message);
                break;
            case "ERROR":
                Debug.LogError("ERROR : " + message);
                break;
        }
    }

    void OnGUI()
    {
        if(showGUI && myClient.Connected)
        {
            myMessage = GUILayout.TextField(myMessage);
            if (GUILayout.Button("Send"))
            {
                if (!string.IsNullOrEmpty(myMessage))
                {
                    if (myMessage.ToLower() == "quit")
                    {
                        DebugLog("ERROR", "I Want To Quit");
                    }
                    else
                    {
                        myClient.Send((RTPacketID)123, System.Text.Encoding.UTF8.GetBytes(myMessage));
                        DebugLog("DEBUG", "Sending Message = " + myMessage);
                        myMessage = "";
                    }
                }
            }
        }
    }

    void OnApplicationQuit() { myClient.Disconnect(); }

    void Update()
    {
    }
}

public class exampleClient : RTClient
{
    protected override void HandlePacket(short packetID, byte[] data)
    {
        LogDebug("Got \"" + packetID + "\" packet (" + data.Length + " bytes)");
    }
    protected override void Log(string message) {
        Console.WriteLine("[LOG] " + message);
        TestClient.DebugLog("DEBUG", "[LOG] " + message);
    }
    protected override void LogDebug(string message) {
        Console.WriteLine("[DEBUG] " + message);
        TestClient.DebugLog("DEBUG", "[DEBUG] " + message);
    }
    protected override void LogWarning(string message)
    { Console.WriteLine("[WARNING] " + message);
        TestClient.DebugLog("DEBUG", "WARNING " + message);
    }
    protected override void LogError(string error) {
        Console.WriteLine("[ERROR] " + error);
        TestClient.DebugLog("ERROR", error);
    }
    protected override void LogException(Exception e, string error) {
        Console.WriteLine("[EXCEPTION] " + e.Message + " (" + error + ")\n\t" + e.StackTrace);
        TestClient.DebugLog("ERROR", "[EXCEPTION] " + e.Message + " (" + error + ")\n\t" + e.StackTrace);
    }
}
