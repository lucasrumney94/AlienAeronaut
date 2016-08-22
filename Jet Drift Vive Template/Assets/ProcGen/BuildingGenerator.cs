using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates a building from a footprint mesh
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingGenerator : MonoBehaviour {

    public Color color;

    public float minHeight;
    public float maxHeight;
    public float bottomTaper;
    public float topTaper;

    public Mesh[] replacementMeshes; //List of meshes that building sides will be randomly replaced with. Should be rectangular, facing upwards with top and bottom parallel to the x-axis, and the sides parallel to the z-axis

    public Mesh footprint;

    private MeshFilter meshFilter;

    public void SetBaseMesh(Mesh newBase, Vector3 origin)
    {
        meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = newBase;

        transform.localPosition = origin;
    }
    
    /// <summary>
    /// Turn a base mesh into a building and surrounding street
    /// </summary>
    /// <param name="elevation">Used for specifying the offset of upper layers</param>
    /// <param name="upsidedown">Set true to generate the underside of a layer of buildings</param>
    public void Generate(bool upsidedown = false)
    {
        //Determine height based on size of base or some global variable
        float height = Random.Range(minHeight, maxHeight);
        color = new Color(Random.value, Random.value, Random.value);

        int baseVertLength = meshFilter.mesh.vertexCount;

        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(meshFilter.mesh.vertices);

        List<int> triangles = new List<int>();

        int originalVertCount = vertices.Count;
        int edges = originalVertCount - 1;
        int originOfCurrentEdge = 1; //Keeps track of the first point on the currently worked on edge

        //Create ring of verts around base
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = edgeVert + originOfCurrentEdge;
            //Lerp ring inwards by bottomTaper
            Vector3 newCorner = Vector3.Lerp(vertices[vertIndex], vertices[0], bottomTaper);
            vertices.Add(newCorner);

            //Create triangles linking bottom and top of current quad
            int p1 = vertIndex;
            int p2 = edgeVert + 1 < edges ? p1 + 1 : originOfCurrentEdge;
            int p3 = vertIndex + edges;
            int p4 = edgeVert + 1 < edges ? p3 + 1 : originOfCurrentEdge + edges;

            triangles.Add(p1);
            triangles.Add(p2);
            triangles.Add(p3);

            triangles.Add(p4);
            triangles.Add(p3);
            triangles.Add(p2);
        }
        originOfCurrentEdge += edges; //Move to top edge of current quad

        //Create sides of building; sides are composed of seperated quads, unlike bases, so need twice the number of vertices

        //Duplicate current edge ring for base of side quads
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = edgeVert + originOfCurrentEdge;
            int nextVertIndex = edgeVert + 1 < edges ? vertIndex + 1 : originOfCurrentEdge;
            Vector3 newCorner = vertices[vertIndex];
            Vector3 nextNewCorner = vertices[nextVertIndex];
            vertices.Add(newCorner);
            vertices.Add(nextNewCorner);
        }
        originOfCurrentEdge += edges; //Move to bottom edge of next quad

        int originOfSideEdge = originOfCurrentEdge; //Cache reference to vert index at base of the side quads, for use after basic shape generation is done

        //Move 0th vertex to the top of the extrude
        vertices[0] += Vector3.up * height;

        //Create ring of verts around top
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = (edgeVert * 2) + originOfCurrentEdge; //Increase vertIndex by 2 each loop because number of vertices are doubled
            int nextVertIndex = vertIndex + 1;
            //Duplicate vertex from bottom ring and move upwards
            Vector3 newCorner = vertices[vertIndex] + (Vector3.up * height);
            Vector3 nextNewCorner = vertices[nextVertIndex] + (Vector3.up * height);
            //Lerp vertex inwards by topTaper
            newCorner = Vector3.Lerp(newCorner, vertices[0], topTaper);
            nextNewCorner = Vector3.Lerp(nextNewCorner, vertices[0], topTaper);
            vertices.Add(newCorner);
            vertices.Add(nextNewCorner);

            //Skip this step because sides of buildings should be replaced with handmade meshes
            /*
            */
        }
        originOfCurrentEdge += edges * 2; //Move to top edge of current quad, multiply by 2 because the edge loop is twice as long for the sides of the building

        //Duplicate current edge ring for edge of roof quads
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = (edgeVert * 2) + originOfCurrentEdge; //Increase vertIndex by 2 each loop because number of vertices are doubled 
            Vector3 newCorner = vertices[vertIndex];
            vertices.Add(newCorner);
        }
        originOfCurrentEdge += edges * 2; //Move to bottom edge of next quad, multiply by 2 because the edge loop is twice as long for the sides of the building

        //Create triangles linking ring 3 and 0
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = edgeVert + originOfCurrentEdge;

            int p1 = 0;
            int p2 = vertIndex;
            int p3 = edgeVert + 1 < edges ? p2 + 1 : originOfCurrentEdge;

            triangles.Add(p1);
            triangles.Add(p2);
            triangles.Add(p3);
        }

        //Fill sides of the building with randomly chosen replacement meshes, or quads if no replacements available
        for (int edgeVert = 0; edgeVert < edges; edgeVert++)
        {
            int vertIndex = (edgeVert * 2) + originOfSideEdge;

            int p1 = vertIndex;
            int p2 = p1 + 1;
            int p3 = vertIndex + (edges * 2);
            int p4 = p3 + 1;

            if (replacementMeshes != null && replacementMeshes.Length > 0)
            {
                //Pick a replacement mesh to use for this side of the building, and add its vertices and triangles to the mesh
                int meshIndex = Random.Range(0, replacementMeshes.Length);
                Mesh replacementMesh = replacementMeshes[meshIndex];

                Vector3 minExtents = replacementMesh.bounds.min;
                Vector3 maxExtents = replacementMesh.bounds.max;

                Vector3 v1 = vertices[p1];
                Vector3 v2 = vertices[p2];
                Vector3 v3 = vertices[p3];
                Vector3 v4 = vertices[p4];

                //Add triangles of replacement mesh, increasing the index by the current vertex list length
                int triangleIndexOffset = vertices.Count;
                for (int tri = 0; tri < replacementMesh.triangles.Length; tri++)
                {
                    triangles.Add(replacementMesh.triangles[tri] + triangleIndexOffset);
                }

                //Create a transformation matrix to convert points from the [x, z] plane to the plane of the face
                Vector3 midpointBottom = Vector3.Lerp(v1, v2, 0.5f);
                Vector3 midpointTop = Vector3.Lerp(v3, v4, 0.5f);
                float halfHeight = (midpointTop.y - midpointBottom.y) / 2f;

                float rotationY = (Mathf.Atan2(v1.x - v2.x, v1.z- v2.z) * Mathf.Rad2Deg) - 90f;
                //float rotationYTop = (Mathf.Atan2(v3.x - v4.x, v3.z - v4.z) * Mathf.Rad2Deg) - 90f;
                float relativeX = Mathf.Sqrt(Mathf.Pow(midpointTop.z - midpointBottom.z, 2) + Mathf.Pow(midpointTop.x - midpointBottom.x, 2));
                float rotationX = Mathf.Atan2(midpointTop.y - midpointBottom.y, relativeX) * Mathf.Rad2Deg * -1f;
                float rotationZ = 0f; //Mathf.Atan2(midpointTop.y - midpointBottom.y, midpointTop.x - midpointBottom.x) * Mathf.Rad2Deg;
                Vector3 transformRotation = new Vector3(rotationX, rotationY, rotationZ);
                //Vector3 topTransformRotation = new Vector3(rotationX, rotationYTop, rotationZ);

                //Need to find way to move positions so they rest on the surface
                float distanceToFace = halfHeight * Mathf.Tan((90f + rotationX) * Mathf.Deg2Rad);
                Vector3 towardsFace = Vector3.Cross(v1 - v2, Vector3.up);
                towardsFace.Normalize();
                //Debug.Log(halfHeight + " / tan(" + -rotationX + ") = " + distanceToFace);
                towardsFace *= distanceToFace;

                Vector3 bottomTransformPosition = midpointBottom + (Vector3.up * halfHeight) + towardsFace; //Move up and forward
                Vector3 topTransformPosition = midpointTop + (Vector3.up * -halfHeight) - towardsFace; //Move down and backward

                float meshWidth = maxExtents.x - minExtents.x;
                float sideWidth = Vector3.Distance(v1, v2);
                float relativeWidth = sideWidth / meshWidth;

                float topSideWidth = Vector3.Distance(v3, v4);
                float topRelativeWidth = topSideWidth / meshWidth;

                float meshHeight = maxExtents.z - minExtents.z;
                float sideHeight = Vector3.Distance(midpointBottom, midpointTop);
                float relativeHeight = sideHeight / meshHeight;

                float relativeDepth = Mathf.Min(relativeWidth, relativeHeight);

                Vector3 transformScale = new Vector3(relativeWidth, relativeDepth, relativeHeight);
                Vector3 topTransformScale = new Vector3(topRelativeWidth, relativeDepth, relativeHeight);

                MatrixTransform transformMatrix = new MatrixTransform(bottomTransformPosition, transformRotation, transformScale);
                MatrixTransform topTransformMatrix = new MatrixTransform(topTransformPosition, transformRotation, topTransformScale);

                //Use positions of defined points to lerp the vertex positions of a replacement mesh, like uv coordinates
                for (int vertex = 0; vertex < replacementMesh.vertices.Length; vertex++) //Redoing this to use a matrix transformation instead
                {
                    Vector3 oldVertex = replacementMesh.vertices[vertex];
                    Vector3 newVertexBottom = transformMatrix.TransformPoint(oldVertex);
                    Vector3 newVertexTop = topTransformMatrix.TransformPoint(oldVertex);

                    //Interpolate between top and bottom vertices by the height of the vertex
                    float interpolant = Mathf.InverseLerp(minExtents.z, maxExtents.z, oldVertex.z);
                    Vector3 newVertex = Vector3.LerpUnclamped(newVertexBottom, newVertexTop, interpolant);

                    vertices.Add(newVertex);
                }
            }
            else
            {
                //Fill with triangles instead
                triangles.Add(p1);
                triangles.Add(p2);
                triangles.Add(p3);

                triangles.Add(p4);
                triangles.Add(p3);
                triangles.Add(p2);
            }
        }

        //Flip if upsidedown is true
        if (upsidedown)
        {
            //Flip sign of y position
            for (int vert = 0; vert < vertices.Count; vert++)
            {
                Vector3 flipped = vertices[vert];
                flipped.y = -flipped.y;
                vertices[vert] = flipped;
            }

            //Reverse triangles so buildings are visable from outside
            for (int tri = 0; tri < triangles.Count; tri += 3)
            {
                int temp = triangles[tri + 1]; //Cache first vertex in triangle
                triangles[tri + 1] = triangles[tri + 2]; //Assign first vertex equal to second vertex
                triangles[tri + 2] = temp; //Assign second vertex to cached first vertex
            }
        }

        List<Color> vertexColors = new List<Color>();

        //Color vertices
        for (int vert = 0; vert < vertices.Count; vert++)
        {
            vertexColors.Add(color);
        }

        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetColors(vertexColors); //Why are you blue
        meshFilter.mesh.SetTriangles(triangles, 0);
        meshFilter.mesh.RecalculateNormals();
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
