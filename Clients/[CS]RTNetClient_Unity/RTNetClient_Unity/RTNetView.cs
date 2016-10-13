using System;
using System.Collections.Generic;
using UnityEngine;
using RTNet;
using System.Reflection;
using RTNetClient_Unity.Vectors;

namespace RTNetClient_Unity
{
    public class RTNetView : MonoBehaviour
    {
        public static RTNetClient Client { get; private set; }

        public static short ID { get { return Client != null ? Client.ID : (short)0; } }
        public static bool Connected { get { return Client != null ? Client.Connected : false; } }
        public static string Address { get { return Client != null ? Client.Address : string.Empty; } }
        public static int Port { get { return Client != null ? Client.Port : 0; } }

        [SerializeField]
        public short ViewID = 0;
        private short _viewID;

        private static List<short> viewIDs = new List<short>();
        private static List<RTNetRPC> unhandledRPCs = new List<RTNetRPC>();
        private static List<InstantiateRequest> instantiationRequests = new List<InstantiateRequest>();

        public RTNetView()
        {
            //TODO: Fix this, the viewID doesn't assign but it calls.
            while (viewIDs.Contains(ViewID))
                ViewID++;
            _viewID = ViewID;
            if (Client == null)
                Client = new RTNetClient();
        }

        public void RPC(string method, RTReceiver receiver, params object[] args)
        {
            RPC(method, (short)receiver, args);
        }
        public void RPC(string method, short receiver, params object[] args)
        {
            RTNetRPC rpc = new RTNetRPC();
            rpc.SenderID = Client.ID;
            rpc.ViewID = ViewID;
            rpc.Receiver = receiver;

            rpc.Method = method;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].GetType().Equals(typeof(Vector2)))
                    args[i] = (Vec2)args[i];
                else if (args[i].GetType().Equals(typeof(Vector3)))
                    args[i] = (Vec3)args[i];
                else if (args[i].GetType().Equals(typeof(Vector4)))
                    args[i] = (Vec4)args[i];
                else if (args[i].GetType().Equals(typeof(Quaternion)))
                    args[i] = (Vec4)args[i];
            }

            rpc.Args = args;

            if (Client == null)
                Debug.LogWarning("Tried sending RPC but no RTNetUnityClient exists");
            else
                Client.Send((short)UnityPackets.RPC, rpc.Data);
        }

        void LateUpdate()
        {
            #region Handle RPCs
            MonoBehaviour[] behaviours = gameObject.GetComponents<MonoBehaviour>();
            bool found = false;
            for (int i = 0; i < unhandledRPCs.Count; i++)
            {
                if (unhandledRPCs[i].ViewID == this.ViewID)
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
                if (found)
                {
                    unhandledRPCs.RemoveAt(i);
                    found = false;
                }
            }
            #endregion
            #region Handle Instantiation Requests
            for (int i = 0; i < instantiationRequests.Count; i++)
            {
                if (instantiationRequests[i].BeingHandled)
                    continue;
                instantiationRequests[i].BeingHandled = true;
                GameObject prefab = Resources.Load<GameObject>(instantiationRequests[i].Prefab);
                if (prefab == null)
                {
                    Debug.LogWarning("[RTNet][WARNING] Could not find prefab \"" + instantiationRequests[i].Prefab + "\" to instantiate");
                    instantiationRequests.RemoveAt(i);
                    continue;
                }
                GameObject instantiated = (GameObject)Instantiate(prefab, instantiationRequests[i].Position, instantiationRequests[i].Rotation);
                if (instantiationRequests[i].Scale != new Vector3(1, 1, 1))
                    instantiated.transform.localScale = instantiationRequests[i].Scale;
                instantiationRequests.RemoveAt(i);
            }
            #endregion

            // No one tampers with MYYY ID!!
            if (ViewID != _viewID)
                ViewID = _viewID;
        }

        internal static void HandleRPC(RTNetRPC rpc)
        {
            unhandledRPCs.Add(rpc);
        }

        public void OnApplicationQuit()
        {
            Client.Disconnect();
        }

        /// <summary>
        /// Spawns a prefab with the given name from the Resources folder
        /// </summary>
        /// <param name="prefabName">Name of the prefab to instantiate</param>
        public static void NetworkInstantiate(string prefabName) { NetworkInstantiate(prefabName, Vector3.zero, new Vector3(1, 1, 1), Quaternion.identity); }
        /// <summary>
        /// Spawns a prefab with the given name from the Resources folder
        /// </summary>
        /// <param name="prefabName">Name of the prefab to instantiate</param>
        /// <param name="position">Position to spawn the prefab at</param>
        public static void NetworkInstantiate(string prefabName, Vector3 position) { NetworkInstantiate(prefabName, position, new Vector3(1, 1, 1), Quaternion.identity); }
        /// <summary>
        /// Spawns a prefab with the given name from the Resources folder
        /// </summary>
        /// <param name="prefabName">Name of the prefab to instantiate</param>
        /// <param name="position">Position to spawn the prefab at</param>
        /// <param name="rotation">Rotation of the spawned prefab</param>
        public static void NetworkInstantiate(string prefabName, Vector3 position, Quaternion rotation) { NetworkInstantiate(prefabName, position, new Vector3(1, 1, 1), rotation); }
        /// <summary>
        /// Spawns a prefab with the given name from the Resources folder
        /// </summary>
        /// <param name="prefabName">Name of the prefab to instantiate</param>
        /// <param name="position">Position to spawn the prefab at</param>
        /// <param name="scale">Scale of the spawned prefab</param>
        public static void NetworkInstantiate(string prefabName, Vector3 position, Vector3 scale) { NetworkInstantiate(prefabName, position, scale, Quaternion.identity); }

        /// <summary>
        /// Spawns a prefab with the given name from the Resources folder
        /// </summary>
        /// <param name="prefabName">Name of the prefab to instantiate</param>
        /// <param name="position">Position to spawn the prefab at</param>
        /// <param name="scale">Scale of the spawned prefab</param>
        /// <param name="rotation">Rotation of the spawned prefab</param>
        public static void NetworkInstantiate(string prefabName, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            if (Resources.Load<GameObject>(prefabName) == null)
            {
                Debug.LogWarning("[RTNet][WARNING] Could not instantiate \"" + prefabName + "\" over network - resource not found");
                return;
            }

            byte[] data = new InstantiateRequest(prefabName, position, scale, rotation).Data;
            Client.Send((short)UnityPackets.Instantiate, data);
        }

        internal static void HandleInstantiationRequest(InstantiateRequest request) { instantiationRequests.Add(request); }
    }
}
