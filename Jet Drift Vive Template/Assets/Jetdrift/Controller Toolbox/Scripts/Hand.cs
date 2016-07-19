using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Hand : VRTool {

    public int deviceIndex;

    public float forceMultiplier = 1.5f;

    public LayerMask collisionMask;

    public Rigidbody handRigidbody;
    public GameObject highlightedObject;
    public GameObject heldObject;

    private FixedJoint joint;

    void Start()
    {
        deviceIndex = GetComponentInParent<ControllerInputTracker>().index;
        vrInput = transform.GetComponentInParent<ControllerInputTracker>();
        handRigidbody = GetComponent<Rigidbody>();

        vrInput.triggerPressedDown += new ControllerInputDelegate(TryPickup);
        vrInput.triggerPressedUp += new ControllerInputDelegate(DeactivateHeld);
        vrInput.triggerPressedUp += new ControllerInputDelegate(TryPickup);
        vrInput.gripPressedDown += new ControllerInputDelegate(TryDrop);
    }

    void OnEnable()
    {
        InitializeOptions();
    }

    public override void InitializeOptions()
    {
        toolOptions = new Option[1];
        toolOptions[0] = new Option(new ReferenceValue<float>(() => forceMultiplier, v => { forceMultiplier = v; }), "Throw strength", 1f, 10f);
    }

    public void TryPickup()
    {
        if (heldObject != null)
        {
            heldObject.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }
        else if (highlightedObject != null)
        {
            Pickup(highlightedObject);
        }
    }

    public void DeactivateHeld()
    {
        if (heldObject != null)
        {
            heldObject.SendMessage("DeActivate", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TryDrop()
    {
        if (heldObject != null)
        {
            Drop(heldObject);
        }
    }
    
    private void Pickup(GameObject picked)
    {
        picked.SendMessage("Pickup", deviceIndex, SendMessageOptions.DontRequireReceiver);
        joint = picked.AddComponent<FixedJoint>();
        joint.connectedBody = handRigidbody;
        heldObject = picked;
    }

    private void Drop(GameObject held)
    {
        Rigidbody heldRigidbody = held.GetComponent<Rigidbody>();
        heldRigidbody.velocity = vrInput.velocity * forceMultiplier;
        heldRigidbody.angularVelocity = vrInput.angularVelocity;
        heldRigidbody.maxAngularVelocity = heldRigidbody.angularVelocity.magnitude;
        Destroy(joint);
        joint = null;
        heldObject = null;
    }

    void OnTriggerEnter(Collider other)
    {
        highlightedObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (highlightedObject == other.gameObject)
        {
            highlightedObject = null;
        }
    }
}