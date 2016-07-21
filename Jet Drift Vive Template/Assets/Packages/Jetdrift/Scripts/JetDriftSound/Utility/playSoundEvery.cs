using UnityEngine;
using System.Collections;

public class playSoundEvery : MonoBehaviour {

	private AudioSource mySound;
	private bool firstPlay = true;
	public float seconds;
	public float initialDelay;
	
	// Use this for initialization
	void Start () 
	{
		mySound = this.gameObject.GetComponent<AudioSource>();

		StartCoroutine("soundPlay");
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		
	}
	
	IEnumerator soundPlay()
	{
		for (;;) //always
		{
			if (firstPlay)
			{
				firstPlay = false;
				yield return new WaitForSeconds(initialDelay);
			}
			else
			{
//				Debug.Log("initial delay was passed and sound is playing regularly");
				mySound.Play();
				yield return new WaitForSeconds(seconds);
			}
		}
	}


}
