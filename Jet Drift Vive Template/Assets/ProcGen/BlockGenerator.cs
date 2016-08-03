using UnityEngine;
using System.Collections;

/// <summary>
/// Created and destroyed by a CityGenerator, handles creation of building objects, and passes them a base mesh to generate from
/// </summary>
public class BlockGenerator : MonoBehaviour {

    public bool pointsSet = false;
    public bool generationDone = false;

    public Int2 indexPosition;

    public float blockSize;
    public float roadWidth;
    public int buildingCount;
    public Vector3[] pointList;
    public BuildingGenerator buildingGeneratorPrefab;
    public BuildingGenerator[] buildings;

    public void Initialize(Int2 index, int buildingCount, float blockSize, float roadWidth)
    {
        indexPosition = index;
        this.blockSize = blockSize;
        this.roadWidth = roadWidth;
        this.buildingCount = buildingCount;
        pointList = VoroniMeshGenerator.GeneratePointsList(blockSize, blockSize, buildingCount);

        pointsSet = true;
    }

    //Activate gameObject and provide pointlist to adjacent blocks, but do not generate any geometry yet
    public void WakeUp()
    {
        gameObject.SetActive(true);
    }

    public void Generate(Vector3[] surroundingPoints) //Should this take in a reference to the surrounding blocks?
    {
        if (generationDone == false && pointsSet == true)
        {
            //Do generation stuff
            Vector3[] origins;
            Vector3 bottomLeftCorner = new Vector3(-blockSize, 0f, -blockSize);
            Vector3 topRightCorner = new Vector3(blockSize * 2, 0f, blockSize * 2);
            Mesh[] buildingBases = VoroniMeshGenerator.GenerateVoroniIslands(pointList, surroundingPoints, bottomLeftCorner, topRightCorner, blockSize, blockSize, roadWidth, out origins);

            for (int i = 0; i < buildingCount; i++)
            {
                BuildingGenerator newBuilding = Instantiate(buildingGeneratorPrefab);
                newBuilding.transform.SetParent(transform);
                newBuilding.SetBaseMesh(buildingBases[i], origins[i]);
            }
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
