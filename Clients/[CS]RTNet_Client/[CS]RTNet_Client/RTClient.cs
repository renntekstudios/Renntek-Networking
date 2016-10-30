using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace RTNet
{
	public enum RTConnectionStatus { Disconnected, Connecting, Connected }
	internal enum InternalPacketID : short
	{
		LANDiscovery = 3
	}
	public enum RTPacketID : short
	{
		Disconnect = 1,
		Auth = 2
	}

	public class RTClient
	{
		private static Type classType;
		private enum RTClientSignatures : byte { Server = 99, CSharp = 1 }

		private const int ReceiveTimeout = 30000;
		private int bufferSize = 512;
		public int BufferSize { get { return bufferSize; } set { if (!Connected) bufferSize = value; else LogWarning("Cannot change buffer size while connected to server!"); } }

		private bool timeout;
		public bool Timeout { get { return timeout; } set { if (Status != RTConnectionStatus.Disconnected) return; else timeout = value; } }
		public string Address { get { return EndPoint == null ? "" : ((IPEndPoint)EndPoint).Address.ToString(); } }
		public int Port { get { return EndPoint == null ? 0 : ((IPEndPoint)EndPoint).Port; } }
		public bool DebugMode { get; set; }
		
		public short ID { get; private set; }
		public RTConnectionStatus Status { get; private set; }
		public bool Connected { get { return Status != RTConnectionStatus.Disconnected; } }

		private Socket Socket { get; set; }
		private TcpClient ReliableSocket { get; set; }
		private EndPoint _endpoint;
		private EndPoint EndPoint { get { return _endpoint; } }

		private byte[] _buffer;
		// private Thread receiveThread;
		private List<short> internalIDs = new List<short>();

		private List<RTServerInfo> servers = new List<RTServerInfo>();

		public RTClient()
		{
			Status = RTConnectionStatus.Disconnected;
			classType = this.GetType();
		}

		public RTClient(string ip, int port)
		{
			Status = RTConnectionStatus.Disconnected;
			classType = this.GetType();
			Connect(ip, port);
		}

		public void Connect(string ip, int port, int tcpPort = 0)
		{
			if(string.IsNullOrEmpty(ip) || port <= 0)
			{
				LogWarning("Cannot connect to server - invalid address and/or port");
				return;
			}

			OnConnecting();

			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (Timeout)
				Socket.ReceiveTimeout = ReceiveTimeout;
			_endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

			ReliableSocket = new TcpClient();
			if (Timeout)
				ReliableSocket.ReceiveTimeout = ReceiveTimeout;

			Status = RTConnectionStatus.Connecting;

			// ReliableSocket.Connect(ip, tcpPort == 0 ? port + 1 : tcpPort);

			byte[] signature = { 17, 19, (byte)RTClientSignatures.CSharp };
			Socket.SendTo(signature, EndPoint);
			BeginReceive();
		}

		public void Disconnect(bool sendPacket = true)
		{
			if(sendPacket)
				Send(RTPacketID.Disconnect);

			Status = RTConnectionStatus.Disconnected;
			Socket.Close();
			_endpoint = null;
			OnDisconnected();
			Log("Disconnected from server");
		}

		private Dictionary<short, UnhandledPacket> unhandledPackets = new Dictionary<short, UnhandledPacket>();
		private byte[] SortData(byte[] buffer, int bytesRead)
		{
			short packet_status = BitConverter.ToInt16(buffer, 0);
			short packet_internal_id = BitConverter.ToInt16(buffer, sizeof(short));
			short packet_index = BitConverter.ToInt16(buffer, sizeof(short) * 2);

			// buffer = buffer.Take(bytesRead).Skip(sizeof(short) * 3).ToArray();
			byte[] data = new byte[bytesRead];
			for (int i = 0; i < bytesRead; i++)
				data[i] = buffer[i + (sizeof(short) * 3)];
			// LogDebug("STATUS: " + packet_status + "; INTERNAL_ID: " + packet_internal_id + "; INDEX: " + packet_index);
			
			if(packet_status == -1)
			{
				if(unhandledPackets.ContainsKey(packet_internal_id))
				{
					unhandledPackets[packet_internal_id].Bytes.Add(packet_index, data);
					if (unhandledPackets[packet_internal_id].Expected > 0 && unhandledPackets[packet_internal_id].Bytes.Count == unhandledPackets[packet_internal_id].Expected)
					{
						data = unhandledPackets[packet_internal_id].GetFinalBuffer();
						if (data == null || data.Length == 0)
							return new byte[0];
						unhandledPackets.Remove(packet_internal_id);
					}
					else
						return new byte[0];
				}
				else
				{
					UnhandledPacket packet = new UnhandledPacket();
					packet.PacketID = packet_internal_id;
					packet.Bytes.Add(packet_index, data);
					unhandledPackets.Add(packet_internal_id, packet);
					return new byte[0];
				}
			}
			else if(packet_status == -2)
			{
				if(unhandledPackets.ContainsKey(packet_internal_id))
				{
					unhandledPackets[packet_internal_id].Bytes.Add(packet_index, data);
					int expected = unhandledPackets[packet_internal_id].Expected = (short)(packet_index + 1);
					if (expected > 0 && unhandledPackets[packet_internal_id].Bytes.Count == expected)
					{
						data = unhandledPackets[packet_internal_id].GetFinalBuffer();
						if (data == null || data.Length == 0)
							return new byte[0];
						unhandledPackets.Remove(packet_internal_id);
					}
					else
						return new byte[0];
				}
				else
				{
					UnhandledPacket packet = new UnhandledPacket();
					packet.PacketID = packet_internal_id;
					packet.Bytes.Add(packet_index, data);
					unhandledPackets.Add(packet_internal_id, packet);
					return new byte[0];
				}
			}
			return data;
		}

		private void BeginReceive()
		{
			_buffer = new byte[BufferSize];
			Socket.BeginReceiveFrom(_buffer, 0, BufferSize, SocketFlags.None, ref _endpoint, Receive, null);
		}

		private void Receive(IAsyncResult result)
		{
			if (!Connected)
				return;
			try
			{
				int bytesRead = Socket.EndReceiveFrom(result, ref _endpoint);
				byte[] buffer = _buffer;
				if (bytesRead > 0)
				{
					if (bytesRead == 3)
					{
						if (buffer[0] != (byte)17 || buffer[1] != (byte)19 || buffer[2] != (byte)RTClientSignatures.Server)
						{
							LogError("Server has invalid signature!");
							Disconnect();
							BeginReceive();
							return;
						}
						OnConnected();
						Status = RTConnectionStatus.Connecting;
						BeginReceive();
						return;
					}

					// LogDebug("Got " + bytesRead + " bytes");
					//string s = "";
					//for (int i = 0; i < bytesRead; i++)
						// s += buffer[i] + " ";
					// LogDebug("Sent (" + bytesRead + " bytes) " + s);

					buffer = SortData(buffer, bytesRead);
					if (buffer.Length == 0)
					{
						BeginReceive();
						return;
					}

					short packetID = BitConverter.ToInt16(buffer, 0);
					// LogDebug("Got \"" + packetID + "\" packet (" + buffer.Length + " bytes)");
					buffer = buffer.Skip(sizeof(short)).ToArray();
					switch ((RTPacketID)packetID)
					{
						case RTPacketID.Disconnect:
							Disconnect(false);
							break;
						case RTPacketID.Auth:
							ID = BitConverter.ToInt16(buffer, 0);
							Status = RTConnectionStatus.Connected;
							Log("Connected to server (ID: " + ID + ")");
							break;
						default:
							HandlePacket(packetID, buffer);
							break;
					}
				} 
				else
					Disconnect();
			}
			catch(Exception e)
			{
				if (!Timeout && e.Message.Contains("A blocking operation"))
				{
					BeginReceive();
					return;
				}
				LogError("Could not receive from server, \"" + e.GetType().Name + "\" exception - " + e.Message + "\n\t" + e.StackTrace);
				// Disconnect();
			}
			BeginReceive();
		}

		private short GetInternalPacketID()
		{
			short index = 0;
			while (internalIDs.Contains(++index)) ;
			return index;
		}

		public int Send(RTPacketID packetID) { return Send((short)packetID); }
		public int Send(short packetID)
		{
			if(!Connected)
			{
				LogWarning("Tried sending packet when disconnected from server");
				return -1;
			}
			short internalPacketID = GetInternalPacketID();
			List<byte> toSend = new List<byte>();
			toSend.AddRange(BitConverter.GetBytes((short)-3));
			toSend.AddRange(BitConverter.GetBytes(internalPacketID));
			toSend.AddRange(BitConverter.GetBytes((short)0));
			toSend.AddRange(BitConverter.GetBytes((short)packetID));

			int result = Socket.SendTo(toSend.ToArray(), EndPoint);
			internalIDs.Remove(internalPacketID);
			return result;
		}

		public int Send(RTPacketID packetID, byte[] buffer) { return Send((short)packetID, buffer); }
		public int Send(short packetID, byte[] buffer)
		{
			if(!Connected)
			{
				LogWarning("Tried sending packet when disconnected from server");
				return -1;
			}
			short internalPacketID = GetInternalPacketID(), index = 0;
			List<byte> toSend = BitConverter.GetBytes(packetID).ToList();
			toSend.AddRange(buffer);
			buffer = toSend.ToArray();
			toSend.Clear();

			// string s = "";

			int packetSize = sizeof(short) * 3;
			if (buffer.Length > BufferSize - packetSize)
			{
				while (buffer.Length > BufferSize - packetSize)
				{
					toSend.AddRange(BitConverter.GetBytes((short)-1));
					toSend.AddRange(BitConverter.GetBytes(internalPacketID));
					toSend.AddRange(BitConverter.GetBytes(index++));
					toSend.AddRange(buffer.Take(BufferSize - packetSize));

					Socket.SendTo(toSend.ToArray(), EndPoint);
					// LogDebug("Sent " + toSend.Count + " bytes");
					// s = "";
					// for (int i = 0; i < toSend.Count; i++)
						// s += toSend[i] + " ";
					// LogDebug("Received (" + toSend.Count + " bytes) " + s);

					toSend.Clear();
					buffer = buffer.Skip(BufferSize - packetSize).ToArray();
				}
				toSend.AddRange(BitConverter.GetBytes((short)-2));
				toSend.AddRange(BitConverter.GetBytes(internalPacketID));
				toSend.AddRange(BitConverter.GetBytes(index++));
				toSend.AddRange(buffer);
			}
			else
			{
				toSend.AddRange(BitConverter.GetBytes((short)-3));
				toSend.AddRange(BitConverter.GetBytes(internalPacketID));
				toSend.AddRange(BitConverter.GetBytes(index));
				toSend.AddRange(buffer);
			}

			int result = Socket.SendTo(toSend.ToArray(), EndPoint);
			internalIDs.Remove(internalPacketID);
			// LogDebug("Sent " + toSend.Count + " bytes");
			// s = "";
			// for (int i = 0; i < toSend.Count; i++)
				// s += toSend[i] + " ";
			// LogError("Sent (" + toSend.Count + " bytes) " + s);
			return result;
		}

		private int SendRaw(byte[] buffer)
		{
			return Socket.SendTo(buffer, EndPoint);
		}

		internal protected virtual void HandlePacket(short packetID, byte[] data) { }

		/// <summary>
		/// Searches for any RTNet servers in the local network on the given port
		/// <param name="port">Port that any server may be on</param>
		/// </summary>
		public RTServerInfo DiscoverLAN(int port, int timeout = 3)
		{
			RTServerInfo info = null;
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.EnableBroadcast = true;
			s.ReceiveTimeout = timeout * 1000;
			EndPoint ep = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);
			s.SendTo(new byte[] { (byte)17, (byte)19, (byte)(short)InternalPacketID.LANDiscovery }, ep);

			byte[] buffer = new byte[3];
			ep = new IPEndPoint(IPAddress.Any, port);
			s.ReceiveFrom(buffer, ref ep);
			if (buffer[0] == (byte)17 && buffer[1] == (byte)19 && buffer[2] == (byte)(short)InternalPacketID.LANDiscovery)
				info = new RTServerInfo(((IPEndPoint)ep).Address.ToString(), ((IPEndPoint)ep).Port, true);
			return info;
		}

		/// <summary>
		/// Called just before trying to connect to a server
		/// </summary>
		protected virtual void OnConnecting() { }
		/// <summary>
		/// Called when connected to a server
		/// </summary>
		protected virtual void OnConnected() { }
		/// <summary>
		/// Called once disconnected from a server
		/// </summary>
		protected virtual void OnDisconnected() { }

		#region Logging
		internal protected virtual void Log(string message) { }
		internal void _internal_debug(string message) { if (DebugMode) LogDebug(message); }
		internal protected virtual void LogDebug(string message) { }
		internal protected virtual void LogWarning(string message) { }
		internal protected virtual void LogError(string error) { }
		internal protected virtual void LogException(Exception e, string error = "") { }

		internal static void Debug(string msg)
		{
			RTClient c = (RTClient)Activator.CreateInstance(classType);
			c._internal_debug(msg);
		}
		#endregion
	}

	public class RTServerInfo
	{
		/// <summary>
		/// Contains the IP address of the server
		/// </summary>
		public string IP { get; internal set; }
		/// <summary>
		/// Contains the port number of the server
		/// </summary>
		public int Port { get; internal set; }

		/// <summary>
		/// Returns true if the server is on the local network
		/// </summary>
		public bool isLocal { get; internal set; }

		internal RTServerInfo(string ip, int port)
		{
			this.IP = ip;
			this.Port = port;
			this.isLocal = false;
		}

		internal RTServerInfo(string ip, int port, bool local)
		{
			this.IP = ip;
			this.Port = port;
			this.isLocal = local;
		}
	}
}