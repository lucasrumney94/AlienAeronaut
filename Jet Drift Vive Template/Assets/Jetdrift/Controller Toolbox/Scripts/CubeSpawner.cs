using UnityEngine;
using System.Collections;

public class CubeSpawner : VRTool {

    public GameObject cubePrefab;

    public float initialForce;
    public float scale = 1f;
    public bool spawnEveryFrame;

    void OnEnable()
    {
        InitializeOptions();

        vrInput = transform.GetComponentInParent<ControllerInputTracker>();

        vrInput.triggerPressedDown += new ControllerInputDelegate(SpawnCube);
    }

    void OnDisable()
    {
        vrInput.triggerPressedDown -= new ControllerInputDelegate(SpawnCube); //Remove tool controls from event list when disabled
    }

    public override void InitializeOptions()
    {
        toolOptions = new Option[3];
        toolOptions[0] = new Option(new ReferenceValue<float>(() => initialForce, v => { initialForce = v; }), "Initial force", 0f, 10f);
        toolOptions[1] = new Option(new ReferenceValue<float>(() => scale, v => { scale = v; }), "Initial scale", 0.1f, 5f);
        toolOptions[2] = new Option(new ReferenceValue<bool>(() => spawnEveryFrame, v => { spawnEveryFrame = v; }), "Spawn every frame while trigger held?");
    }

    void Update()
    {
        if (spawnEveryFrame && vrInput.triggerStrength == 1f)
        {
            SpawnCube();
        }
    }

    public void SpawnCube()
    {
        GameObject newCube = Instantiate(cubePrefab, vrInput.position, vrInput.rotation) as GameObject;
        newCube.transform.position += newCube.transform.forward * 0.1f;
        newCube.transform.localScale *= scale;
        newCube.GetComponent<Rigidbody>().velocity = newCube.transform.forward * initialForce;
    }
}
