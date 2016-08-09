using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates a building from a footprint mesh
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingGenerator : MonoBehaviour {

    public float minHeight;
    public float maxHeight;
    public float taper;

    public Mesh footprint;

    private MeshFilter meshFilter;

    public void SetBaseMesh(Mesh newBase, Vector3 origin)
    {
        meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = newBase;

        transform.localPosition = origin;
    }

    //Extrude base mesh
    public void Generate()
    {
        //Determine height based on size of base or some global variable
        float height = Random.Range(minHeight, maxHeight);

        int baseVertLength = meshFilter.mesh.vertexCount;
        Extrude(meshFilter.mesh, height);
        TaperTopOfExtrude(meshFilter.mesh, baseVertLength, taper);
    }

    private void Extrude(Mesh baseMesh, float height)
    {
        List<Vector3> extrudedVertices = new List<Vector3>();
        extrudedVertices.AddRange(baseMesh.vertices);
        //Create a new quad for each edge of the base, and move the original vertex upwards by height
        extrudedVertices[0] += Vector3.up * height;
        for (int v = 1; v < baseMesh.vertexCount; v++)
        {
            //Get positions for quad corners
            Vector3 v1 = baseMesh.vertices[v];
            int wrapIndexToOne = v + 1 < baseMesh.vertexCount ? v + 1 : 1;
            Vector3 v2 = baseMesh.vertices[wrapIndexToOne];
            Vector3 v3 = v1 + (Vector3.up * height);
            Vector3 v4 = v2 + (Vector3.up * height);

            //Assign to vertex list
            extrudedVertices.Add(v1);
            extrudedVertices.Add(v2);
            extrudedVertices.Add(v3);
            extrudedVertices.Add(v4);

            //Move original vertex upwards
            extrudedVertices[v] = v3;
        }

        //Add original triangles to list
        List<int> extrudedTriangles = new List<int>();
        extrudedTriangles.AddRange(baseMesh.triangles);

        //Create the side triangles
        for (int quadIndex = baseMesh.vertexCount; quadIndex < extrudedVertices.Count; quadIndex += 4)
        {
            //Create 2 triangles
            extrudedTriangles.Add(quadIndex);
            extrudedTriangles.Add(quadIndex + 1);
            extrudedTriangles.Add(quadIndex + 2);

            extrudedTriangles.Add(quadIndex + 3);
            extrudedTriangles.Add(quadIndex + 2);
            extrudedTriangles.Add(quadIndex + 1);
        }

        //Assign to base mesh
        baseMesh.Clear();
        baseMesh.name = "Extruded Base";
        baseMesh.vertices = extrudedVertices.ToArray();
        baseMesh.triangles = extrudedTriangles.ToArray();
        baseMesh.RecalculateNormals();
    }

    private void TaperTopOfExtrude(Mesh extrudedMesh, int baseVertLength, float value)
    {
        Vector3[] vertices = extrudedMesh.vertices;
        for (int v = 1; v < baseVertLength; v++)
        {
            vertices[v] = Vector3.Lerp(vertices[v], vertices[0], value);
            //Set the top 2 vertices of each quad to equal verticess[v]
            int v3 = baseVertLength + ((v - 1) * 4) + 2; //Get the index of the top left corner of the quad
            int v4 = v3 - 3; //Subtract three to get the index of the top right corner of the previous quad
            if (v4 < baseVertLength)
            {
                v4 = vertices.Length - 1;
            }

            vertices[v3] = vertices[v];
            vertices[v4] = vertices[v];
        }
        //Assign top right corner of last quad
        vertices[vertices.Length - 1] = vertices[1];
        extrudedMesh.vertices = vertices;
        extrudedMesh.RecalculateNormals();
    }

    //void OnDrawGizmosSelected()
    //{
    //    if (Application.isPlaying)
    //    {
    //        for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
    //        {
    //            Gizmos.color = Color.Lerp(Color.white, Color.red, (float)i / (meshFilter.mesh.vertices.Length + 1));
    //            Gizmos.DrawCube(transform.TransformPoint(meshFilter.mesh.vertices[i]), Vector3.one * 0.2f);
    //        }

    //        for (int i = 1; i < meshFilter.mesh.vertices.Length; i++)
    //        {
    //            Gizmos.color = Color.green;
    //            Gizmos.DrawLine(transform.TransformPoint(meshFilter.mesh.vertices[i - 1]), transform.TransformPoint(meshFilter.mesh.vertices[i]));
    //        }
    //    }
    //}
}
