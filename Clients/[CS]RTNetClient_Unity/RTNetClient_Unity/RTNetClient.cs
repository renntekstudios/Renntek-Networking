using System;
using System.Collections.Generic;
using UnityEngine;
using RTNet;

namespace RTNetClient_Unity
{
    public class RTNetClient : RTClient
    {
        protected override void Log(string message) { Debug.Log("[RTNet][LOG] " + message); }
        protected override void LogDebug(string message) { Debug.Log("[RTNet][DEBUG] " + message); }
        protected override void LogWarning(string message) { Debug.LogWarning("[RTNet][WARNING] " + message); }
        protected override void LogError(string error) { Debug.LogError("[RTNet][ERROR] " + error); }
        protected override void LogException(Exception e, string error = "") { Debug.LogError("[RTNet][EXCEPTION] " + e.Message + (string.IsNullOrEmpty(error) ? "\n\t" : error + "\n\t") + e.StackTrace); }

        protected override void HandlePacket(short packetID, byte[] data)
        {
            switch (packetID)
            {
                case (short)UnityPackets.RPC:
                    RTNetView.HandleRPC(RTNetRPC.FromData(data));
                    break;
                case (short)UnityPackets.Instantiate:
                    RTNetView.HandleInstantiationRequest(InstantiateRequest.FromData(data));
                    break;
                default:
                    LogWarning("Got unhandled packet with ID \"" + packetID + "\"");
                    break;
            }
        }

        protected override void OnConnected()
        {
            Log("OVERRIDE - Connected to server!");
        }

        protected override void OnDisconnected()
        {
            Log("OVERRIDE - Disconnected from server");
        }
    }

}
