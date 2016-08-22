using UnityEngine;
using System.Collections;

public class basicTranslationalFlight : MonoBehaviour {

    public float thrustStrength = 1.0f;

    public bool translateControlsActive;
    private Transform TranslateSphereTransform;
    private Rigidbody CockpitRigidbody;

	// Use this for initialization
	void Start ()
    {
        translateControlsActive = false;
        TranslateSphereTransform = GameObject.FindGameObjectWithTag("TranslateSphere").transform;
	    CockpitRigidbody = GameObject.FindGameObjectWithTag("Cockpit").GetComponent<Rigidbody>();
    }

    void FixedUpdate ()
    {
        if (translateControlsActive)
        {
            CockpitRigidbody.AddForce(thrustStrength * (TranslateSphereTransform.position - this.gameObject.transform.position));
        }    
	}

    void onTriggerEnter(Collider other)
    {
        if (other.tag.Equals("TranslateSphere"))
        {
            translateControlsActive = true;
        } 
    }
}
