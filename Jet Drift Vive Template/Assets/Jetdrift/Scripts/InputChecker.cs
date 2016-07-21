/*
Test script to make sure inputs are working correctly

Observations:
After trigger is released, trigger strength tends to stick at ~0.035, unless manually pushed back to resting position
TriggerTouchedDown is called when triggerStrength > 0.25, but TriggerTouchedUp is not called until triggerStrength < 0.2
TriggerPressedDown is called when triggerStrentgh > 0.55, but TriggerPressedUp is not called until triggerStrength < 0.45
TriggerStrength is exactly 1.0 after the 'click' is heard when pulling
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InputChecker : MonoBehaviour {

    public Text triggerTouched;
    public Text triggerPressed;
    public Text touchpadTouched;
    public Text touchpadPressed;
    public Text gripPressed;
    public Text menuPressed;

    public Text triggerStrength;
    public Text touchpadAxis;
    public Text touchpadAngle;
    public Text dpadDirection;

    private ControllerInputTracker inputTracker;

    void Start()
    {
        inputTracker = transform.GetComponentInParent<ControllerInputTracker>();

        inputTracker.triggerTouchedDown += new ControllerInputDelegate(TriggerTouchedDown);
        inputTracker.triggerTouchedUp += new ControllerInputDelegate(TriggerTouchedUp);
        inputTracker.triggerPressedDown += new ControllerInputDelegate(TriggerPressedDown);
        inputTracker.triggerPressedUp += new ControllerInputDelegate(TriggerPressedUp);
        inputTracker.touchpadTouchedDown += new ControllerInputDelegate(TouchpadTouchedDown);
        inputTracker.touchpadTouchedUp += new ControllerInputDelegate(TouchpadTouchedUp);
        inputTracker.touchpadPressedDown += new ControllerInputDelegate(TouchpadPressedDown);
        inputTracker.touchpadPressedUp += new ControllerInputDelegate(TouchpadPressedUp);
        inputTracker.gripPressedDown += new ControllerInputDelegate(GripPressedDown);
        inputTracker.gripPressedUp += new ControllerInputDelegate(GripPressedUp);
        inputTracker.menuPressedDown += new ControllerInputDelegate(MenuPressedDown);
        inputTracker.menuPressedUp += new ControllerInputDelegate(MenuPressedUp);
    }

    void Update()
    {
        UpdateTriggerStrength(inputTracker.triggerStrength);
        UpdateTouchpadAxis(inputTracker.touchpadAxis);
        UpdateTouchpadAngle(inputTracker.touchpadAngle);
        UpdateDPadDirection(inputTracker.dpadDirection);
    }

    public void TriggerTouchedDown()
    {
        triggerTouched.text = string.Format("Trigger Touched: {0}", true);
    }

    public void TriggerTouchedUp()
    {
        triggerTouched.text = string.Format("Trigger Touched: {0}", false);
    }

    public void TriggerPressedDown()
    {
        triggerPressed.text = string.Format("Trigger Pressed: {0}", true);
    }

    public void TriggerPressedUp()
    {
        triggerPressed.text = string.Format("Trigger Pressed: {0}", false);
    }

    public void TouchpadTouchedDown()
    {
        touchpadTouched.text = string.Format("Touchpad Touched: {0}", true);
    }

    public void TouchpadTouchedUp()
    {
        touchpadTouched.text = string.Format("Touchpad Touched: {0}", false);
    }

    public void TouchpadPressedDown()
    {
        touchpadPressed.text = string.Format("Touchpad Pressed: {0}", true);
    }

    public void TouchpadPressedUp()
    {
        touchpadPressed.text = string.Format("Touchpad Pressed: {0}", false);
    }

    public void GripPressedDown()
    {
        gripPressed.text = string.Format("Grip Pressed: {0}", true);
    }

    public void GripPressedUp()
    {
        gripPressed.text = string.Format("Grip Pressed: {0}", false);
    }

    public void MenuPressedDown()
    {
        menuPressed.text = string.Format("Menu Pressed: {0}", true);
    }

    public void MenuPressedUp()
    {
        menuPressed.text = string.Format("Menu Pressed: {0}", false);
    }

    public void UpdateTriggerStrength(float strength)
    {
        triggerStrength.text = string.Format("Trigger Strength: {0:F3}", strength);
    }

    public void UpdateTouchpadAxis(Vector2 axis)
    {
        touchpadAxis.text = string.Format("Touch Position: ({0:F3}, {1:F3})", axis.x, axis.y);
    }

    public void UpdateTouchpadAngle(float angle)
    {
        touchpadAngle.text = string.Format("Touch Angle: {0:F1}", angle);
    }

    public void UpdateDPadDirection(Direction direction)
    {
        dpadDirection.text = string.Format("DPad Direction: {0}", direction);
    }
}
