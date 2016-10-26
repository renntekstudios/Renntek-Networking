using UnityEngine;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class Template_SimpleSyncMenu : MonoBehaviour
{
	[Header("Client settings")]
	public string ip = "127.0.0.1";
	public int port = 4434;
	public bool debugMode = false;
	public int bufferSize = 512;

    [Header("Player Prefab")]
    public string PlayerPrefab = "Rectangle Thingy";

    [Header("Network")]
    public RTNetView nv;

    [Header("Player Related")]
    private bool showGUI;

    [Header("Tank Requirements")]
    public GameObject CameraTransform;
    public GameObject AimTransform;

    void Start()
	{
        nv = GetComponent<RTNetView>();
		nv.DebugMode = debugMode;
        nv.BufferSize = bufferSize;
        nv.Connect(ip, port);

        if (nv.Connected)
            showGUI = true;
    }

    void OnGUI()
    {
        if (nv.Connected && showGUI)
        {
            if (GUILayout.Button("Spawn"))
            {

                GameObject myClone = nv.NetworkInstantiate(PlayerPrefab);

                GameObject myCam = Instantiate(CameraTransform);
                myCam.GetComponent<Follow>().target = myClone.transform;

                GameObject AimTrans = Instantiate(AimTransform);

                //get player camera and set to our camera
                myClone.GetComponent<PlayerTank>().camera = myCam.GetComponent<Camera>();
                //get player aim transform and set to our transform
                myClone.GetComponent<PlayerTank>().aimTransform = AimTrans.transform;
                showGUI = false;
            }
        }
    }
}
