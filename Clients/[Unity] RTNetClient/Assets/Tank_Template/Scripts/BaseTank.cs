using UnityEngine;
using System.Collections;

public class BaseTank : MonoBehaviour {

    private Transform turretTransform;
    private Transform nozzleTransform;
    private Transform exhaustTransform;

    [SerializeField]
    private float coolDownTime = 2;
    protected float coolDown;

    private PlaySound soundScript;
    private Light flash;
    private float startIntensity;

    [SerializeField]
    private GameObject bullet;


    [SerializeField]
    protected float accelleration = 1f;
    private bool accellerating = false;
    [SerializeField]
    private float decelleration = 0.5f;
    [SerializeField]
    protected float maxSpeed = 30f;
    protected float speed = 0f;
    private int direction = 1;
    
    [SerializeField]
    protected float rotationSpeed = 90f;
   
    public float GetSpeed() 
    {
        return speed;
    }

    // Use this for initialization
    protected void onStart() 
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
        {
           
            string name = t.gameObject.name;
            switch (name)
            {
                case "TurretHolder":
                    turretTransform = t;                 
                    break;
                case "Nozzle":
                    nozzleTransform = t;                   
                    break;
                case "exhaust":
                    exhaustTransform = t;
                    break;
            }

        } 

        flash = nozzleTransform.gameObject.GetComponent<Light>();   
        if (flash != null) startIntensity = flash.intensity; else print("Add Light Component");
        if (bullet == null) print("No bullet prefab referenced");
        soundScript = GetComponent<PlaySound>();
        if (soundScript == null) print("Add the PlaySound script");
        if (exhaustTransform == null) print("an exhaust gameobject is needed");
    
    }	
	// Update is called once per frame
	protected void onUpdate () {
        if(coolDown > 0)coolDown-= Time.deltaTime;
        if (flash.enabled)
        {
            flash.intensity -= 0.2f;
            if (flash.intensity <= 0)
            {
                flash.enabled = false;
            }
        }
        if (!accellerating)
        {
            if (speed * direction > 0 || speed * direction < 0) speed -= decelleration * direction;
        }
        accellerating = false;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        exhaustTransform.gameObject.GetComponent<ParticleSystem>().emissionRate = 5 + Mathf.Abs(speed);
        exhaustTransform.gameObject.GetComponent<ParticleSystem>().startSize = 5 + Mathf.Abs( speed);
	}
  
    protected void MoveTank(int direction) 
    {
        accellerating = true;
        this.direction = direction;
        if (speed < maxSpeed && speed > -maxSpeed) speed += accelleration * direction;
       
    }
    protected void TurnTank(float degrees) 
    {
        Vector3 rotation = Vector3.up * degrees;
        transform.Rotate(rotation * Time.deltaTime);
    }
    protected void Shoot()
    {
        coolDown = coolDownTime;
        GameObject b = GameObject.Instantiate(bullet) as GameObject;
        b.transform.position = nozzleTransform.position;
        b.transform.rotation = nozzleTransform.rotation;
        b.GetComponent<ParticleSystem>().Play();
    
        soundScript.PlayRandomSound();
              
        flash.intensity = startIntensity;
        flash.enabled = true;
       
    }
    protected void TurnTurret(Transform target) 
    {      
        turretTransform.LookAt(target);      
    }

}
