using UnityEngine;
using System.Collections;

/// <summary>
/// Created and destroyed by a CityGenerator, handles creation of building objects, and passes them a base mesh to generate from
/// </summary>
public class BlockGenerator : MonoBehaviour {

    public bool pointsSet = false;
    public bool generationDone = false;

    public Int2 indexPosition;

    public int BuildingCount;
    public Vector3[] pointList;
    public BuildingGenerator[] buildings;

    public void Initialize(int pointCount, Int2 index)
    {
        indexPosition = index;
        BuildingCount = pointCount;
        pointList = new Vector3[BuildingCount];
        //Randomly distribute points

        pointsSet = true;
    }

    //Activate gameObject and provide pointlist to adjacent blocks, but do not generate any geometry yet
    public void WakeUp()
    {
        gameObject.SetActive(true);
    }

    public void Generate() //Should this take in a reference to the surrounding blocks?
    {
        if (generationDone == false && pointsSet == true)
        {
            //Do generation stuff

        }

        generationDone = true;
    }

    /// <summary>
    /// Disable gameObject
    /// </summary>
    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
