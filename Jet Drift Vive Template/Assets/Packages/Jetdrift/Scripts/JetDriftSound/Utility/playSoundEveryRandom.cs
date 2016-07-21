using UnityEngine;
using System.Collections;

public class playSoundEveryRandom : MonoBehaviour {

	public float initialDelay;

	public float min;
	public float max;

	private float seconds;

	private AudioSource mySound;
	private bool firstPlay = true;

	// Use this for initialization
	void Start () 
	{
		mySound = this.gameObject.GetComponent<AudioSource>();
		
		StartCoroutine("soundPlay");
		Random.seed = gameObject.GetInstanceID();
		seconds = Random.Range(min,max);
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
