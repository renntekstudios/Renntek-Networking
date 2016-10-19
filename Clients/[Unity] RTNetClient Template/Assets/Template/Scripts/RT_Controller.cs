using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class RT_Controller : RTNetBehaviour {



    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.position += new Vector3(h, 0, v);
    }
}
