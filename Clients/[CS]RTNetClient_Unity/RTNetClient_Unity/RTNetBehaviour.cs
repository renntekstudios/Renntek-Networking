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
		public bool EndOfStream { get { if (isWriting) return false; return index >= objects.Count; } }

		public RTStream() { objects = new List<object>(); }
		internal RTStream(object[] o) { objects = new List<object>(o); }
		internal RTStream(List<object> o) { objects = new List<object>(o); }

		internal void SetReading() { isReading = true; isWriting = false; }
		internal void SetWriting() { isWriting = true; isReading = false; }
		
		public void Write(object o)
		{
			/*
			if (o.GetType().Equals(typeof(Vector2)))
				o = (Vec2)(Vector2)o;
			else if (o.GetType().Equals(typeof(Vector3)))
				o = (Vec3)(Vector3)o;
			else if (o.GetType().Equals(typeof(Vector4)))
				o = (Vec4)(Vector4)o;
			else if (o.GetType().Equals(typeof(Quaternion)))
				o = (Vec4)(Quaternion)o;
			else if (o.GetType().Equals(typeof(Color)))
				o = (Vec4)(Color)o;
			*/
			objects.Add(o);
		}

		public void Write(object[] o)
		{
			for (int i = 0; i < o.Length; i++)
				Write(o[i]);
		}

		public object Read()
		{
			if (EndOfStream)
				return null;
			return objects[index++];
		}
		
		public object[] ReadRemaining()
		{
			if (EndOfStream)
				return new object[0];
			object[] o = objects.Skip(index).ToArray();
			index = objects.Count;
			return o;
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

		internal byte[] Data
		{
			get
			{
				if (objects.Count == 0)
					return new byte[0];
				return RTSerialization.Serialize(objects.ToArray());
				/*
				BinaryFormatter bf = new BinaryFormatter();
				MemoryStream ms = new MemoryStream();

				bf.Serialize(ms, objects.ToArray());
				return ms.ToArray();
				*/
			}
		}

		public static RTStream GetStream(byte[] data)
		{
			return new RTStream(RTSerialization.Deserialize(data));
			/*
			RTStream s = new RTStream((object[])new BinaryFormatter().Deserialize(new MemoryStream(data)));
			for(int i = 0; i < s.objects.Count; i++)
			{
				if (s.objects[i].GetType().Equals(typeof(Vec2)))
					s.objects[i] = (Vector2)(Vec2)s.objects[i];
				else if (s.objects[i].GetType().Equals(typeof(Vec3)))
					s.objects[i] = (Vector3)(Vec3)s.objects[i];
				else if(s.objects[i].GetType().Equals(typeof(Vec4)))
				{
					switch(((Vec4)s.objects[i]).Type)
					{
						default:
						case Vec4.Vec4Type.Normal: s.objects[i] = (Vector4)(Vec4)s.objects[i]; break;
						case Vec4.Vec4Type.Quaternion:s.objects[i] = (Quaternion)(Vec4)s.objects[i]; break;
						case Vec4.Vec4Type.Color: s.objects[i] = (Color)(Vec4)s.objects[i]; break;
					}
				}
			}
			return s;
			*/
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
