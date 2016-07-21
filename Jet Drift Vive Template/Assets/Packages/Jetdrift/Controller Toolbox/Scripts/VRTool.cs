/*
Base class for tools associated with a vive controller.
Can assign class variables to a list of bool or float options, which can be modified in-game when instantiated through a Toolbox
*/
using UnityEngine;
using System.Collections;

public class VRTool : MonoBehaviour {

    protected ControllerInputTracker vrInput;

    public Option[] toolOptions;

    /// <summary>
    /// The process for initializing the options list isn't intuitive, here is the format it must follow:
    /// -initialize list length
    /// toolOptions = new Option[n];
    /// 
    /// -initialize each option in the list
    /// for example, initializing the float 'speed' as an Option would be done as follows:
    /// toolOptions[0] = new Option(new ReferenceValue<float>(() => speed, v => { speed = v; }), "Speed Option", 0.1f, 5f);
    /// where the 3rd parameter defines a display name for the option, and the 4th and 5th parameters define a range for the slider UI element
    /// 
    /// or in the case of a boolean option:
    /// toolOptions[0] = new Option(new ReferenceValue<bool>(() => run, v => { run = v; }), "Can the character run?");
    /// </summary>
    public virtual void InitializeOptions()
    {
        Debug.LogWarning("Tried to call InitializeOptions on the base class of VRTool! Did you mean to override this function?");
        toolOptions = new Option[0];
    }
}
