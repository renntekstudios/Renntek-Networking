using UnityEngine;
using System.Collections;

public class MoveBullet : MonoBehaviour {

	private ParticleSystem ps;
	[SerializeField]
	private float mSpeed = 10;
    private bool exploding = false;
    [SerializeField]
    private GameObject explosion;

    
    // Use this for initialization
	void Start () {
		ps = this.gameObject.GetComponent<ParticleSystem>();
		ps.Play();
			

	}	
	// Update is called once per frame
	void Update () {


		if(!exploding)transform.Translate(Vector3.forward * Time.deltaTime * mSpeed);

		if(transform.position.magnitude > 1500)
		{
			Destroy(this.gameObject);
		
		}

	}
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy") 
        {
            Explode();
        }
    }
    void Explode() 
    {
      
        exploding = true;
        GameObject obj = (GameObject)GameObject.Instantiate(explosion, transform.position, transform.rotation);        
        Destroy(this.gameObject);
    
    }
}
