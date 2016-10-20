using UnityEngine;
using System.Collections;
using RTNet;
public class RT_PlayerConfig : MonoBehaviour {

    public MonoBehaviour[] enable;

    void Start()
    {
        if (GetComponent<RTNetView>().isMine)
        {
            //enable
            foreach(MonoBehaviour Enable in enable)
            {
                Enable.enabled = true;
            }
            Debug.Log("<Color=green> Im LocalPlayer </Color>");
        }
    }
}
