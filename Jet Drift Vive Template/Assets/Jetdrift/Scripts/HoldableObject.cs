using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class HoldableObject : MonoBehaviour {

    public bool held;
    public int heldDeviceIndex = -1;

    public Vector3 anchor;

    void Start()
    {

    }

    public virtual void Pickup(int deviceIndex)
    {
        held = true;
        heldDeviceIndex = deviceIndex;
        transform.position += transform.TransformDirection(anchor);
        Debug.Log("Picked up by controller #" + heldDeviceIndex);
    }

    public virtual void Drop()
    {
        held = false;
        heldDeviceIndex = -1;
    }
}
