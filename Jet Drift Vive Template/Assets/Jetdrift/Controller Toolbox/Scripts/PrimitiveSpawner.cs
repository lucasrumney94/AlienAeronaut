using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrimitiveSpawner : VRTool
{

    private ControllerInputTracker vrInput;

    public List<GameObject> customPrimitives;
    public List<float> customPrimitiveScales;

    public float initialForce = 3.0f;
    public float primitiveScale = 0.3f;
    public bool RepeatSpawn = true;

    public int currentPrimitive;

    private GameObject previewPrimitive;

    void OnEnable()
    {
        StartCoroutine("RepeatSpawner");

        vrInput = transform.GetComponentInParent<ControllerInputTracker>();

        vrInput.triggerPressedDown += new ControllerInputDelegate(SpawnCurrentPrimitive);

        vrInput.touchpadPressedDown += new ControllerInputDelegate(NextPrimitive);

        InitializeOptions();

        previewPrimitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        previewPrimitive.transform.localScale = 0.01f*Vector3.one;
        previewPrimitive.transform.parent = this.gameObject.transform;
        previewPrimitive.transform.localPosition = new Vector3(0.0f, 0.05f, 0.05f);
    }

    void OnDisable()
    {
        StopCoroutine("RepeatSpawner");
        vrInput.triggerPressedDown -= new ControllerInputDelegate(SpawnCurrentPrimitive); //Remove tool controls from event list when disabled
    }

    public override void InitializeOptions()
    {
        toolOptions = new Option[3];
        toolOptions[0] = new Option(new ReferenceValue<float>(() => initialForce, v => { initialForce = v; }), "Initial force", 0f, 10f);
        toolOptions[1] = new Option(new ReferenceValue<float>(() => primitiveScale, v => { primitiveScale = v; }), "Initial scale", 0.1f, 5f);
        toolOptions[2] = new Option(new ReferenceValue<bool>(() => RepeatSpawn, v => { RepeatSpawn = v; }), "Spawn every frame while trigger held?");
    }


    public IEnumerator RepeatSpawner()
    {
        for (;;)
        {
            if (RepeatSpawn && vrInput.triggerPressed)
            {
                SpawnCurrentPrimitive();
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void SpawnCurrentPrimitive()
    {
        if (currentPrimitive == 0)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ; 
            temp.transform.localScale *= primitiveScale;
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;

        }
        else if (currentPrimitive == 1)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ;
            temp.transform.localScale *= primitiveScale;
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }
        else if (currentPrimitive == 2)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ;
            temp.transform.localScale *= primitiveScale;
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }
        else if (currentPrimitive == 3)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ;
            temp.transform.localScale *= primitiveScale/10;
            temp.AddComponent<BoxCollider>();
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }
        else if (currentPrimitive == 4)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ;
            temp.transform.localScale *= primitiveScale;
            temp.AddComponent<BoxCollider>();
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }
        else if (currentPrimitive == 5)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.transform.position = vrInput.transform.position + vrInput.transform.forward ;
            temp.transform.localScale *= primitiveScale;
            temp.AddComponent<Rigidbody>().drag = 0.0f;
            temp.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }
        else if (currentPrimitive >= 6)
        {
            GameObject newCustomPrimitive = Instantiate(customPrimitives[currentPrimitive - 6], vrInput.position, vrInput.rotation) as GameObject;
            newCustomPrimitive.transform.position = vrInput.transform.position + vrInput.transform.forward;
            newCustomPrimitive.transform.localScale *= customPrimitiveScales[currentPrimitive - 6];
            //newCustomPrimitive.AddComponent<Rigidbody>().drag = 0.0f;
            newCustomPrimitive.GetComponent<Rigidbody>().velocity = initialForce * vrInput.transform.forward;
        }


    }

    public void NextPrimitive()
    {
        currentPrimitive++;

        if (currentPrimitive > 5 + customPrimitives.Count)
        {
            currentPrimitive = 0;
        }

        if (currentPrimitive == 0)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule).GetComponent<MeshFilter>().mesh;
        }
        else if (currentPrimitive == 1)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
        }
        else if (currentPrimitive == 2)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshFilter>().mesh;
        }
        else if (currentPrimitive == 3)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshFilter>().mesh;
        }
        else if (currentPrimitive == 4)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshFilter>().mesh;
        }
        else if (currentPrimitive == 5)
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh;
        }

        if (currentPrimitive >= 6) //6 because 5 basic prims
        {
            previewPrimitive.GetComponent<MeshFilter>().mesh = customPrimitives[currentPrimitive - 6].GetComponent<MeshFilter>().sharedMesh;
        }

    }
}
