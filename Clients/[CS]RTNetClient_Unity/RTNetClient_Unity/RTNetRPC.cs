using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace RTNetClient_Unity
{
    [Serializable()]
    internal class RTNetRPC
    {
        public short SenderID;
        public short ViewID;
        public short Receiver;

        public string Method;
        public object[] Args;

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

        public static RTNetRPC FromData(byte[] data) { return (RTNetRPC)new BinaryFormatter().Deserialize(new MemoryStream(data)); }
    }
}
