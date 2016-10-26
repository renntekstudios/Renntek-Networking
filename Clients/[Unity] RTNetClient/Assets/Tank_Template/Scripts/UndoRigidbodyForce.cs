using UnityEngine;
using System.Collections;
using RTNet;
public class UndoRigidbodyForce : MonoBehaviour {
	Rigidbody rb;
    public RTNetView nv;

    // Use this for initialization
    void Start () {
		rb = this.GetComponent<Rigidbody>();
        nv = GetComponent<RTNetView>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(nv.isMine)
		    if(rb.velocity != Vector3.zero)rb.velocity=Vector3.zero;
	}
}
