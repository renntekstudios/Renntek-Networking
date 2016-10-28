using UnityEngine;
using System.Collections;
using RTNet;

public class TestMovement : MonoBehaviour {


    void Update()
    {
        InputMovement();
    }

    public Rigidbody rb;
    public float speed = 10f;

    void InputMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.MovePosition(rb.position + Vector3.forward * speed * Time.deltaTime);
            GetComponent<RTNetView>().RPC("UpdatePosition", RTReceiver.All, transform.position.x, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
            rb.MovePosition(rb.position - Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            rb.MovePosition(rb.position + Vector3.right * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
        {
            rb.MovePosition(rb.position - Vector3.right * speed * Time.deltaTime);
        }
    }

    void UpdatePosition(Vector3 pos)
    {
        Debug.Log("[RPC] Got Update Position");
        Vector3 poss = transform.position;
        pos.x = poss.x;
        pos.y = poss.y;
        pos.z = poss.z;
        poss = transform.position;

    }
}
