/*
Add this component to both controllers in [CameraRig]
Triggers events when controller buttons are pressed;
contains public variables for button specific information;
contains public variables for controller transform + velocity;
*/
using UnityEngine;
using System.Collections;

public delegate void ControllerInputDelegate();

public enum Direction
{
    None,
    Up,
    Down,
    Right,
    Left
}

public class ControllerInputTracker : MonoBehaviour {

    #region Public Variables

    private Transform origin; //Transform for translating controller position & physics information into worldspace
    private ushort maxPulseLength = 3999; //Hardware limit for vibration pulse length, in microseconds

    public Vector3 position; //Worldspace potision of controller
    public Quaternion rotation; //Worldspace rotation of controller

    public Vector3 velocity; //Velocity of controller
    public Vector3 angularVelocity; //Angular Velocity of controller

    public Vector2 touchpadAxis; //Current touched position of touchpad; (0, 0) is center of touchpad
    private Vector2 lastTouchpadAxis;
    public Vector2 touchpadAxisDelta; //Change in touchpadAxis between this frame and the last

    public float touchpadAngle; //Between 0 and 360 going counter-clockwise; 0 is in the positive 'x' direction
    private float lastTouchpadAngle;
    public float touchpadAngleDelta; //Change in touchpadAngle between this frame and the last

    public float dpadDeadzone = 0.25f; //Inside a circle of this radius at the center of the touchpad no direction is registered
    public Direction dpadDirection = Direction.None; //Enum indicating which quadrant of the touchpad is currently touched, if any, for using the touchpad as a directional pad
    private Direction lastDpadDirection;

    public float triggerStrength; //Analog value of the trigger's pressed state, between 0 and 1
    private float lastTriggerStrength;
    public float triggerStrengthDelta; //Change in triggerStrength between this frame and the last

    public bool triggerTouched; //Light pull, true when triggerStrength is > 0.25
    public bool triggerPressed; //Heavy pull, true when triggerStrength is > 0.55, triggers before 'click' on trigger is heard
    public bool gripPressed; //Either of the side buttons on the controller
    public bool menuPressed; //Button above trackpad
    public bool touchpadTouched; //True when finger is resting on trackpad
    public bool touchpadPressed; //True after touchpad has been clicked

    #endregion

    #region Event Definitions

    /*
    Assign to these using += or -= when you want to add or remove a listener for an input event
    Example:
    ControllerInputTracker inputTracker = GetComponent<ControllerInputTracker>();

    private void Fire() { ... }  <== NB: void function with no parameters

    //Start calling Fire() every time trigger is pressed down:
    inputTracker.triggerPressedDown += new ControllerInputDelegate(Fire);
    //or to stop listening for the event:
    inputTracker.triggerPressedDown -= new ControllerInputDelegate(Fire);
    */

    public event ControllerInputDelegate triggerTouchedDown;
    public event ControllerInputDelegate triggerTouchedUp;

    public event ControllerInputDelegate triggerPressedDown;
    public event ControllerInputDelegate triggerPressedUp;

    public event ControllerInputDelegate gripPressedDown;
    public event ControllerInputDelegate gripPressedUp;

    public event ControllerInputDelegate menuPressedDown;
    public event ControllerInputDelegate menuPressedUp;

    public event ControllerInputDelegate touchpadTouchedDown;
    public event ControllerInputDelegate touchpadTouchedUp;

    public event ControllerInputDelegate touchpadPressedDown;
    public event ControllerInputDelegate touchpadPressedUp;

    public event ControllerInputDelegate dpadUpTouchedStart;
    public event ControllerInputDelegate dpadDownTouchedStart;
    public event ControllerInputDelegate dpadRightTouchedStart;
    public event ControllerInputDelegate dpadLeftTouchedStart;

    public event ControllerInputDelegate dpadUpTouchedEnd;
    public event ControllerInputDelegate dpadDownTouchedEnd;
    public event ControllerInputDelegate dpadRightTouchedEnd;
    public event ControllerInputDelegate dpadLeftTouchedEnd;

