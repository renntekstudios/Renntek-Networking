using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class Template_SimpleSync : RTNetBehaviour
{
	public enum SyncType { None, Interpolate }

	public SyncType syncType = SyncType.None;

	private Vector3 _position, _scale;
	private Quaternion _rotation;

	protected override void OnSerializeView(ref RTStream stream)
	{
		if(stream.isWriting)
		{
			if(transform.position != _position || transform.rotation != _rotation || transform.localScale != _scale)
			{
				stream.Write(_position = transform.position);
				stream.Write(_rotation = transform.rotation);
				stream.Write(_scale = transform.localScale);
			}
			else
			{
				// switch(syncType)
				transform.position = stream.Read<Vector3>();
				transform.rotation = stream.Read<Quaternion>();
				transform.localScale = stream.Read<Vector3>();
			}
		}
	}
}
