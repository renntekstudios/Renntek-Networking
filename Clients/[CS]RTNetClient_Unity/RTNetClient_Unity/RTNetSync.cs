using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RTNet
{
	[RequireComponent(typeof(RTNetView))]
	public class RTNetSync : MonoBehaviour
	{
		private const float MinSendRate = 1f;
		private const float MaxSendRate = 100f;

		public enum SyncType { None, Interpolation/*, Extrapolation */ }

		public bool sync = true;

		[Header("Network Performance")]
		[Tooltip("How many times per second the rate at which the object is checked for change")]
		public float sendRate = 60;

		[Tooltip("Determines how to sync the object on the other clients")]
		public SyncType syncType = SyncType.Interpolation;

		private Vector3 _position, _scale;
		private Quaternion _rotation;

		void Start()
		{
			if (GetComponent<RTNetView>().isMine)
			{
				_position = transform.position;
				_scale = transform.localScale;
				_rotation = transform.rotation;

				StartCoroutine(Loop());
			}
		}

		void SyncObject(Vector3 pos, Vector3 scale, Quaternion rotation)
		{
			if (GetComponent<RTNetView>().isMine)
				return; // shouldn't be here ever... 
			switch(syncType)
			{
				default:
				case SyncType.None:
					_position = transform.position = pos;
					_scale = transform.localScale = scale;
					_rotation = transform.rotation = rotation;
					break;
				/*
				case SyncType.Interpolation:
					// Lerp
					break;
				*/
			}
		}

		IEnumerator Loop()
		{
			while(true)
			{
				if (sendRate < MinSendRate)
					sendRate = MinSendRate;
				if (sendRate > MaxSendRate)
					sendRate = MaxSendRate;

				if(_position != transform.position || _scale != transform.localScale || _rotation != transform.rotation)
					GetComponent<RTNetView>().RPC("SyncObject", RTReceiver.Others, _position = transform.position, _scale = transform.localScale, _rotation = transform.rotation);
				yield return new WaitForSeconds(1.0f / sendRate);
			}
		}
	}
}
