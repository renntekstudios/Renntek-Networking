using UnityEngine;
using System.Collections;
using RTNet;
public class PlaySound : MonoBehaviour {
	[SerializeField]
	private AudioClip[] possibleSounds;
	private AudioSource audioSource;
    [SerializeField]
    private bool playOnStart;
    public RTNetView nv;

    // Use this for initialization
    void Start()
    {
        nv = GetComponent<RTNetView>();
        if (nv.isMine)
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();
            if (playOnStart) PlayRandomSound();
        }
    }
	public void PlayRandomSound ()
	{
        
		if(possibleSounds.Length != 0)
		{	
			float f = Random.Range(0,possibleSounds.Length );

           
            int index = (int)Mathf.Floor(f);
            
			audioSource.PlayOneShot(possibleSounds[index]);
            f = Random.Range(0.7f, 1.3f);
           
            audioSource.pitch = f;
		}
		else
		{
			print("No Sounds Referenced");
		}

	}


}
