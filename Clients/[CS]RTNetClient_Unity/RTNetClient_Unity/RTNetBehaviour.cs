using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace RTNet
{
	public class RTStream
	{
		internal List<object> objects = new List<object>();
		private int index = 0;

		public bool isReading { get; internal set; }
		public bool isWriting { get; internal set; }

		internal RTStream() { objects = new List<object>(); }
		internal RTStream(object[] o) { objects = new List<object>(o); }
		internal RTStream(List<object> o) { objects = new List<object>(o); }

		internal void SetReading() { isReading = true; isWriting = false; }
		internal void SetWriting() { isWriting = true; isReading = false; }
		
		public void Write(object o)
		{
			if (o.GetType().Equals(typeof(Vector2)))
				objects.Add((Vec2)(Vector2)o);
			else if (o.GetType().Equals(typeof(Vector3)))
				objects.Add((Vec3)(Vector3)o);
			else if (o.GetType().Equals(typeof(Vector4)))
				objects.Add((Vec4)(Vector4)o);
			else if (o.GetType().Equals(typeof(Quaternion)))
				objects.Add((Vec4)(Quaternion)o);
			else if (o.GetType().Equals(typeof(Color)))
				objects.Add((Vec4)(Color)o);
			else
				objects.Add(o);
		}

		public object Read()
		{
			if (index >= objects.Count)
				return null;
			return objects[index++];
		}

		public T Read<T>()
		{
			if (index >= objects.Count)
				return default(T);
			return (T)objects[index];
		}

		public object ReadAt(int i)
		{
			if (i < 0 || i >= objects.Count)
				return null;
			return objects[i];
		}

		public void Skip() { index++; }
		public void Rewind() { index = 0; }

		internal byte[] GetData()
		{
			if (objects.Count == 0)
				return new byte[0];
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();

			bf.Serialize(ms, objects);
			byte[] buffer = ms.ToArray();
			ms.Dispose();

			return buffer;
		}

		internal static RTStream GetStream(byte[] data) { return new RTStream((List<object>)new BinaryFormatter().Deserialize(new MemoryStream(data))); }
	}

	[RequireComponent(typeof(RTNetView))]
	public class RTNetBehaviour : MonoBehaviour
	{
		private RTNetView View { get { return GetComponent<RTNetView>(); } }

		public bool isMine { get { return View.isMine; } }
		public bool isPlayer { get { return View.isPlayer; } set { View.isPlayer = value; } }

		internal void Init()
		{
			RTNetClient.onConnected += OnConnected;
			RTNetClient.onDisconnected += OnDisconnected;
		}

		void _internal_sync_stream(byte[] data)
		{
			Debug.Log("INTERNAL SYNC THING");
			RTStream stream = RTStream.GetStream(data);
			stream.SetReading();
			OnSerializeView(ref stream);
		}

		void Update()
		{
			if(isMine)
			{
				RTStream stream = new RTStream();
				stream.SetWriting();
				OnSerializeView(ref stream);
				// byte[] data = stream.GetData();
				if(stream.objects.Count > 0)
					View.RPC("_internal_sync_stream", RTReceiver.Others, stream.objects);
			}
		}

		protected virtual void OnConnected() { }
		protected virtual void OnDisconnected() { }

		protected virtual void OnSerializeView(ref RTStream stream) { }
	}
}
