using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace RTNet
{
	public static class RTSerialization
	{
		private enum SerializeType : byte { Unknown, Vec2, Vector2, Vec3, Vector3, Vec4, Vector4, Quaternion, Color, Float, String, Int, Bool, Short, Long }

		public static byte[] Serialize(object o)
		{
			if(o == null)
				return new byte[0];
			byte[] temp = new byte[0];
			SerializeType type = SerializeType.Unknown;
			if (o.GetType().Equals(typeof(Vec2)))
			{
				temp = ((Vec2)o).Data;
				type = SerializeType.Vec2;
			}
			else if (o.GetType().Equals(typeof(Vector2)))
			{
				temp = ((Vec2)(Vector2)o).Data;
				type = SerializeType.Vector2;
			}
			else if (o.GetType().Equals(typeof(Vec3)))
			{
				temp = ((Vec3)o).Data;
				type = SerializeType.Vec3;
			}
			else if (o.GetType().Equals(typeof(Vector3)))
			{
				temp = ((Vec3)(Vector3)o).Data;
				type = SerializeType.Vector3;
			}
			else if (o.GetType().Equals(typeof(Vec4)))
			{
				temp = ((Vec4)o).Data;
				type = SerializeType.Vec4;
			}
			else if (o.GetType().Equals(typeof(Vector4)))
			{
				temp = ((Vec4)(Vector4)o).Data;
				type = SerializeType.Vector4;
			}
			else if (o.GetType().Equals(typeof(Quaternion)))
			{
				temp = ((Vec4)(Quaternion)o).Data;
				type = SerializeType.Quaternion;
			}
			else if (o.GetType().Equals(typeof(Color)))
			{
				temp = ((Vec4)(Color)o).Data;
				type = SerializeType.Color;
			}
			else if(o.GetType().Equals(typeof(string)))
			{
				temp = Encoding.UTF8.GetBytes((string)o);
				type = SerializeType.String;
			}
			else if(o.GetType().Equals(typeof(float)))
			{
				temp = BitConverter.GetBytes((int)(Math.Round((float)o, 3) * 1000));
				type = SerializeType.Float;
			}
			else if(o.GetType().Equals(typeof(int)))
			{
				temp = BitConverter.GetBytes((int)o);
				type = SerializeType.Int;
			}
			else if(o.GetType().Equals(typeof(bool)))
			{
				temp = new byte[1];
				temp[0] = (byte)(((bool)o) ? 1 : 0);
				type = SerializeType.Bool;
			}
			else if(o.GetType().Equals(typeof(short)))
			{
				temp = BitConverter.GetBytes((short)o);
				type = SerializeType.Short;
			}
			else if(o.GetType().Equals(typeof(long)))
			{
				temp = BitConverter.GetBytes((long)o);
				type = SerializeType.Long;
			}
			else
			{
				BinaryFormatter bf = new BinaryFormatter();
				MemoryStream ms = new MemoryStream();
				bf.Serialize(ms, o);
				temp = ms.ToArray();
				ms.Dispose();
			}
			List<byte> d = new List<byte>();
			d.AddRange(BitConverter.GetBytes((short)temp.Length));
			d.Add((byte)type);
			d.AddRange(temp);
			return d.ToArray();
		}

		public static byte[] Serialize(object[] o)
		{
			if (o.Length == 0)
				return new byte[0];
			List<byte> d = new List<byte>();
			for (int i = 0; i < o.Length; i++)
				d.AddRange(Serialize(o[i]));

			/*
			string s = "";
			for (int i = 0; i < d.Count; i++)
				s += d[i];
			*/

			return d.ToArray();
		}

		public static object[] Deserialize(byte[] data) { return Deserialize(data, 0, data.Length); }
		public static object[] Deserialize(byte[] data, int offset) { return Deserialize(data, offset, data.Length - offset); }
		public static object[] Deserialize(byte[] data, int offset, int count)
		{
			try
			{
				if (offset > 0)
					data = data.Skip(offset).ToArray();
				if(count < data.Length)
					data = data.Take(count).ToArray();
				if (data.Length == 0)
					return new object[0];

				/*
				string s = "";
				for (int i = 0; i < data.Length; i++)
					s += data[i];
				*/

				List<object> o = new List<object>();
				short length = 0;
				SerializeType type = SerializeType.Unknown;
				BinaryFormatter bf = new BinaryFormatter();
				while (data.Length > 0)
				{
					length = BitConverter.ToInt16(data, 0);
					if (length == 0)
						break;
					type = (SerializeType)data[2];
					Debug.Log("Length: " + length + "; Type: " + Enum.GetName(typeof(SerializeType), type));
					data = data.Skip(sizeof(short) + 1).ToArray();
					switch(type)
					{
						case SerializeType.Vec2:			o.Add(Vec2.FromData(data.Take(length).ToArray())); break;
						case SerializeType.Vector2:			o.Add((Vector2)(Vec2.FromData(data.Take(length).ToArray()))); break;
						case SerializeType.Vec3:			o.Add(Vec3.FromData(data.Take(length).ToArray())); break;
						case SerializeType.Vector3:			o.Add((Vector3)(Vec3.FromData(data.Take(length).ToArray()))); break;
						case SerializeType.Vec4:			o.Add(Vec4.FromData(data.Take(length).ToArray())); break;
						case SerializeType.Vector4:			o.Add((Vector4)(Vec4.FromData(data.Take(length).ToArray()))); break;
						case SerializeType.Quaternion:		o.Add((Quaternion)(Vec4.FromData(data.Take(length).ToArray()))); break;
						case SerializeType.Color:			o.Add((Color)(Vec4.FromData(data.Take(length).ToArray()))); break;
						case SerializeType.Int:				o.Add(BitConverter.ToInt32(data.Take(length).ToArray(), 0)); break;
						case SerializeType.Short:			o.Add(BitConverter.ToInt16(data.Take(length).ToArray(), 0)); break;
						case SerializeType.Long:			o.Add(BitConverter.ToInt64(data.Take(length).ToArray(), 0)); break;
						case SerializeType.Float:			o.Add(BitConverter.ToInt32(data.Take(length).ToArray(), 0) / 1000.0f); break;
						case SerializeType.Bool:			o.Add(data[0] == 1 ? true : false); break;
						case SerializeType.String:			o.Add(Encoding.UTF8.GetString(data.Take(length).ToArray())); break;
						default: case SerializeType.Unknown: o.Add(bf.Deserialize(new MemoryStream(data.Take(length).ToArray()))); break;
					}
					data = data.Skip(length).ToArray();
				}
				return o.ToArray();
			}
			catch(Exception e)
			{
				Debug.LogError("[RTNet][EXCEPTION] Could not deserialize " + data.Length + " bytes - " + e.Message + "\n\t" + e.StackTrace);
				return null;
			}
		}

		public static object DeserializeSingle(byte[] data) { return DeserializeSingle(data, 0, data.Length); }
		public static object DeserializeSingle(byte[] data, int offset) { return DeserializeSingle(data, offset, data.Length - offset); }
		public static object DeserializeSingle(byte[] data, int offset, int count)
		{
			if (data.Length == 0)
				return null;
			try
			{
				if (offset > 0)
					data = data.Skip(offset).ToArray();
				if (count < data.Length)
					data = data.Take(count).ToArray();

				short length = BitConverter.ToInt16(data, 0);
				SerializeType type = (SerializeType)data[2];
				switch (type)
				{
					case SerializeType.Vec2: return (Vec2.FromData(data.Take(length).ToArray()));
					case SerializeType.Vector2: return ((Vector2)(Vec2.FromData(data.Take(length).ToArray())));
					case SerializeType.Vec3: return (Vec3.FromData(data.Take(length).ToArray()));
					case SerializeType.Vector3: return ((Vector3)(Vec3.FromData(data.Take(length).ToArray())));
					case SerializeType.Vec4: return (Vec4.FromData(data.Take(length).ToArray()));
					case SerializeType.Vector4: return ((Vector4)(Vec4.FromData(data.Take(length).ToArray())));
					case SerializeType.Quaternion: return ((Quaternion)(Vec4.FromData(data.Take(length).ToArray())));
					case SerializeType.Color: return ((Color)(Vec4.FromData(data.Take(length).ToArray())));
					default: case SerializeType.Unknown:  return (new BinaryFormatter().Deserialize(new MemoryStream(data.Take(length).ToArray())));
				}
			}
			catch (Exception e)
			{
				Debug.LogError("[RTNet][EXCEPTION] Could not deserialize " + data.Length + " bytes - " + e.Message + "\n\t" + e.StackTrace);
				return null;
			}
		}

		public static T Deserialize<T>(byte[] data) { return (T)DeserializeSingle(data, 0, data.Length); }
		public static T Deserialize<T>(byte[] data, int offset) { return (T)DeserializeSingle(data, offset, data.Length - offset); }
		public static T Deserialize<T>(byte[] data, int offset, int count) { return (T)DeserializeSingle(data, offset, count); }
	}
}
