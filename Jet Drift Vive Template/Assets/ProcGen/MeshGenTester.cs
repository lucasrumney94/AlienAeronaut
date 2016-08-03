using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenTester : MonoBehaviour {

    public Vector2 regionSize;
    public int regions;

    void OnEnable()
    {
        Generate();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
    }

    public void Generate()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh;
        if (meshFilter.sharedMesh == null)
        {
            mesh = new Mesh();
        }
        else
        {
            mesh = meshFilter.sharedMesh;
        }
        VoroniMeshGenerator.GenerateVoroniMesh(mesh, regionSize.x, regionSize.y, regions);
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;

        //for (int i = 0; i < mesh.vertices.Length; i++)
        //{
        //    Debug.Log(mesh.vertices[i]);
        //}
    }

    //void OnDrawGizmos()
    //{
    //    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
    //    for (int i = 0; i < mesh.vertices.Length; i++)
    //    {
    //        Gizmos.color = Color.Lerp(Color.white, Color.red, (float)i / (mesh.vertexCount + 1));
    //        Gizmos.DrawCube(mesh.vertices[i], Vector3.one * 0.5f);
    //    }
    //}
}
