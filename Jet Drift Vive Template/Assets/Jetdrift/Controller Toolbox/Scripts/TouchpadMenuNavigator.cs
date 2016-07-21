/*
Attatch to a canvas object to enable navigation with the vive touchpad. Will only function on one canvas at a time per controller.
It is highly recommended to assign a bright and distinctive 'Highlighted Color' property on all selectable UI elements on the canvas.
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TouchpadMenuNavigator : MonoBehaviour {

    public GameObject currentSelected; //Currently highlighted UI object

    public ScrollRect scrollRect; //Optional parameter to help navigate menus inside a scrollable area

    private EventSystem eventSystem;
    private ControllerInputTracker inputTracker;

    void OnEnable()
    {
        FindEventSystem();
        FindScrollRect();

        inputTracker = transform.GetComponentInParent<ControllerInputTracker>();

        inputTracker.dpadUpTouchedStart += new ControllerInputDelegate(NavigateMenuUp);
        inputTracker.dpadDownTouchedStart += new ControllerInputDelegate(NavigateMenuDown);
        inputTracker.dpadRightTouchedStart += new ControllerInputDelegate(NavigateMenuRight);
        inputTracker.dpadLeftTouchedStart += new ControllerInputDelegate(NavigateMenuLeft);
        inputTracker.triggerPressedDown += new ControllerInputDelegate(MenuSelect);
    }

    void OnDisable()
    {
        inputTracker.dpadUpTouchedStart -= new ControllerInputDelegate(NavigateMenuUp);
        inputTracker.dpadDownTouchedStart -= new ControllerInputDelegate(NavigateMenuDown);
        inputTracker.dpadRightTouchedStart -= new ControllerInputDelegate(NavigateMenuRight);
        inputTracker.dpadLeftTouchedStart -= new ControllerInputDelegate(NavigateMenuLeft);
        inputTracker.triggerPressedDown -= new ControllerInputDelegate(MenuSelect);
    }

    private void FindEventSystem()
    {
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
    }

    private void FindScrollRect()
    {
        if (scrollRect == null)
        {
            scrollRect = transform.GetComponentInChildren<ScrollRect>();
        }
    }

    void Update()
    {
        if (currentSelected == null)
        {
            ResetSelected();
        }
    }

    /// <summary>
    /// Ensure that some UI object is always selected, to keep the navigation from breaking
    /// </summary>
    private void ResetSelected()
    {
        if (scrollRect == null && transform.childCount > 0)
        {
            currentSelected = transform.GetChild(0).gameObject;
            eventSystem.SetSelectedGameObject(currentSelected);
        }
        else if (scrollRect.content.transform.childCount > 0)
        {
            currentSelected = scrollRect.content.transform.GetChild(0).gameObject;
            eventSystem.SetSelectedGameObject(currentSelected);
        }
    }

    /// <summary>
    /// If the navigation mode on all UI objects is not set to 'Explicit', Unity will make navigation links between seperate canvases, causing broken behavior when navigating.
    /// This function is a simple solution, and needs to be called whenever navigation takes place.
    /// </summary>
    private void CheckForSelectedOutOfBounds()
    {
        if (currentSelected.transform.parent != scrollRect.content.transform)
        {
            ResetSelected();
        }
    }

    private void NavigateMenuUp()
    {
        NavigateMenu(MoveDirection.Up);
    }

    private void NavigateMenuDown()
    {
        NavigateMenu(MoveDirection.Down);
    }

    private void NavigateMenuRight()
    {
        NavigateMenu(MoveDirection.Right);
    }

    private void NavigateMenuLeft()
    {
        NavigateMenu(MoveDirection.Left);
    }

    /// <summary>
    /// Hack-y solution to allow navigation of multiple UI's with one EventSystem
    /// Works for both navigating between objects and setting values on sliders
    /// </summary>
    /// <param name="direction">Direction to send to the UI navigation system</param>
    private void NavigateMenu(MoveDirection direction)
    {
        eventSystem.SetSelectedGameObject(currentSelected);
        AxisEventData axisData = new AxisEventData(eventSystem);
        axisData.moveDir = direction;
        ExecuteEvents.Execute(currentSelected, axisData, ExecuteEvents.moveHandler);
        currentSelected = eventSystem.currentSelectedGameObject;
        CheckForSelectedOutOfBounds();
        ScrollToActive();
    }

    /// <summary>
    /// Equivalent to clicking on the current selected UI object
    /// </summary>
    private void MenuSelect()
    {
        eventSystem.SetSelectedGameObject(currentSelected);
        BaseEventData baseData = new BaseEventData(eventSystem);
        currentSelected = eventSystem.currentSelectedGameObject;
        baseData.selectedObject = currentSelected;
        ExecuteEvents.Execute(currentSelected, baseData, ExecuteEvents.submitHandler);
    }

    /// <summary>
    /// If the selected UI object is outside the boundaries of its content rect, scroll scrollRect so currentSelected is fully visable.
    /// </summary>
    private void ScrollToActive()
    {
        if (scrollRect != null && currentSelected != null)
        {
            RectTransform currentTransform = currentSelected.GetComponent<RectTransform>();
            RectTransform content = scrollRect.content;

            if (Mathf.Abs(currentTransform.anchoredPosition.y) < Mathf.Abs(content.offsetMax.y) || Mathf.Abs(currentTransform.anchoredPosition.y) > Mathf.Abs(content.rect.height + content.offsetMin.y))
            {
                Vector2 normalizedPosition = new Vector2(currentTransform.anchoredPosition.x / content.rect.width, currentTransform.anchoredPosition.y / content.rect.height);
                scrollRect.verticalNormalizedPosition = 1f - Mathf.Abs(normalizedPosition.y);
            }
        }
    }
}
