using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace RTNetClient_Unity.Vectors
{
    [Serializable]
    internal class Vec4 // Quaternions can also be converted to Vec4
    {
        public float x, y, z, w;

        public Vec4() { x = 0; y = 0; z = 0; w = 0; }
        public Vec4(Vector4 v) { x = v.x; y = v.y; z = v.z; w = v.w; }
        public Vec4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }

        public Vector4 ToVector4() { return new Vector4(x, y, z, w); }
        public Quaternion ToQuaternion() { return new Quaternion(x, y, z, w); }
        public static Vec4 zero { get { return new Vec4(0, 0, 0, 0); } }
        public static Vec4 FromVector4(Vector4 v) { return new Vec4(v.x, v.y, v.z, v.w); }
        public static Vec4 FromQuaternion(Quaternion v) { return new Vec4(v.x, v.y, v.z, v.w); }

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

        public static Vec4 FromData(byte[] data) { return (Vec4)new BinaryFormatter().Deserialize(new MemoryStream(data)); }

        public static implicit operator Vector4(Vec4 v) { return v.ToVector4(); }
        public static implicit operator Quaternion(Vec4 v) { return v.ToQuaternion(); }

        public static implicit operator Vec4(Vector4 v) { return FromVector4(v); }
        public static implicit operator Vec4(Quaternion v) { return FromQuaternion(v); }
    }
}
