using UnityEngine;
using System.Collections;
using RTNet;
public class OnFired : MonoBehaviour {
	[SerializeField]
	private int life = 10;
    // Use this for initialization
    public RTNetView nv;

    void Start()
    {
        nv = GetComponent<RTNetView>();
    }

	void OnCollisionEnter(Collision collision)
	{
		GameObject b = collision.gameObject;
		if(b.tag == "Bullet")
		{
			//Destroy(b);
			life--;
			if(life==0)
			{
				Destroy(this.gameObject);
			}
		}
	
	}
}
