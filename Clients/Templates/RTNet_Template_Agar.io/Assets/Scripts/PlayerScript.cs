using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class PlayerScript : RTNetBehaviour
{
	public int _sendRate = 10;
	public float moveSpeed = 10;

	void Start()
	{
		this.sendRate = _sendRate;
	}

	void Update()
	{
		if(isMine)
		{
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			transform.position += new Vector3(h, v, 0) * moveSpeed;
		}
	}

	protected override void OnSerializeView(ref RTStream stream)
	{
		if(stream.isWriting)
		{
			stream.Write(transform.position);
			stream.Write(transform.localScale);
		}
		else
		{
			transform.position = stream.Read<Vector3>();
			transform.localScale = stream.Read<Vector3>();
		}
	}
}
