using System;
using System.Runtime.Serialization.Formatters.Binary;
using RTNetClient_Unity.Vectors;
using System.IO;

namespace RTNetClient_Unity
{
    [Serializable]
    internal class InstantiateRequest
    {
        [NonSerialized]
        public bool BeingHandled;

        public short SenderID;
        public string Prefab;
        public Vec3 Position;
        public Vec3 Scale;
        public Vec4 Rotation;

        public InstantiateRequest(string prefab) : this(prefab, Vec3.zero, Vec3.zero, Vec4.zero) { }
        public InstantiateRequest(string prefab, Vec3 position, Vec3 scale, Vec4 rotation)
        {
            this.SenderID = RTNetView.ID;
            this.Prefab = prefab;
            this.Position = position;
            this.Scale = scale;
            this.Rotation = rotation;
        }

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

        public static InstantiateRequest FromData(byte[] data) { return (InstantiateRequest)new BinaryFormatter().Deserialize(new MemoryStream(data)); }
    }
}
