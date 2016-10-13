using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace RTNetClient_Unity.Vectors
{
    [Serializable]
    internal class Vec3
    {
        public float x, y, z;

        public Vec3() { x = 0; y = 0; z = 0; }
        public Vec3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public Vector3 ToVector3() { return new Vector3(x, y, z); }
        public static Vec3 zero { get { return new Vec3(0, 0, 0); } }
        public static Vec3 FromVector3(Vector3 v) { return new Vec3(v.x, v.y, v.z); }

        public byte[] Data
        {
            get
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();

                bf.Serialize(ms, this);
                byte[] data = ms.ToArray();
                ms.Dispose();

                return data;
            }
        }

        public static Vec3 FromData(byte[] data) { return (Vec3)new BinaryFormatter().Deserialize(new MemoryStream(data)); }

        public static implicit operator Vector3(Vec3 v) { return v.ToVector3(); }
        public static implicit operator Vec3(Vector3 v) { return FromVector3(v); }
    }
}
