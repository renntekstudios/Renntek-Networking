using UnityEngine;
using System.Collections;

using RTNet;

[RequireComponent(typeof(RTNetView))]
public class TestMove : MonoBehaviour
{
	void OnGUI()
	{
		if (GUI.Button(new Rect((Screen.width / 2) - 50, (Screen.height / 2) - 12.5f, 100, 25), "Move"))
			GetComponent<RTNetView>().RPC("RPCMove", RTReceiver.All, transform.position + new Vector3(0, 0, 1));
	}

	public void RPCMove(Vector3 pos)
	{
		transform.position = pos;
	}
}
