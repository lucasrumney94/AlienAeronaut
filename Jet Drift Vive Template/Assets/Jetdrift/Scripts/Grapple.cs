using UnityEngine;
using System.Collections;

public class Grapple : HoldableObject {

    public float range = 20f;
    public float grappleSpeed = 1f;
    public Vector3 sightedPosition;
    public bool hitObject;
    public bool latched;

    public LineRenderer laser;
    public GameObject cameraRig; //set this up to assign automatically

    void Update()
    {
        LaserRayCast();
        laser.SetPosition(0, transform.position);
        laser.SetPosition(1, sightedPosition);
    }

    void FixedUpdate()
    {
        if (latched)
        {
            MoveCamRigTowardsPosition(sightedPosition, grappleSpeed / 60f);
        }
    }

    private void LaserRayCast()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, range))
        {
            sightedPosition = hit.point;
            hitObject = true;
        }
        else
        {
            sightedPosition = transform.position + (transform.forward * range);
            hitObject = false;
        }
    }

    private void Activate()
    {
        //draw a line from source to first collision
        if (hitObject)
        {
            Debug.Log(name + " hit surface at " + sightedPosition);
            latched = true;
        }
    }

    private void DeActivate()
    {
        latched = false;
    }

    private void MoveCamRigTowardsPosition(Vector3 position, float speed)
    {
        Vector3 towards = position - transform.position;
        towards = towards.normalized * speed;
        cameraRig.transform.position += towards;
    }
}
