using UnityEngine;
using System.Collections;

public class FlightController : MonoBehaviour {

    private ControllerInputTracker vrInput;


   
	void Start ()
    {
        vrInput = GameObject.FindGameObjectWithTag("Player").GetComponent<ControllerInputTracker>();

	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
