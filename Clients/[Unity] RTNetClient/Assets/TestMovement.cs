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
}
