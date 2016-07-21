using UnityEngine;
using System.Collections;

public class musicTester : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        //Start on Theme 0 by default and play track 0
        musicHandler.play(0);
        //Start Crossfade System **This is just a quirk
        musicHandler.playCrossfade(0);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}



}
