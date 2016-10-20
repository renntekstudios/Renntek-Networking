using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTNet
{
	internal class UnhandledPacket
	{
		public short PacketID { get; set; }
		public short Expected { get; set; }
		public Dictionary<short, byte[]> Bytes { get; private set; }

		public UnhandledPacket()
		{
			Bytes = new Dictionary<short, byte[]>();
		}

		public byte[] GetFinalBuffer()
		{
			if (Bytes.Count == 0)
				return new byte[0];
			List<byte> handled = new List<byte>();
			short i;
			for(i = 0; i < Expected; i++)
			{
				if (!Bytes.ContainsKey(i))
				{
					RTClient.Debug("Couldn't find " + i);
					break;
				}
				handled.AddRange(Bytes[i]);
			}
			if (i != Expected)
			{
				RTClient.Debug("Didn't get the expected amount of packets! Expected " + Expected + " but got " + i);
				return new byte[0];
			}
			Bytes.Clear();
			return handled.ToArray();
		}
	}
}
