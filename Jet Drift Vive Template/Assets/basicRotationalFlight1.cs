using UnityEngine;
using System.Collections;

public class basicRotationalFlight : MonoBehaviour {

    public float thrustStrength = 1.0f;

    public bool RotateControlsActive;
    private Transform RotateSphereTransform;
    private Rigidbody CockpitRigidbody;

	// Use this for initialization
	void Start ()
    {
        RotateControlsActive = false;
        RotateSphereTransform = GameObject.FindGameObjectWithTag("RotateSphere").transform;
	    CockpitRigidbody = GameObject.FindGameObjectWithTag("Cockpit").GetComponent<Rigidbody>();
    }

    void FixedUpdate ()
    {
        if (RotateControlsActive)
        {
            CockpitRigidbody.AddForce(thrustStrength * (this.gameObject.transform.position - RotateSphereTransform.position));

        }    
	}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        if (other.gameObject.tag.Equals("RotateSphere"))
        {
            RotateControlsActive = true;
        } 
    }
}
