using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RTNetClient_Unity.Vectors
{
    [Serializable]
    internal class Vec2
    {
        public float x, y;

        public Vec2() { x = 0; y = 0; }
        public Vec2(Vector2 v) { x = v.x; y = v.y; }
        public Vec2(float x, float y) { this.x = x; this.y = y; }

        public Vector3 ToVector2() { return new Vector3(x, y); }
        public static Vec2 zero { get { return new Vec2(0, 0); } }
        public static Vec2 FromVector2(Vector2 v) { return new Vec2(v.x, v.y); }

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

        public static Vec2 FromData(byte[] data) { return (Vec2)new BinaryFormatter().Deserialize(new MemoryStream(data)); }

        public static implicit operator Vector2(Vec2 v) { return v.ToVector2(); }
        public static implicit operator Vec2(Vector2 v) { return FromVector2(v); }
    }
}