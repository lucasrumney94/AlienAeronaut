using UnityEngine;
using System.Collections;

public class VibrationTester : VRTool {

    public float strength;
    public float duration;

    void OnEnable()
    {
        InitializeOptions();

        vrInput = transform.GetComponentInParent<ControllerInputTracker>();

        vrInput.triggerPressedDown += new ControllerInputDelegate(Vibrate);
    }

    void OnDisable()
    {
        vrInput.triggerPressedDown -= new ControllerInputDelegate(Vibrate);
    }

    public override void InitializeOptions()
    {
        toolOptions = new Option[2];
        toolOptions[0] = new Option(new ReferenceValue<float>(() => strength, v => { strength= v; }), "Vibration Strength", 0f, 3999f);
        toolOptions[1] = new Option(new ReferenceValue<float>(() => duration, v => { duration = v; }), "Duration", 0f, 0.2f);
    }

    private void Vibrate()
    {
        vrInput.VibrateController((ushort)strength, duration);
    }
}