    public event ControllerInputDelegate dpadUpPressedStart;
    public event ControllerInputDelegate dpadDownPressedStart;
    public event ControllerInputDelegate dpadRightPressedStart;
    public event ControllerInputDelegate dpadLeftPressedStart;

    public event ControllerInputDelegate dpadUpPressedEnd;
    public event ControllerInputDelegate dpadDownPressedEnd;
    public event ControllerInputDelegate dpadRightPressedEnd;
    public event ControllerInputDelegate dpadLeftPressedEnd;

    #endregion

    #region Event Trigger Functions

    //Events cannot be passed as arguements, so there is no simple way to reference them cleanly

    private void OnTriggerTouchedDown()
    {
        if (triggerTouchedDown != null)
        {
            triggerTouchedDown();
        }
    }

    private void OnTriggerTouchedUp()
    {
        if (triggerTouchedUp != null)
        {
            triggerTouchedUp();
        }
    }

    private void OnTriggerPressedDown()
    {
        if (triggerPressedDown != null)
        {
            triggerPressedDown();
        }
    }

    private void OnTriggerPressedUp()
    {
        if (triggerPressedUp != null)
        {
            triggerPressedUp();
        }
    }

    private void OnGripPressedDown()
    {
        if (gripPressedDown != null)
        {
            gripPressedDown();
        }
    }

    private void OnGripPressedUp()
    {
        if (gripPressedUp != null)
        {
            gripPressedUp();
        }
    }

    private void OnMenuPressedDown()
    {
        if (menuPressedDown != null)
        {
            menuPressedDown();
        }
    }

    private void OnMenuPressedUp()
    {
        if (menuPressedUp != null)
        {
            menuPressedUp();
        }
    }

    private void OnTouchpadTouchedDown()
    {
        if (touchpadTouchedDown != null)
        {
            touchpadTouchedDown();
        }
    }

    private void OnTouchpadTouchedUp()
    {
        if (touchpadTouchedUp != null)
        {
            touchpadTouchedUp();
        }
    }

    private void OnTouchpadPressedDown()
    {
        if (touchpadPressedDown != null)
        {
            touchpadPressedDown();
        }
    }

    private void OnTouchpadPressedUp()
    {
        if (touchpadPressedUp != null)
        {
            touchpadPressedUp();
        }
    }

    private void OnDpadUpTouchedStart()
    {
        if (dpadUpTouchedStart != null)
        {
            dpadUpTouchedStart();
        }
    }

    private void OnDpadDownTouchedStart()
    {
        if (dpadDownTouchedStart != null)
        {
            dpadDownTouchedStart();
        }
    }

    private void OnDpadRightTouchedStart()
    {
        if (dpadRightTouchedStart != null)
        {
            dpadRightTouchedStart();
        }
    }

    private void OnDpadLeftTouchedStart()
    {
        if (dpadLeftTouchedStart != null)
        {
            dpadLeftTouchedStart();
        }
    }

    private void OnDpadUpTouchedEnd()
    {
        if (dpadUpTouchedEnd != null)
        {
            dpadUpTouchedEnd();
        }
    }

    private void OnDpadDownTouchedEnd()
    {
        if (dpadDownTouchedEnd != null)
        {
            dpadDownTouchedEnd();
        }
    }

    private void OnDpadRightTouchedEnd()
    {
        if (dpadRightTouchedEnd != null)
        {
            dpadRightTouchedEnd();
        }
    }

    private void OnDpadLeftTouchedEnd()
    {
        if (dpadLeftTouchedEnd != null)
        {
            dpadLeftTouchedEnd();
        }
    }

    private void OnDpadUpPressedStart()
    {
        if (dpadUpPressedStart != null)
        {
            dpadUpPressedStart();
        }
    }

    private void OnDpadDownPressedStart()
    {
        if (dpadDownPressedStart != null)
        {
            dpadDownPressedStart();
        }
    }

    private void OnDpadRightPressedStart()
    {
        if (dpadRightPressedStart != null)
        {
            dpadRightPressedStart();
        }
    }

    private void OnDpadLeftPressedStart()
    {
        if (dpadLeftPressedStart != null)
        {
            dpadLeftPressedStart();
        }
    }

