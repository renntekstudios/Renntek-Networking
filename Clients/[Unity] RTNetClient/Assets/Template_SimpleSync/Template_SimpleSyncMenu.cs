using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class Template_SimpleSyncMenu : MonoBehaviour
{
	[Header("Client settings")]
	public string ip = "127.0.0.1";
	public int port = 4434;
	public bool debugMode = false;
	public int bufferSize = 512;

	void Start()
	{
		GetComponent<RTNetView>().DebugMode = debugMode;
		GetComponent<RTNetView>().BufferSize = bufferSize;
		GetComponent<RTNetView>().Connect(ip, port);
	}
}
