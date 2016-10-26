using UnityEngine;
using System.Collections;
using RTNet;

public class PlayerTank : BaseTank {
    [SerializeField]
    public Camera camera;
    [SerializeField]
    public Transform aimTransform;
    public RTNetView nv;

    void Awake()
    {
        nv = GetComponent<RTNetView>();
    }
	// Use this for initialization
	void Start () {

        if (nv.isMine)
        {
            if (aimTransform == null) print("An Empty transform needs to be referenced for aiming");
            onStart();
        }
        else if (!nv.isMine)
        {
            this.enabled = false;
        }
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            MoveTank(1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            MoveTank(-1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            TurnTank(-rotationSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            TurnTank(rotationSpeed);
        }

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = camera.transform.position.y - 1.5f;
        Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
        aimTransform.position = worldPos;
        TurnTurret(aimTransform);

        if (Input.GetMouseButtonDown(0) && coolDown <= 0)
        {
            Shoot();
        }
        onUpdate();
    }
}