    private void OnDpadUpPressedEnd()
    {
        if (dpadUpPressedEnd != null)
        {
            dpadUpPressedEnd();
        }
    }

    private void OnDpadDownPressedEnd()
    {
        if (dpadDownPressedEnd != null)
        {
            dpadDownPressedEnd();
        }
    }

    private void OnDpadRightPressedEnd()
    {
        if (dpadRightPressedEnd != null)
        {
            dpadRightPressedEnd();
        }
    }

    private void OnDpadLeftPressedEnd()
    {
        if (dpadLeftPressedEnd != null)
        {
            dpadLeftPressedEnd();
        }
    }

    #endregion

    private ulong triggerMask = SteamVR_Controller.ButtonMask.Trigger;
    private ulong gripMask = SteamVR_Controller.ButtonMask.Grip;
    private ulong menuMask = SteamVR_Controller.ButtonMask.ApplicationMenu;
    private ulong touchpadMask = SteamVR_Controller.ButtonMask.Touchpad;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
    private SteamVR_TrackedObject trackedObject;
    public int index;

    void OnEnable()
    {
        triggerMask = SteamVR_Controller.ButtonMask.Trigger;
        gripMask = SteamVR_Controller.ButtonMask.Grip;
        menuMask = SteamVR_Controller.ButtonMask.ApplicationMenu;
        touchpadMask = SteamVR_Controller.ButtonMask.Touchpad;

        trackedObject = GetComponent<SteamVR_TrackedObject>();
        index = (int)trackedObject.index;
        origin = trackedObject.origin ? trackedObject.origin : transform.parent;
    }

    void Update()
    {
        //Set public variables
        position = origin.TransformPoint(transform.localPosition);
        rotation = transform.rotation;
        velocity = origin.TransformVector(controller.velocity);
        angularVelocity = origin.TransformVector(controller.angularVelocity);

        triggerStrength = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
        triggerStrengthDelta = triggerStrength - lastTriggerStrength;

        touchpadAxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        touchpadAxisDelta = touchpadAxis - lastTouchpadAxis;

        touchpadAngle = GetTouchpadAngle(touchpadAxis);
        touchpadAngleDelta = touchpadAngle - lastTouchpadAngle;

        CheckInputs();

        //Check for touchpad direction
        if (touchpadPressed == true)
        {
            dpadDirection = GetDPadDirection(touchpadAngle);
        }
        else if (touchpadTouched == true)
        {
            dpadDirection = GetDPadDirection(touchpadAngle);
        }
        else
        {
            dpadDirection = Direction.None;
        }

        //Check for dpad direction change, enables triggering multiple direction inputs without lifting finger
        if (dpadDirection != lastDpadDirection)
        {
            if (touchpadPressed == true)
            {
                DpadPressEnd(lastDpadDirection);
                DpadPressStart(lastDpadDirection);
            }
            else if (touchpadTouched == true)
            {
                DpadTouchEnd(lastDpadDirection);
                DpadTouchStart(dpadDirection);
            }
        }

        lastTriggerStrength = triggerStrength;
        lastTouchpadAxis = touchpadAxis;
        lastTouchpadAngle = touchpadAngle;
        lastDpadDirection = dpadDirection;
    }

    /// <summary>
    /// Checks SteamVR_Controller.Device.Get[Touch/Press][Up/Down], calls the appropiate event triggers, and sets button state bools
    /// </summary>
    private void CheckInputs()
    {
        if (controller.GetTouchDown(triggerMask))
        {
            triggerTouched = true;
            OnTriggerTouchedDown();
        }
        if (controller.GetTouchUp(triggerMask))
        {
            triggerTouched = false;
            OnTriggerTouchedUp();
        }

        if (controller.GetPressDown(triggerMask))
        {
            triggerPressed = true;
            OnTriggerPressedDown();
        }
        if (controller.GetPressUp(triggerMask))
        {
            triggerPressed = false;
            OnTriggerPressedUp();
        }

        if (controller.GetPressDown(gripMask))
        {
            gripPressed = true;
            OnGripPressedDown();
        }
        if (controller.GetPressUp(gripMask))
        {
            gripPressed = false;
            OnGripPressedUp();
        }

        if (controller.GetPressDown(menuMask))
        {
            menuPressed = true;
            OnMenuPressedDown();
        }
        if (controller.GetPressUp(menuMask))
        {
            menuPressed = false;
            OnMenuPressedUp();
        }

        if (controller.GetTouchDown(touchpadMask))
        {
            touchpadTouched = true;
            OnTouchpadTouchedDown();
        }
        if (controller.GetTouchUp(touchpadMask))
        {
            touchpadTouched = false;
            OnTouchpadTouchedUp();
        }

        if (controller.GetPressDown(touchpadMask))
        {
            touchpadPressed = true;
            OnTouchpadPressedDown();
        }
        if (controller.GetPressUp(touchpadMask))
        {
            touchpadPressed = false;
            OnTouchpadPressedUp();
        }
    }

