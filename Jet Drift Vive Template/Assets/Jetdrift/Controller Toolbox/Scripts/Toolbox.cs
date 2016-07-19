/*
Add this component to an empty GameObject childed to a controller.
Enables managing of multiple VRTools, and in-game modifiable options on each tool
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

//Magic from stackoverflow to make the options menu work
public class ReferenceValue<T> //Taken from Eric Lippert's answer to this question: http://stackoverflow.com/questions/2256048/store-a-reference-to-a-value-type
{
    private Func<T> getter;
    private Action<T> setter;
    public ReferenceValue(Func<T> getter, Action<T> setter)
    {
        this.getter = getter;
        this.setter = setter;
    }
    public T Value
    {
        get { return getter(); }
        set { setter(value); }
    }
}

/// <summary>
/// Struct that can take a float or bool class variable, and allows assigning to the source variable whenever the Option is changed
/// The ReferenceValue parameter needs to be assigned like this (for a class variable 'speed'):
/// new ReferenceValue<float>(() => speed, v => { speed = v; })
/// </summary>
[Serializable]
public struct Option
{
    public Type type;

    public string name;

    private ReferenceValue<bool> boolReference;
    private ReferenceValue<float> floatReference;

    public bool boolValue
    {
        get
        {
            return boolReference.Value;
        }
        set
        {
            boolReference.Value = value;
        }
    }
    public float floatValue
    {
        get
        {
            return floatReference.Value;
        }
        set
        {
            floatReference.Value = value;
        }
    }
    public float minValue;
    public float maxValue;

    public Option(ReferenceValue<float> floatReference, string optionName, float min = 0f, float max = 1f)
    {
        type = typeof(float);
        name = optionName;
        this.boolReference = new ReferenceValue<bool>(() => false, v => { });
        this.floatReference = floatReference;
        minValue = min;
        maxValue = max;
    }

    public Option(ReferenceValue<bool> boolReference, string optionName)
    {
        type = typeof(bool);
        name = optionName;
        this.boolReference = boolReference;
        this.floatReference = new ReferenceValue<float>(() => 0f, v => { });
        minValue = 0f;
        maxValue = 0f;
    }
}

public class Toolbox : MonoBehaviour {

    private ControllerInputTracker inputTracker;

    public Canvas selectionCanvas; //Canvas for switching between tools
    public RectTransform selectionContent; //Assign to the 'content' child of the selectionCanvas ScrollRect

    public Canvas optionsCanvas; //Canvas for changing tool options
    public RectTransform optionsContent; //Assign to the 'content' child of the selectionCanvas ScrollRect

    private GameObject[] optionsUIElements;

    //Can be found in Jetdrift/Controller Toolbox/Prefabs/UI
    public Button selectionButtonPrefab;
    public Toggle toggleOptionPrefab;
    public Slider sliderOptionPrefab;

    public VRTool[] toolPrefabs; //List of VRTools available to switch between

    public VRTool activeTool; //Current selected tool, can be void by default

    public Option[] activeToolOptions; //Leave empty, can be used to modify tool options in the inspector instead of in-game

    void OnEnable()
    {
        inputTracker = transform.GetComponentInParent<ControllerInputTracker>();

        inputTracker.menuPressedDown += new ControllerInputDelegate(StartListeningForLongPress);
    }

    void OnDisable()
    {
        inputTracker.menuPressedDown -= new ControllerInputDelegate(StartListeningForLongPress);
    }

    void Start()
    {
        PopulateToolBoxMenu();
    }

    void Update()
    {
        ListenForLongPress();
        UpdateOptionsUI();
    }

    #region Press length detection

    private bool listeningForLongPress = false;
    private float timeOfLastMenuDown;

    /// <summary>
    /// Called when the menu toggle button is pressed
    /// </summary>
    private void StartListeningForLongPress()
    {
        listeningForLongPress = true;
        timeOfLastMenuDown = Time.time;
    }

    private void StopListeningForLongPress()
    {
        listeningForLongPress = false;
    }

    /// <summary>
    /// Called every frame
    /// Check if the menu toggle button has been held down for at least one second, and Open the toolbox menu if it has.
    /// Otherwise, open the tool options menu, or close the toolbox menu if it's already open.
    /// </summary>
    private void ListenForLongPress()
    {
        if (listeningForLongPress)
        {
            if (Time.time - timeOfLastMenuDown < 1f)
            {
                if (inputTracker.menuPressed == false)
                {
                    //Stop Listneing
                    StopListeningForLongPress();
                    if (selectionCanvas.gameObject.activeInHierarchy) //If selection menu is already open
                    {
                        ToggleToolboxMenu();
                    }
                    else
                    {
                        //Do short press action
                        ToggleToolOptions();
                    }
                }
            }
            else
            {
                //Stop listening
                StopListeningForLongPress();
                //Do long press action
                CloseToolOptions();
                ToggleToolboxMenu();
            }
        }
    }

    #endregion

    #region Toolbox menu

    public void ToggleToolboxMenu()
    {
        if (selectionCanvas.gameObject.activeInHierarchy)
        {
            CloseToolboxMenu();
        }
        else if (selectionCanvas.gameObject.activeInHierarchy == false)
        {
            OpenToolboxMenu();
        }
    }

    private void OpenToolboxMenu()
    {
        //Temporarily disable active tool
        if (activeTool != null)
        {
            activeTool.gameObject.SetActive(false);
        }

        //Open selection canvas
        selectionCanvas.gameObject.SetActive(true);
    }

    private void CloseToolboxMenu()
    {
        //Reeanble active tool
        if (activeTool != null)
        {
            activeTool.gameObject.SetActive(true);
        }

        //Disable canvas
        selectionCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiate a menu button in the selection canvas for each assigned VRTool prefab,
    /// set each button's text and onClick event to call ChangeActiveTool([associated vr tool]),
    /// and sets the navitagion mode on each element to vertical.
    /// </summary>
    public void PopulateToolBoxMenu()
    {
        foreach (VRTool tool in toolPrefabs)
        {
            VRTool newTool = Instantiate(tool).GetComponent<VRTool>();
            newTool.transform.SetParent(inputTracker != null ? inputTracker.transform : transform.parent);
            newTool.gameObject.SetActive(false);

            Button toolButton = Instantiate(selectionButtonPrefab).GetComponent<Button>();
            toolButton.transform.SetParent(selectionContent);
            toolButton.transform.localScale = Vector3.one;
            toolButton.transform.localPosition = Vector3.zero;
            toolButton.transform.localRotation = Quaternion.identity;
            Text buttonText = toolButton.transform.GetChild(0).GetComponent<Text>();
            if (buttonText != null)
            {
                buttonText.text = tool.name;
            }
            else
            {
                Debug.Log("Button prefab did not have a UI Text as its first child!");
            }
            toolButton.onClick = new Button.ButtonClickedEvent();
            toolButton.onClick.AddListener(() => ChangeActiveTool(newTool));
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Vertical;
            toolButton.navigation = navigation;
        }
    }

    /// <summary>
    /// Sets the active tool to newTool, and closes the selection menu
    /// </summary>
    /// <param name="newTool"></param>
    public void ChangeActiveTool(VRTool newTool)
    {
        activeTool = newTool;
        CloseToolboxMenu();
    }

    #endregion

    #region Tool Options Menu

    private void ToggleToolOptions()
    {
        if (optionsCanvas.gameObject.activeInHierarchy)
        {
            CloseToolOptions();
        }
        else if (optionsCanvas.gameObject.activeInHierarchy == false)
        {
            OpenToolOptions();
        }
    }

    private void OpenToolOptions()
    {
        //Temporarily disable active tool
        if (activeTool != null)
        {
            activeToolOptions = activeTool.toolOptions;

            //Remove all children of content
            Transform contentTransform = optionsContent.transform;
            int childCount = contentTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(contentTransform.GetChild(i).gameObject);
            }

            //Spawn new Toggle or Slider for each option
            PopulateToolOptionsMenu();

            activeTool.gameObject.SetActive(false);
        }

        optionsCanvas.gameObject.SetActive(true);
    }

    private void CloseToolOptions()
    {
        //Reeanble active tool
        if (activeTool != null)
        {
            activeTool.gameObject.SetActive(true);
        }

        optionsCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates UI Toggles and Sliders for the Options on the active tool,
    /// sets the UI object OnValueChanged event to assign to the associated Option,
    /// and sets up the UI navigation mode on each button to vertical.
    /// </summary>
    private void PopulateToolOptionsMenu()
    {
        optionsUIElements = new GameObject[activeToolOptions.Length];
        for (int i = 0; i < activeToolOptions.Length; i++)
        {
            GameObject newOptionUIElement;
            if (activeToolOptions[i].type == typeof(bool))
            {
                newOptionUIElement = Instantiate(toggleOptionPrefab.gameObject);
            }
            else if (activeToolOptions[i].type == typeof(float))
            {
                newOptionUIElement = Instantiate(sliderOptionPrefab.gameObject);
            }
            else
            {
                Debug.Log("Tool option not set to valid type! Use either float or bool.");
                return;
            }

            optionsUIElements[i] = newOptionUIElement;

            newOptionUIElement.transform.SetParent(optionsContent.transform);
            newOptionUIElement.transform.localScale = Vector3.one;
            newOptionUIElement.transform.localPosition = Vector3.zero;
            newOptionUIElement.transform.localRotation = Quaternion.identity;

            Text optionText = newOptionUIElement.transform.GetComponentInChildren<Text>();
            if (optionText != null)
            {
                optionText.text = activeToolOptions[i].name;
            }
            else
            {
                Debug.Log("Option prefab did not have a UI Text as a child!");
            }

            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Vertical;

            Toggle optionToggle = newOptionUIElement.GetComponent<Toggle>();
            Slider optionSlider = newOptionUIElement.GetComponent<Slider>();

            int currentIndex = i; //Necessary for some reason to do with delegates / lambda

            if (optionToggle != null) //If the instantiated UI object is a toggle
            {
                optionToggle.isOn = activeToolOptions[i].boolValue;
                optionToggle.navigation = navigation;

                optionToggle.onValueChanged.AddListener((value) => { SetBoolOption(currentIndex, value); });
            }
            else if (optionSlider != null) //If the instantiated UI object is a slider
            {
                optionSlider.minValue = activeToolOptions[i].minValue;
                optionSlider.maxValue = activeToolOptions[i].maxValue;
                optionSlider.value = activeToolOptions[i].floatValue;
                optionSlider.navigation = navigation;

                optionSlider.onValueChanged.AddListener((value) => { SetFloatOption(currentIndex, value); });
            }
        }
    }

    /// <summary>
    /// Called every frame.
    /// Updates the Options UI to match the inspector Options values
    /// </summary>
    private void UpdateOptionsUI()
    {
        if (optionsUIElements != null)
        {
            for (int i = 0; i < optionsUIElements.Length; i++)
            {
                Toggle optionToggle = optionsUIElements[i].GetComponent<Toggle>();
                Slider optionSlider = optionsUIElements[i].GetComponent<Slider>();

                if (optionToggle != null)
                {
                    optionToggle.isOn = activeToolOptions[i].boolValue;
                }
                else if (optionSlider != null)
                {
                    optionSlider.minValue = activeToolOptions[i].minValue;
                    optionSlider.maxValue = activeToolOptions[i].maxValue;
                    optionSlider.value = activeToolOptions[i].floatValue;
                    Text optionText = optionSlider.transform.GetComponentInChildren<Text>();
                    if (optionText != null)
                    {
                        optionText.text = string.Format("{0}: {1:F2}", activeToolOptions[i].name, optionSlider.value);
                    }
                }
            }
        }
    }

    public void SetFloatOption(int optionIndex, float newValue)
    {
        activeToolOptions[optionIndex].floatValue = newValue;
    }

    public void SetBoolOption(int optionIndex, bool newValue)
    {
        activeToolOptions[optionIndex].boolValue = newValue;
    }

    #endregion
}
