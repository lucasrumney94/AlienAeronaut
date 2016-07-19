/*
Add this component to [CameraRig]
Tracks the transform of the HMD, and provides a function to change the position of [CameraRig] based on a desired HMD position
*/
using UnityEngine;
using System.Collections;

public class PlayerPositionTracker : MonoBehaviour {

    public SteamVR_TrackedObject headCamera;

    public Vector3 CameraRigPosition
    {
        get
        {
            return rigTransform.position;
        }
        set
        {
            rigTransform.position = value;
        }
    }
    public Vector3 HeadPosition
    {
        get
        {
            return cameraTransform.position;
        }
    }
    public Vector3 HeadRotationEulerAngles
    {
        get
        {
            return cameraTransform.eulerAngles;
        }
    }
    public Quaternion HeadRotation
    {
        get
        {
            return cameraTransform.rotation;
        }
    }

    /// <summary>
    /// Provide a desired player position (y should be the floor height), and [CameraRig] will be moved to place the player camera above that point
    /// </summary>
    /// <param name="worldPosition"></param>
    public void SetHeadPosition(Vector3 worldPosition)
    {
        Vector3 newPosition;
        newPosition.x = worldPosition.x - (HeadPosition.x - CameraRigPosition.x);
        newPosition.y = worldPosition.y;
        newPosition.z = worldPosition.z - (HeadPosition.z - CameraRigPosition.z);

        CameraRigPosition = newPosition;
    }

    private Transform rigTransform;
    private Transform cameraTransform;

    void OnEnable()
    {
        rigTransform = transform;
        cameraTransform = headCamera.transform;
    }
}
