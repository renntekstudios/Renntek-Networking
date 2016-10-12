using System;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace RTNet
{
	internal enum UnityPackets : short
	{
		RPC = 101,
		Instantiate = 102
	}

	public enum RTReceiver : short
    {
        All = -1,
        Others = -2
    };
}