    /// <summary>
    /// For sending vibration to the controller, still needs testing
    /// </summary>
    /// <param name="strength"></param>
    public void VibrateController(ushort strength, float duration = 0f)
    {
        ushort microSeconds = strength > maxPulseLength ? maxPulseLength : strength;
        if (duration == 0f)
        {
            controller.TriggerHapticPulse(microSeconds);
        }
        else
        {
            StopCoroutine("Vibrate");
            StartCoroutine(Vibrate(microSeconds, duration));
        }
    }

    private IEnumerator Vibrate(ushort strength, float duration)
    {
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            controller.TriggerHapticPulse(strength);
            yield return null;
        }
    }

    private float GetTouchpadAngle(Vector2 touchpadAxis)
    {
        if (touchpadAxis == Vector2.zero)
        {
            return 0f;
        }
        float angle = Mathf.Atan2(touchpadAxis.y, touchpadAxis.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }
        return angle;
    }

    #region Dpad Touch/Press Detection

    private Direction GetDPadDirection(float angle)
    {
        if (touchpadAxis.magnitude < dpadDeadzone)
        {
            return Direction.None;
        }
        else if (45f < angle && angle <= 135f)
        {
            return Direction.Up;
        }
        else if (135f < angle && angle <= 225f)
        {
            return Direction.Left;
        }
        else if (225f < angle && angle <= 315f)
        {
            return Direction.Down;
        }
        else
        {
            return Direction.Right;
        }
    }

    private void DpadTouchStart(Direction direction)
    {
        if (direction == Direction.Up)
        {
            OnDpadUpTouchedStart();
        }
        else if (direction == Direction.Down)
        {
            OnDpadDownTouchedStart();
        }
        else if (direction == Direction.Right)
        {
            OnDpadRightTouchedStart();
        }
        else if (direction == Direction.Left)
        {
            OnDpadLeftTouchedStart();
        }
    }

    private void DpadTouchEnd(Direction direction)
    {
        if (direction == Direction.Up)
        {
            OnDpadUpTouchedEnd();
        }
        else if (direction == Direction.Down)
        {
            OnDpadDownTouchedEnd();
        }
        else if (direction == Direction.Right)
        {
            OnDpadRightTouchedEnd();
        }
        else if (direction == Direction.Left)
        {
            OnDpadLeftTouchedEnd();
        }
    }

    private void DpadPressStart(Direction direction)
    {
        if (direction == Direction.Up)
        {
            OnDpadUpPressedStart();
        }
        else if (direction == Direction.Down)
        {
            OnDpadDownPressedStart();
        }
        else if (direction == Direction.Right)
        {
            OnDpadRightPressedStart();
        }
        else if (direction == Direction.Left)
        {
            OnDpadLeftPressedStart();
        }
    }

    private void DpadPressEnd(Direction direction)
    {
        if (direction == Direction.Up)
        {
            OnDpadUpPressedEnd();
        }
        else if (direction == Direction.Down)
        {
            OnDpadDownPressedEnd();
        }
        else if (direction == Direction.Right)
        {
            OnDpadRightPressedEnd();
        }
        else if (direction == Direction.Left)
        {
            OnDpadLeftPressedEnd();
        }
    }

    #endregion
}
