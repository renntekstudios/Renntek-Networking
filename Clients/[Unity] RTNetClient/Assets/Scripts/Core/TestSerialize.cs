using UnityEngine;
using System.Collections;
using RTNet;

public class TestSerialize : RTNetBehaviour
{
	public Vector3 _position = Vector3.zero;
	public Quaternion _rotation = Quaternion.identity;

	protected override void OnSerializeView(ref RTStream stream)
	{
		Debug.Log("OnSerializeView (" + (stream.isReading ? "Reading" : "Writing") + ")");
		if(stream.isWriting)
		{
			if (transform.position != _position || transform.rotation != _rotation)
			{
				stream.Write(_position = transform.position);
				stream.Write(_rotation = transform.rotation);
			}
		}
		else if(stream.isReading)
		{
			Debug.Log("Reading Vector3...");
			transform.position = stream.Read<Vector3>();
			Debug.Log("Reading Quaternion...");
			transform.rotation = stream.Read<Quaternion>();
		}
	}
}
