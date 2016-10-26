using System;
using System.IO;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace RTNet
{
	internal enum UnityPackets : short
	{
		RPC = 101,
		Instantiate = 102,
		SerializedData = 103,
	}

	public enum RTReceiver : short { All = -1, Others = -2 }

	public class RTNetClient : RTClient
	{
		protected override void Log(string message) { Debug.Log("[RTNet][LOG] " + message); }
		protected override void LogDebug(string message) { if(DebugMode) Debug.Log("[RTNet][DEBUG] " + message); }
		protected override void LogWarning(string message) { Debug.LogWarning("[RTNet][WARNING] " + message); }
		protected override void LogError(string error) { Debug.LogError("[RTNet][ERROR] " + error); }
		protected override void LogException(Exception e, string error = "") { Debug.LogError("[RTNet][EXCEPTION] " + e.Message + (string.IsNullOrEmpty(error) ? "\n\t" : error + "\n\t") + e.StackTrace); }

		protected override void HandlePacket(short packetID, byte[] data)
		{
			switch(packetID)
			{
				case (short)UnityPackets.RPC:
					if (data != null)
					{
						string b = "";
						for (uint i = 0; i < data.Length; i++)
							b += data[i];
						// LogDebug("RPC_DATA: " + b);
						RTNetView.HandleRPC(RTNetRPC.FromData(data));
					}
					else
						LogError("RPC but data was null!");
					break;
				case (short)UnityPackets.Instantiate:
					RTNetView.HandleInstantiationRequest(InstantiateRequest.FromData(data));
					break;
				case (short)UnityPackets.SerializedData:
					RTNetView.HandleStream(data);
					break;
				default:
					handlePacket?.Invoke(packetID, data);
					if(handlePacket == null)
						LogDebug("Got unhandled packet with ID \"" + packetID + "\"");
					break;
			}
		}

		protected override void OnConnected() { onConnected?.Invoke(); }
		protected override void OnDisconnected() { onDisconnected?.Invoke(); }

		internal delegate void _onConnected();
		internal static event _onConnected onConnected;

		internal delegate void _onDisconnected();
		internal static event _onDisconnected onDisconnected;

		internal delegate void _handlePacket(short packetID, byte[] data);
		internal static event _handlePacket handlePacket;
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

		public static RTNetRPC FromData(byte[] data) { if (data == null) return null; return (RTNetRPC)new BinaryFormatter().Deserialize(new MemoryStream(data));	}
	}

	public class RTNetView : MonoBehaviour
	{
		public static RTNetClient Client { get; private set; }

		public int Port { get { return Client != null ? Client.Port : 0; } }
		public short ID { get { return Client != null ? Client.ID : (short)0; } }
		public bool Connected { get { return Client != null ? Client.Connected : false; } }
		public bool Timeout { get { return Client.Timeout; } set { Client.Timeout = value; } }
		public string Address { get { return Client != null ? Client.Address : string.Empty; } }
		public bool DebugMode { get { return Client.DebugMode; } set { Client.DebugMode = value; } }
		public int BufferSize { get { return Client.BufferSize; } set { Client.BufferSize = value; } }

		public bool isMine { get { return _ownerID == ID; } }
		public bool isPlayer;

		[SerializeField]
		public short ViewID = 0;
		private short _viewID;

		[SerializeField]
		public short OwnerID = 0;
		private short _ownerID = 0;

		private static List<short> viewIDs = new List<short>();
		private static List<byte[]> streams = new List<byte[]>();
		private static List<RTNetRPC> unhandledRPCs = new List<RTNetRPC>();
		private static List<InstantiateRequest> instantiationRequests = new List<InstantiateRequest>();

		void Awake()
		{
			while (viewIDs.Contains(ViewID))
				ViewID++;
			_viewID = ViewID;
			viewIDs.Add(ViewID);
			if (Client == null)
				Client = new RTNetClient();
			if (_ownerID < 0)
				_ownerID = ID;
			OwnerID = _ownerID;

			foreach(RTNetBehaviour b in GetComponents<RTNetBehaviour>())
				b.Init();
		}

		public void RPC(string method, RTReceiver receiver) { RPC(method, (short)receiver, new object[0]); }
		public void RPC(string method, short receiver) { RPC(method, receiver, new object[0]); }
		public void RPC(string method, RTReceiver receiver, params object[] args) { RPC(method, (short)receiver, args); }
		public void RPC(string method, short receiver, params object[] args)
		{
			RTNetRPC rpc = new RTNetRPC();
			rpc.SenderID = Client.ID;
			rpc.ViewID = ViewID;
			rpc.Receiver = receiver;

			for(int i = 0; i < args.Length; i++)
			{
				if (args[i].GetType().Equals(typeof(Vector2)))
					args[i] = (Vec2)(Vector2)args[i];
				else if (args[i].GetType().Equals(typeof(Vector3)))
					args[i] = (Vec3)(Vector3)args[i];
				else if (args[i].GetType().Equals(typeof(Vector4)))
					args[i] = (Vec4)(Vector4)args[i];
				else if (args[i].GetType().Equals(typeof(Quaternion)))
					args[i] = (Vec4)(Quaternion)args[i];
				else if (args[i].GetType().Equals(typeof(Color)))
					args[i] = (Vec4)(Color)args[i];
			}

			rpc.Method = method;
			rpc.Args = args;

			if (Client == null)
				Debug.LogWarning("Tried sending RPC but no RTNetUnityClient exists");
			else
			{
				byte[] rpc_data = rpc.Data;

				string b = "";
				for (uint i = 0; i < rpc_data.Length; i++)
					b += rpc_data[i];
				// Debug.Log("RPC_DATA: " + b);

				Client.Send((short)UnityPackets.RPC, rpc_data);
			}
		}

		internal void SendStream(ref RTStream stream, short index)
		{
			List<byte> toSend = new List<byte>();
			toSend.AddRange(BitConverter.GetBytes(ViewID));
			toSend.AddRange(BitConverter.GetBytes(index));
			toSend.AddRange(BitConverter.GetBytes(ID));
			toSend.AddRange(stream.GetData());
			Client.Send((short)UnityPackets.SerializedData, toSend.ToArray());
		}

		void Update()
		{
			#region Handle RPCs
			MonoBehaviour[] behaviours = gameObject.GetComponents<MonoBehaviour>();
			bool found = false;
			for (int i = 0; i < unhandledRPCs.Count; i++)
			{
				if (unhandledRPCs[i].ViewID != ViewID)
					continue;
				switch (unhandledRPCs[i].Receiver)
				{
					case (short)RTReceiver.All:
					default:
						break;
					case (short)RTReceiver.Others:
						if (unhandledRPCs[i].SenderID == Client.ID)
							continue;
						break;
				}

				foreach (MonoBehaviour b in behaviours)
				{
					if (found)
						break;
					foreach (MethodInfo method in b.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
					{
						if (method.Name.ToLower().Equals(unhandledRPCs[i].Method.ToLower()))
						{
							try
							{
								method.Invoke(method.IsStatic ? null : b, unhandledRPCs[i].Args);
							}
							catch (Exception e)
							{
								string m = method.Name + "(";
								for (int a = 0; a < unhandledRPCs[i].Args.Length; a++)
									m += unhandledRPCs[i].Args[a].GetType().Name + (a < unhandledRPCs[i].Args.Length - 1 ? ", " : "");
								m += ")";
								Debug.LogWarning("[RTNet][WARNING] Could not invoke \"" + m + "\" - " + e.Message);
							}
							found = true;
							break;
						}
					}
				}
				if (found)
					found = false;
				else
					Debug.LogWarning("[RTNet][WARNING] Could not find RPC method \"" + unhandledRPCs[i].Method + "\"");
				unhandledRPCs.RemoveAt(i);
			}
			#endregion
			#region Handle Instantiation Requests
			for (int i = 0; i < instantiationRequests.Count; i++)
			{
				if (instantiationRequests[i].BeingHandled)
					continue;
				instantiationRequests[i].BeingHandled = true;
				GameObject prefab = Resources.Load<GameObject>(instantiationRequests[i].Prefab);
				if(prefab == null)
				{
					Debug.LogWarning("[RTNet][WARNING] Could not find prefab \"" + instantiationRequests[i].Prefab + "\" to instantiate");
					instantiationRequests.RemoveAt(i);
					continue;
				}
				GameObject instantiated = (GameObject)Instantiate(prefab, instantiationRequests[i].Position, instantiationRequests[i].Rotation);
				if(instantiationRequests[i].Scale != new Vector3(1, 1, 1))
					instantiated.transform.localScale = instantiationRequests[i].Scale;
				if (instantiated.GetComponent<RTNetView>())
					instantiated.GetComponent<RTNetView>().OwnerID = instantiated.GetComponent<RTNetView>()._ownerID = instantiationRequests[i].SenderID;
				instantiationRequests.RemoveAt(i);
			}
			#endregion
			#region Handle Streams
			/*
			 * ViewID
			 * ClientID
			 * BehaviourIndex
			 * Stream_data
			*/
			RTNetBehaviour[] netBehaviours = gameObject.GetComponents<RTNetBehaviour>();
			found = false;
			short viewID, clientID, behaviourIndex;
			for(int i = 0; i < streams.Count; i++)
			{
				viewID = BitConverter.ToInt16(streams[i], 0);
				behaviourIndex = BitConverter.ToInt16(streams[i], sizeof(short));
				clientID = BitConverter.ToInt16(streams[i], sizeof(short) * 2);
				if (viewID != ViewID || clientID == ID)
					continue;
				foreach(RTNetBehaviour r in netBehaviours)
				{
					if (found)
						break;
					if (behaviourIndex != r.index)
						continue;
					r._internal_sync_stream(streams[i].Skip(sizeof(short) * 3).ToArray());
					found = true;
				}
				if (found)
					found = false;
				else
					Debug.LogWarning("[RTNet][WARNING] Could not find RTNetBehaviour with internal index '" + behaviourIndex + "'");
				streams.RemoveAt(i);
			}
			#endregion
			if (ViewID != _viewID)
				ViewID = _viewID;
		}

		internal static void HandleRPC(RTNetRPC rpc)
		{
			for(int i = 0; i < rpc.Args.Length; i++)
			{
				if (rpc.Args[i].GetType().Equals(typeof(Vec2)))
					rpc.Args[i] = (Vector2)(Vec2)rpc.Args[i];
				else if (rpc.Args[i].GetType().Equals(typeof(Vec3)))
					rpc.Args[i] = (Vector3)(Vec3)rpc.Args[i];
				else if (rpc.Args[i].GetType().Equals(typeof(Vec4)))
				{
					if (((Vec4)rpc.Args[i]).Type == Vec4.Vec4Type.Quaternion)
						rpc.Args[i] = (Quaternion)(Vec4)rpc.Args[i];
					else if (((Vec4)rpc.Args[i]).Type == Vec4.Vec4Type.Color)
						rpc.Args[i] = (Color)(Vec4)rpc.Args[i];
					else
						rpc.Args[i] = (Vector4)(Vec4)rpc.Args[i];
				}
			}
			// Debug.Log("Got RPC \"" + rpc.Method + "\"");
			unhandledRPCs.Add(rpc);
		}
		internal static void HandleStream(byte[] data) { streams.Add(data); }

		public void Connect(string ip, int port, int tcpPort = 0) { Client.Connect(ip, port, tcpPort); }
		public void Disconnect(bool sendPacket = true) { Client.Disconnect(sendPacket); }

		public int Send(RTPacketID packet) { return Client.Send(packet); }
		public int Send(short packet) { return Client.Send(packet); }
		public int Send(RTPacketID packet, byte[] data) { return Client.Send(packet, data); }
		public int Send(short packet, byte[] data) { return Client.Send(packet, data); }

		public void OnApplicationQuit()
		{
			if(Client != null && Connected)
				Client.Disconnect();
		}

		/// <summary>
		/// Spawns a prefab with the given name from the Resources folder
		/// </summary>
		/// <param name="prefabName">Name of the prefab to instantiate</param>
		public GameObject NetworkInstantiate(string prefabName) { return NetworkInstantiate(prefabName, Vector3.zero, new Vector3(1, 1, 1), Quaternion.identity); }
		/// <summary>
		/// Spawns a prefab with the given name from the Resources folder
		/// </summary>
		/// <param name="prefabName">Name of the prefab to instantiate</param>
		/// <param name="position">Position to spawn the prefab at</param>
		public GameObject NetworkInstantiate(string prefabName, Vector3 position) { return NetworkInstantiate(prefabName, position, new Vector3(1, 1, 1), Quaternion.identity); }
		/// <summary>
		/// Spawns a prefab with the given name from the Resources folder
		/// </summary>
		/// <param name="prefabName">Name of the prefab to instantiate</param>
		/// <param name="position">Position to spawn the prefab at</param>
		/// <param name="rotation">Rotation of the spawned prefab</param>
		public GameObject NetworkInstantiate(string prefabName, Vector3 position, Quaternion rotation) { return NetworkInstantiate(prefabName, position, new Vector3(1, 1, 1), rotation); }
		/// <summary>
		/// Spawns a prefab with the given name from the Resources folder
		/// </summary>
		/// <param name="prefabName">Name of the prefab to instantiate</param>
		/// <param name="position">Position to spawn the prefab at</param>
		/// <param name="scale">Scale of the spawned prefab</param>
		public GameObject NetworkInstantiate(string prefabName, Vector3 position, Vector3 scale) { return NetworkInstantiate(prefabName, position, scale, Quaternion.identity); }

		/// <summary>
		/// Spawns a prefab with the given name from the Resources folder
		/// </summary>
		/// <param name="prefabName">Name of the prefab to instantiate</param>
		/// <param name="position">Position to spawn the prefab at</param>
		/// <param name="scale">Scale of the spawned prefab</param>
		/// <param name="rotation">Rotation of the spawned prefab</param>
		public GameObject NetworkInstantiate(string prefabName, Vector3 position, Vector3 scale, Quaternion rotation)
		{
			if(Resources.Load<GameObject>(prefabName) == null)
			{
				Debug.LogWarning("[RTNet][WARNING] Could not instantiate \"" + prefabName + "\" over network - resource not found");
				return;
			}

			byte[] data = new InstantiateRequest(prefabName, position, scale, rotation, ID).Data;
			Send((short)UnityPackets.Instantiate, data);
                        GameObject go = Resources.Load<GameObject>(prefabName, position, rotation);
                        go.transform.localScale = scale;
                        if(go.GetComponent<RTNetView>())
                                go.GetComponent<RTNetView>()._ownerID = go.GetComponent<RTNetView>().OwnerID = ID;
                        return go;
		}

		internal static void HandleInstantiationRequest(InstantiateRequest request) { instantiationRequests.Add(request); }

		private void _internal_destroy()
		{
			DestroyImmediate(gameObject);
		}

		public void NetworkDestroy(RTReceiver receiver = RTReceiver.All)
		{
			RPC("_internal_destroy", receiver);
		}

		public void NetworkDestroy(short receiver)
		{
			RPC("_internal_destroy", receiver);
		}

		public RTServerInfo GetLocalServers(int port = 4434) { return Client.DiscoverLAN(port); }
	}

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

	[Serializable]
	internal class Vec4 // Quaternions can also be converted to Vec4b
	{
		internal enum Vec4Type { Normal, Quaternion, Color }
		public Vec4Type Type { get; internal set; }
		public float x, y, z, w;

		public Vec4() { x = 0; y = 0; z = 0; w = 0; }
		public Vec4(Vector4 v) { x = v.x; y = v.y; z = v.z; w = v.w; }
		public Vec4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }

		public Vector4 ToVector4() { return new Vector4(x, y, z, w); }
		public Quaternion ToQuaternion() { return new Quaternion(x, y, z, w); }
		public Color ToColor() { return new Color(x, y, z, w); }
		public static Vec4 zero { get { return new Vec4(0, 0, 0, 0); } }
		public static Vec4 FromVector4(Vector4 v) { return new Vec4(v.x, v.y, v.z, v.w); }
		public static Vec4 FromQuaternion(Quaternion v) { return new Vec4(v.x, v.y, v.z, v.w) { Type = Vec4Type.Quaternion }; }
		public static Vec4 FromColor(Color c) { return new Vec4(c.r, c.g, c.b, c.a) { Type = Vec4Type.Color }; }

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
		public static implicit operator Color(Vec4 c) { return c.ToColor(); }

		public static implicit operator Vec4(Vector4 v) { return FromVector4(v); }
		public static implicit operator Vec4(Quaternion v) { return FromQuaternion(v); }
		public static implicit operator Vec4(Color c) { return FromColor(c); }
	}

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

		public InstantiateRequest(string prefab, short senderID) : this(prefab, Vec3.zero, Vec3.zero, Vec4.zero, senderID) { }
		public InstantiateRequest(string prefab, Vec3 position, Vec3 scale, Vec4 rotation, short senderID)
		{
			this.SenderID = senderID;
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
