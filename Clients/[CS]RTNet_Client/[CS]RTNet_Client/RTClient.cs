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
	public enum RTPacketID : short
	{
		Disconnect = 1
	}

	public class RTClient
	{
		private enum RTClientSignatures : byte { Server = 99, CSharp = 1 }

		private const int ReceiveTimeout = 30000;
		private int bufferSize = 512;
		internal protected int BufferSize { get { return bufferSize; } set { if (Connected) bufferSize = value; else LogWarning("Cannot change buffer size while connected to server!"); } }

		private bool timeout;
		public bool Timeout { get { return timeout; } set { if (Status != RTConnectionStatus.Disconnected) return; else timeout = value; } }
		public string Address { get { return EndPoint == null ? "" : ((IPEndPoint)EndPoint).Address.ToString(); } }
		public int Port { get { return EndPoint == null ? 0 : ((IPEndPoint)EndPoint).Port; } }

		// TODO: Send ID to client when connection established
		public short ID { get; private set; }
		public RTConnectionStatus Status { get; private set; }
		public bool Connected { get { return Status != RTConnectionStatus.Disconnected; } }

		private Socket Socket { get; set; }
		private TcpClient ReliableSocket { get; set; }
		private EndPoint _endpoint;
		private EndPoint EndPoint { get { return _endpoint; } }

		private Thread receiveThread;
		private List<short> internalIDs = new List<short>();

		public RTClient()
		{
			Status = RTConnectionStatus.Disconnected;
		}

		public RTClient(string ip, int port)
		{
			Status = RTConnectionStatus.Disconnected;
			Connect(ip, port);
		}

		public void Connect(string ip, int port, int tcpPort = 0)
		{
			if(string.IsNullOrEmpty(ip) || port <= 0)
			{
				LogWarning("Cannot connect to server - invalid address and/or port");
				return;
			}

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
			receiveThread = new Thread(Receive);
			receiveThread.Start();
		}

		public void Disconnect(bool sendPacket = true)
		{
			if(sendPacket)
				Send(RTPacketID.Disconnect);

			Status = RTConnectionStatus.Disconnected;
			Socket.Close();
			_endpoint = null;
			Log("Disconnected from server");
		}

		private void Receive()
		{
			try
			{
				while(Connected)
				{
					byte[] buffer = new byte[BufferSize];
					int bytesRead = Socket.ReceiveFrom(buffer, ref _endpoint);
					if(bytesRead > 0)
					{
						if(bytesRead == 3)
						{
							if(buffer[0] != (byte)17 || buffer[1] != (byte)19 || buffer[2] != (byte)RTClientSignatures.Server)
							{
								LogError("Server has invalid signature!");
								Status = RTConnectionStatus.Disconnected;
								return;
							}
							Log("Connected to server");
							Status = RTConnectionStatus.Connected;
							continue;
						}

						LogDebug("Got \"" + BitConverter.ToInt16(buffer, sizeof(short) * 3) + "\" packet (" + bytesRead + " bytes)");
						short packetID = BitConverter.ToInt16(buffer, sizeof(short) * 3);
						buffer = buffer.Skip(sizeof(short) * 4).ToArray();
						switch((RTPacketID)packetID)
						{
							case RTPacketID.Disconnect:
								Disconnect(false);
								break;
							default:
								HandlePacket(packetID, buffer);
								break;
						}
					}
					else
					{
						Disconnect();
						break;
					}
				}
			}
			catch(Exception e)
			{
				if (!Timeout && e.Message.Contains("A blocking operation"))
					return;
				// LogException(e, "Could not receive from server");
				LogError("Could not receive from server - " + e.Message);
				Disconnect();
			}
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
			List<byte> toSend = BitConverter.GetBytes((short)packetID).ToList();
			toSend.AddRange(buffer);
			buffer = toSend.ToArray();
			toSend.Clear();

			int packetSize = sizeof(short) * 3;
			while (buffer.Length > BufferSize - packetSize)
			{
				toSend.AddRange(BitConverter.GetBytes((short)-1));
				toSend.AddRange(BitConverter.GetBytes(internalPacketID));
				toSend.AddRange(BitConverter.GetBytes(index++));
				toSend.AddRange(buffer.Take(BufferSize - packetSize));

				Socket.SendTo(toSend.ToArray(), EndPoint);
				toSend.Clear();
				buffer = buffer.Skip(BufferSize - packetSize).ToArray();
			}
			if(index > 0)
			{
				toSend.AddRange(BitConverter.GetBytes((short)-2));
				toSend.AddRange(BitConverter.GetBytes(internalPacketID));
				toSend.AddRange(BitConverter.GetBytes(index++));
				toSend.AddRange(buffer);
			}
			else
			{
				toSend.AddRange(BitConverter.GetBytes((short)-3));
				toSend.AddRange(BitConverter.GetBytes(internalPacketID));
				toSend.AddRange(BitConverter.GetBytes(index++));
				toSend.AddRange(buffer);
			}

			int result = Socket.SendTo(toSend.ToArray(), EndPoint);
			internalIDs.Remove(internalPacketID);
			return result;
		}

		private int SendRaw(byte[] buffer)
		{
			return Socket.SendTo(buffer, EndPoint);
		}

		internal protected virtual void HandlePacket(short packetID, byte[] data) { }

		#region Logging
		internal protected virtual void Log(string message) { }
		internal protected virtual void LogDebug(string message) { }
		internal protected virtual void LogWarning(string message) { }
		internal protected virtual void LogError(string error) { }
		internal protected virtual void LogException(Exception e, string error = "") { }
		#endregion
	}
}