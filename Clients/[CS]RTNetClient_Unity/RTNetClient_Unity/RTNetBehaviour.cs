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
			try
			{
				return (T)objects[index++];
			}
			catch(Exception e)
			{
				Debug.LogError("[RTNet][ERROR] Could not Read as type \"" + typeof(T).Name + "\" because type is actually \"" + objects[index].GetType().Name + "\" \n\t" + e.Message);
				return default(T);
			}
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

		internal static RTStream GetStream(byte[] data)
		{
			RTStream stream = new RTStream((List<object>)new BinaryFormatter().Deserialize(new MemoryStream(data)));
			for(int i = 0; i < stream.objects.Count; i++)
			{
				if (stream.objects[i].GetType().Equals(typeof(Vec2)))
					stream.objects[i] = (Vector2)(Vec2)stream.objects[i];
				else if (stream.objects[i].GetType().Equals(typeof(Vec3)))
					stream.objects[i] = (Vector3)(Vec3)stream.objects[i];
				else if (stream.objects[i].GetType().Equals(typeof(Vec4)))
				{
					switch(((Vec4)stream.objects[i]).Type)
					{
						default:
						case Vec4.Vec4Type.Normal: stream.objects[i] = (Vector4)(Vec4)stream.objects[i]; break;
						case Vec4.Vec4Type.Color: stream.objects[i] = (Color)(Vec4)stream.objects[i]; break;
						case Vec4.Vec4Type.Quaternion: stream.objects[i] = (Quaternion)(Vec4)stream.objects[i]; break;
					}
				}
			}
			return stream;
		}
	}

	[RequireComponent(typeof(RTNetView))]
	public class RTNetBehaviour : MonoBehaviour
	{
		private RTNetView View { get { return GetComponent<RTNetView>(); } }
		private List<short> indexes = new List<short>();
		internal short index = 0;

		public bool isMine { get { return View.isMine; } }
		public bool isPlayer { get { return View.isPlayer; } set { View.isPlayer = value; } }

		[Tooltip("in Hz")]
		public float sendRate = 60; // Hz

		internal void Init()
		{
			RTNetClient.onConnected += OnConnected;
			RTNetClient.onDisconnected += OnDisconnected;
			while (indexes.Contains(index++)) ;
			indexes.Add(index);

			StartCoroutine(CheckStream());
		}

		internal void _internal_sync_stream(byte[] data)
		{
			Debug.Log("INTERNAL SYNC THING");
			RTStream stream = RTStream.GetStream(data);
			stream.SetReading();
			OnSerializeView(ref stream);
		}

		System.Collections.IEnumerator CheckStream()
		{
			while (RTNetView.Client.Connected)
			{
				if (isMine)
				{
					RTStream stream = new RTStream();
					stream.SetWriting();
					OnSerializeView(ref stream);
					if (stream.objects.Count > 0)
						View.SendStream(ref stream, index);
				}
				yield return new WaitForSeconds(1f / sendRate);
			}
		}

		protected virtual void OnConnected() { }
		protected virtual void OnDisconnected() { }

		protected virtual void OnSerializeView(ref RTStream stream) { }
	}
}
