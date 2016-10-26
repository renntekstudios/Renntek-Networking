using UnityEngine;
using System.Collections;

using RTNet;

[RequireComponent(typeof(RTNetView))]
public class TestMove : MonoBehaviour
{
    public RTNetView nv;
    
    void Start() { nv = GetComponent<RTNetView>(); }
	void Update()
	{
        if (nv.isMine)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            transform.position += new Vector3(h, 0, v);
        }
        else this.enabled = false;
	}
}
