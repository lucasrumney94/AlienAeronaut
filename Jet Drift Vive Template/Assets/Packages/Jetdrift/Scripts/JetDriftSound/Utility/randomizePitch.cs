using UnityEngine;
using System.Collections;

public class randomizePitch : MonoBehaviour {

	[Range(0,2)] public float variance = 1;

	private AudioSource myAudioSource;


	// Use this for initialization
	void Start ()
	{
		Random.seed = gameObject.GetInstanceID();
		myAudioSource = this.gameObject.GetComponent<AudioSource>();
		myAudioSource.pitch = 1.0f + (Random.Range(-variance, variance));
	}
	
	// Update is called once per frame
	void Update () 
	{

	}
}
