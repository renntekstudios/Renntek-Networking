using System;
using System.Threading;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace RTNet
{
	internal class RTNetClient : RTClient
	{
		protected override void Log(string message) { Debug.Log("[RTNet][LOG] " + message); }
		protected override void LogDebug(string message) { Debug.Log("[RTNet][DEBUG] " + message); }
		protected override void LogWarning(string message) { Debug.LogWarning("[RTNet][WARNING] " + message); }
		protected override void LogError(string error) { Debug.LogError("[RTNet][ERROR] " + error); }
		protected override void LogException(Exception e, string error = "") { Debug.LogError("[RTNet][EXCEPTION] " + e.Message + (string.IsNullOrEmpty(error) ? "\n\t" : error + "\n\t") + e.StackTrace); }

		protected override void HandlePacket(short packetID, byte[] data)
		{
			
		}
	}

    public class RTNetUnityClient : MonoBehaviour
	{
		internal static RTNetClient Client { get; private set; }

		public void Start()
		{
			if(FindObjectsOfType<RTNetUnityClient>().Length > 1)
			{
				Debug.LogError("MORE THAN ONE RTNETUNITYCLIENT!!");
				return;
			}
			Client = new RTNetClient();
		}
    }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class RTSyncAttribute : Attribute
	{
		public RTSyncAttribute()
		{
			
		}
	}
}
