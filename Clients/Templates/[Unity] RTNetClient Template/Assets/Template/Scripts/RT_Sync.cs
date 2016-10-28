using UnityEngine;
using System.Collections;
using RTNet;

public class RT_Sync : RTNetBehaviour
{
    protected override void OnSerializeView(ref RTStream stream)
    {
        if (stream.isWriting)
        {
            stream.Write(transform.position);
            stream.Write(transform.rotation);
        }
        else if (stream.isReading)
        {
            transform.position = stream.Read<Vector3>();
            transform.rotation = stream.Read<Quaternion>();
        }
    }
}
