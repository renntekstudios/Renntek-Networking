using System;
using RTNet;

namespace TestClient
{
	public class TestClient : RTClient
	{
		protected override void HandlePacket(short packetID, byte[] data)
		{
			LogDebug("Got \"" + packetID + "\" packet (" + data.Length + " bytes)");
		}

		protected override void Log(string message) { Console.WriteLine("[LOG] " + message); }
		protected override void LogDebug(string message) { Console.WriteLine("[DEBUG] " + message); }
		protected override void LogWarning(string message) { Console.WriteLine("[WARNING] " + message); }
		protected override void LogError(string error) { Console.WriteLine("[ERROR] " + error); }
		protected override void LogException(Exception e, string error) { Console.WriteLine("[EXCEPTION] " + e.Message + " (" + error + ")\n\t" + e.StackTrace); }
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "RennTek Client Console";
			TestClient client = new TestClient();
			client.Timeout = false;
			client.Connect("127.0.0.1", 4434);

			string line;
			while (client.Connected)
			{
				line = Console.ReadLine();
				if (line.ToLower() == "quit")
					break;
				else
					client.Send((RTPacketID)123, System.Text.Encoding.UTF8.GetBytes(line));
			}
			client.Disconnect();
		}
	}
}
