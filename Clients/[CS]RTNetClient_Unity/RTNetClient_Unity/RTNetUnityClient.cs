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
		RPC = 101
	}

	public enum RTReceiver : short { All = -1, Others = -2 }

	public class RTNetClient : RTClient
	{
		protected override void Log(string message) { Debug.Log("[RTNet][LOG] " + message); }
		protected override void LogDebug(string message) { Debug.Log("[RTNet][DEBUG] " + message); }
		protected override void LogWarning(string message) { Debug.LogWarning("[RTNet][WARNING] " + message); }
		protected override void LogError(string error) { Debug.LogError("[RTNet][ERROR] " + error); }
		protected override void LogException(Exception e, string error = "") { Debug.LogError("[RTNet][EXCEPTION] " + e.Message + (string.IsNullOrEmpty(error) ? "\n\t" : error + "\n\t") + e.StackTrace); }

		protected override void HandlePacket(short packetID, byte[] data)
		{
			switch(packetID)
			{
				case (short)UnityPackets.RPC:
					RTNetView.HandleRPC(RTNetRPC.FromData(data));
					break;
				default:
					LogWarning("Got unhandled packet with ID \"" + packetID + "\"");
					break;
			}
		}
	}

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

		public static RTNetRPC FromData(byte[] data) { return (RTNetRPC)new BinaryFormatter().Deserialize(new MemoryStream(data));	}
	}

	public class RTNetView : MonoBehaviour
	{
		public static RTNetClient Client { get; private set; }
		public static short ID { get { return Client != null ? Client.ID : (short)0; } }

		public readonly short ViewID = 0;

		private static List<short> viewIDs = new List<short>();
		private static List<RTNetRPC> unhandledRPCs = new List<RTNetRPC>();

		public RTNetView()
		{
			while (viewIDs.Contains(ViewID))
				ViewID++;
			if (Client == null)
				Client = new RTNetClient();
		}

		public void RPC(string method, RTReceiver receiver, params object[] args) { RPC(method, (short)receiver, args); }
		public void RPC(string method, short receiver, params object[] args)
		{
			RTNetRPC rpc = new RTNetRPC();
			rpc.SenderID = Client.ID;
			rpc.ViewID = ViewID;
			rpc.Receiver = receiver;

			rpc.Method = method;
			rpc.Args = args;

			if (Client == null)
				Debug.LogWarning("Tried sending RPC but no RTNetUnityClient exists");
			else
				Client.Send((short)UnityPackets.RPC, rpc.Data);
		}

		void LateUpdate()
		{
			MonoBehaviour[] behaviours = gameObject.GetComponents<MonoBehaviour>();
			bool found = false;
			for (int i = 0; i < unhandledRPCs.Count; i++)
			{
				if(unhandledRPCs[i].ViewID == this.ViewID)
				{
					if (unhandledRPCs[i].Receiver == (short)RTReceiver.All || (unhandledRPCs[i].Receiver == (short)RTReceiver.Others && unhandledRPCs[i].SenderID != Client.ID) || unhandledRPCs[i].ViewID == Client.ID)
					{
						foreach (MonoBehaviour b in behaviours)
						{
							if (found)
								break;
							foreach (MethodInfo method in b.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
							{
								if (method.Name.ToLower().Equals(unhandledRPCs[i].Method.ToLower()))
								{
									method.Invoke(behaviours[i], unhandledRPCs[i].Args);
									found = true;
									break;
								}
							}
						}
					}
				}
				if(found)
				{
					unhandledRPCs.RemoveAt(i);
					found = false;
				}
			}
		}

		internal static void HandleRPC(RTNetRPC rpc) { unhandledRPCs.Add(rpc); }

		public void OnApplicationQuit()
		{
			Client.Disconnect();
		}
	}
}
