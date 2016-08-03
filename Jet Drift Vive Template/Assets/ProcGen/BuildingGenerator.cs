using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a building from a footprint mesh
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingGenerator : MonoBehaviour {

    public Mesh footprint;

    private MeshFilter meshFilter;

    public void SetBaseMesh(Mesh newBase, Vector3 origin)
    {
        meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = newBase;

        transform.localPosition = origin;
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
    //    }
    //}
}
