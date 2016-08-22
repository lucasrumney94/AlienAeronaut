using UnityEngine;
using System.Collections;

/// <summary>
/// Created and destroyed by a CityGenerator, handles creation of building objects, and passes them a base mesh to generate from
/// Also allows for the creation of a second layer of buildings, and the creation of links and passages between layers
/// </summary>
public class BlockGenerator : MonoBehaviour {

    public bool pointsSet = false;
    public bool generationDone = false;

    public Int2 indexPosition;

    public float blockSize;
    public float roadWidth;
    public float topLayerElevation;
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

        //CreateBaseMesh();

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
                //Create 3 new buildings, one for the base layer, one for bottom side of top layer and one for top side of top layer
                BuildingGenerator newBuilding = Instantiate(buildingGeneratorPrefab);
                newBuilding.transform.SetParent(transform);
                newBuilding.SetBaseMesh(buildingBases[i], origins[i]);
                newBuilding.Generate();

                Vector3 elevatedOrigin = origins[i] + (Vector3.up * topLayerElevation); //Reposition origin of mesh to be on the upper layer plane

                newBuilding = Instantiate(buildingGeneratorPrefab);
                newBuilding.transform.SetParent(transform);
                newBuilding.SetBaseMesh(buildingBases[i], elevatedOrigin);
                newBuilding.Generate(true);

                newBuilding = Instantiate(buildingGeneratorPrefab);
                newBuilding.transform.SetParent(transform);
                newBuilding.SetBaseMesh(buildingBases[i], elevatedOrigin);
                newBuilding.Generate();
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

    //private void CreateBaseMesh() //Obsolete now that buildings create their own bases
    //{
    //    MeshFilter meshFilter = GetComponent<MeshFilter>();

    //    Mesh baseMesh;
    //    if (meshFilter.sharedMesh == null)
    //    {
    //        baseMesh = new Mesh();
    //    }
    //    else
    //    {
    //        baseMesh = meshFilter.sharedMesh;
    //    }

    //    Vector3[] vertices = new Vector3[4];

    //    vertices[0] = new Vector3(-blockSize / 2f, 0f, -blockSize / 2f);
    //    vertices[1] = new Vector3(blockSize / 2f, 0f, -blockSize / 2f);
    //    vertices[2] = new Vector3(-blockSize / 2f, 0f, blockSize / 2f);
    //    vertices[3] = new Vector3(blockSize / 2f, 0f, blockSize / 2f);

    //    int[] triangles = new int[6];

    //    triangles[0] = 0;
    //    triangles[1] = 2;
    //    triangles[2] = 1;

    //    triangles[3] = 3;
    //    triangles[4] = 1;
    //    triangles[5] = 2;

    //    Vector3[] normals = new Vector3[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        normals[i] = Vector3.up;
    //    }

    //    baseMesh.Clear();
    //    baseMesh.name = "Block Base";
    //    baseMesh.vertices = vertices;
    //    baseMesh.triangles = triangles;
    //    baseMesh.normals = normals;

    //    meshFilter.sharedMesh = baseMesh;
    //}
}
