using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VoroniMeshGenerator {

    public static void GenerateVoroniMesh(Mesh original, float width, float height, int regions)
    {
        Vector3[] delaunayVerts;
        Vector3[] circumcenters;
        Vector3[] voroniVerts;
        int[] delaunayTris;
        int[] voroniTris;

        delaunayVerts = PlacePoints(width, height, regions);
        delaunayTris = CreateDelaunayTriangles(delaunayVerts, width, height);

        circumcenters = FindCircumcenters(delaunayVerts, delaunayTris);

        List<Vector3> voroniVertsList = new List<Vector3>();
        voroniVertsList.AddRange(delaunayVerts);
        voroniVertsList.AddRange(circumcenters);
        voroniVerts = voroniVertsList.ToArray();

        voroniTris = CreateVoroniTriangles(delaunayTris, ref voroniVerts, circumcenters.Length);

        //Shift verts to be centered on origin
        ShiftToCenter(voroniVerts, width, height);

        original.Clear();

        original.name = "Voroni Regions";
        original.vertices = voroniVerts;
        original.triangles = voroniTris;
    }

    private static Vector3[] PlacePoints(float width, float height, int length)
    {
        Vector3[] pointsList = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            Vector3 position = new Vector3();
            position.x = Random.Range(0, width);
            position.y = Random.Range(0, height);
            position.z = 0;
            pointsList[i] = position;
        }

        return pointsList;
    }

    /// <summary>
    /// Takes 3 vertex indices and adds them to a given triangle list, checking for duplicates and returning the index of the new or existing triangle
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    private static int AddTriangle(List<int> triangles, int v1, int v2, int v3)
    {
        for (int t = 0; t < triangles.Count; t += 3)
        {
            if (triangles[t] == v1 && triangles[t + 1] == v2 && triangles[t + 2] == v3)
            {
                return t;
            }
        }
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
        return triangles.Count - 3;
    }

    private static void ShiftToCenter(Vector3[] points, float width, float height)
    {
        Vector3 shift = new Vector3(width / 2f, height / 2f);
        for (int i = 0; i < points.Length; i++)
        {
            points[i] -= shift;
        }
    }

    #region Delaunay Triangle Generation

    /// <summary>
    /// Uses the Bowyer-Watson Algorithm to generate a Delaunay Triangulation, from which a Voroni Diagram can be derived
    /// </summary>
    private static int[] CreateDelaunayTriangles(Vector3[] vertList, float width, float height)
    {
        if (vertList.Length < 4)
        {
            Debug.LogError("Vertices list too short to generate triangles!");
            return null;
        }
        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(vertList);
        List<int> triangles = new List<int>();

        //Generate supertriangle to enclose all points
        int l = vertices.Count;
        vertices.Add(Vector3.zero);
        vertices.Add(Vector3.right * width * 2f);
        vertices.Add(Vector3.up * height * 2f);
        AddTriangle(triangles, l, l + 1, l + 2);

        //For each point
        for (int i = 0; i < vertList.Length; i++)
        {
            //Debug.Log("Placing new point...");
            List<int> badTriangles = new List<int>();
            List<int> trianglesToRemove = new List<int>();

            //For each triangle, determine if it is no longer valid
            for (int t = 0; t < triangles.Count; t += 3)
            {
                //Debug.Log("Checking if point " + i + " is inside circumcircle of triangle " + t);
                if (PointInsideCircumcircle(vertices[triangles[t]], vertices[triangles[t + 1]], vertices[triangles[t + 2]], vertices[i]))
                {
                    AddTriangle(badTriangles, triangles[t], triangles[t + 1], triangles[t + 2]);
                    AddTriangle(trianglesToRemove, t, t + 1, t + 2);
                    //Debug.Log("Point inside circumcircle, removing triangle #" + t);
                }
            }

            List<int> polygon = new List<int>(); //List of edges making up the boundary around the newest point, which new triangles are formed from

            //Find the outside edges of polygon by removing shared edges of component triangles
            for (int t = 0; t < badTriangles.Count; t += 3)
            {
                //Debug.Log("Edges in polygonal hole: ");
                if (SharesEdge(badTriangles, badTriangles[t], badTriangles[t + 1]) == false)
                {
                    polygon.Add(badTriangles[t]);
                    polygon.Add(badTriangles[t + 1]);
                    //Debug.Log(badTriangles[t] + ", " + badTriangles[t + 1]);
                }
                if (SharesEdge(badTriangles, badTriangles[t], badTriangles[t + 2]) == false)
                {
                    polygon.Add(badTriangles[t]);
                    polygon.Add(badTriangles[t + 2]);
                    //Debug.Log(badTriangles[t] + ", " + badTriangles[t + 2]);
                }
                if (SharesEdge(badTriangles, badTriangles[t + 1], badTriangles[t + 2]) == false)
                {
                    polygon.Add(badTriangles[t + 1]);
                    polygon.Add(badTriangles[t + 2]);
                    //Debug.Log(badTriangles[t + 1] + ", " + badTriangles[t + 2]);
                }
            }

            //Remove invalid triangles from triangles list
            for (int t = 0; t < trianglesToRemove.Count; t++)
            {
                triangles[trianglesToRemove[t]] = -1;
            }
            for (int t = 0; t < trianglesToRemove.Count; t++)
            {
                triangles.Remove(-1);
            }

            //Debug.Log(polygon.Count);

            //Create a triangle between the newly added point and each edge of polygon
            for (int e = 0; e < polygon.Count; e += 2)
            {
                int p1 = i;
                int p2 = polygon[e];
                int p3 = polygon[e + 1];
                if (FlipForCounterClockwiseOrder(vertices[p1], vertices[p2], vertices[p3]))
                {
                    AddTriangle(triangles, p1, p3, p2);
                }
                else
                {
                    AddTriangle(triangles, p1, p2, p3);
                }
                //Debug.Log(string.Format("Creating triangle between {0}, {1}, {2}", p1, p2, p3));
            }
        }

        //Remove triangles that were part of the bounding supertriangle
        EliminateEdgeTriangles(triangles, vertList.Length);

        //Return triangle list
        return triangles.ToArray();
    }

    /// <summary>
    /// Returns true if the point 'd' is within the circumcircle formed by the triangle [a,b,c]
    /// Uses the determanent formula from wikipedia.org/wiki/Delaunay_triangulation
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    private static bool PointInsideCircumcircle(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        float A = a.x - d.x;
        float B = a.y - d.y;
        float C = (Mathf.Pow(a.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(a.y, 2) - Mathf.Pow(d.y, 2));
        float D = b.x - d.x;
        float E = b.y - d.y;
        float F = (Mathf.Pow(b.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(b.y, 2) - Mathf.Pow(d.y, 2));
        float G = c.x - d.x;
        float H = c.y - d.y;
        float I = (Mathf.Pow(c.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(c.y, 2) - Mathf.Pow(d.y, 2));
        float determinant = ((A * E * I) + (B * F * G) + (C * D * H)) - ((C * E * G) + (B * D * I) + (A * F * H));

        //Debug.Log(determinant);

        if (determinant > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if the edge given by [v1, v2] is shared with another triangle in the list t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    private static bool SharesEdge(List<int> t, int v1, int v2)
    {
        bool foundFirstInstance = false;
        for (int i = 0; i < t.Count; i += 3)
        {
            if ((t[i] == v1 || t[i + 1] == v1 || t[i + 2] == v1) && (t[i] == v2 || t[i + 1] == v2 || t[i + 2] == v2))
            {
                if (foundFirstInstance == true)
                {
                    return true;
                }
                foundFirstInstance = true;
            }
        }
        return false;
    }

    /// <summary>
    /// Starting from p1, returns false if p2 should be the next point on a counter-clockwise triangle, or true if p3 should be next.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    private static bool FlipForCounterClockwiseOrder(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 p2xp3 = Vector3.Cross(p2 - p1, p3 - p1);
        if (p2xp3.z > 0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private static void EliminateEdgeTriangles(List<int> triangles, int vertsLength)
    {
        int vertsToRemove = 0; //Counter for vertices to remove after checking is done
        for (int i = 0; i < triangles.Count; i += 3) //Loop through each triangle
        {
            for (int v = 0; v < 3; v++) //Loop through each of the 3 vertices
            {
                if (triangles[i + v] == vertsLength || triangles[i + v] == vertsLength + 1 || triangles[i + v] == vertsLength + 2) //If any vertex is part of the outside triangle
                {
                    //Flag the entire triangle for removal
                    triangles[i] = -1;
                    triangles[i + 1] = -1;
                    triangles[i + 2] = -1;
                    vertsToRemove += 3;
                    v = 3; //End the vertex loop
                }
            }
        }

        for (int i = 0; i < vertsToRemove; i++)
        {
            triangles.Remove(-1);
        }
    }

    #endregion

    #region Voroni Triangle Generation

    private static Vector3[] FindCircumcenters(Vector3[] vertices, int[] triangles)
    {
        Vector3[] centers = new Vector3[triangles.Length / 3];

        for (int i = 0, t = 0; i < centers.Length; i++, t += 3)
        {
            //Get reference to triangle vertices
            Vector3 p1 = vertices[triangles[t]];
            Vector3 p2 = vertices[triangles[t + 1]];
            Vector3 p3 = vertices[triangles[t + 2]];

            //Find slope of 2 of the triangle edges
            float m1 = (p2.y - p1.y) / (p2.x - p1.x);
            float m2 = (p3.y - p1.y) / (p3.x - p1.x);
            //To find the slope of the perpendicular bisector of a side, take the negative of the inverse
            m1 = -1f / m1;
            m2 = -1f / m2;
            //Average the position of 2 vertices to find the midpoint
            Vector3 bisect1 = (p1 + p2) / 2f;
            Vector3 bisect2 = (p1 + p3) / 2f;
            //Find the y-axis intercept of the perpendicular bisectors
            float b1 = bisect1.y - (m1 * bisect1.x);
            float b2 = bisect2.y - (m2 * bisect2.x);

            //Find the value of x where the bisectors intersect
            float x = (b2 - b1) / (m1 - m2);

            //Find the value of y(x) for one of the bisectors
            float y = (m1 * x) + b1;

            //Create a vector from x and y
            Vector3 pos = new Vector3(x, y, 0f);

            //Assign to the centers list
            centers[i] = pos;
        }

        return centers;
    }

    /// <summary>
    /// Takes list of circumcenters and their 3 bounding vertices, and forms a triangle with every vertex shared with another circumcenter.
    /// </summary>
    /// <param name="delTris">Array of input triangles connecting original vertices</param>
    /// <param name="vertices">Array of original vertices with circumcenters appended to the end</param>
    /// <param name="centersLength">The number of points at the end of the vertices list which are circumcenters</param>
    /// <returns></returns>
    private static int[] CreateVoroniTriangles(int[] delTris, ref Vector3[] vertices, int centersLength)
    {
        List<int> triangles = new List<int>();

        int[,] circumcenterConnectedTriangles = new int[centersLength, 6];

        //Fill circumcenterConnectedTriangles with -1
        for (int i = 0; i < centersLength; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                circumcenterConnectedTriangles[i, j] = -1;
            }
        }

        int centersOffset = vertices.Length - centersLength;

        //Loop through each circumcenter and the associated triangle
        for (int c = 0, t = 0; c < centersLength; c++, t += 3)
        {
            int p1 = delTris[t];
            int p2 = delTris[t + 1];
            int p3 = delTris[t + 2];

            int center1 = c + centersOffset;

            //Check each other triangle+circumcenter set to see which ones share at least 2 vertices with triangle [t, t+1, t+2]
            for (int c2 = 0, t2 = 0; c2 < centersLength; c2++, t2 += 3)
            {
                //If c2 is not the same circumcenter as c
                if (c != c2)
                {
                    int q1 = delTris[t2];
                    int q2 = delTris[t2 + 1];
                    int q3 = delTris[t2 + 2];

                    int center2 = c2 + centersOffset;

                    //If at least 2 points in the triangles are the same
                    if (p1 == q1 || p1 == q2 || p1 == q3)
                    {
                        if (p2 == q1 || p2 == q2 || p2 == q3)
                        {
                            //create 2 new triangles and add them to the connected triangle list for the current circumcenter
                            circumcenterConnectedTriangles[c, 1] = AddTriangle(triangles, p1, center1, center2);
                            circumcenterConnectedTriangles[c, 2] = AddTriangle(triangles, p2, center2, center1);
                        }
                        else if (p3 == q1 || p3 == q2 || p3 == q3)
                        {
                            circumcenterConnectedTriangles[c, 5] = AddTriangle(triangles, p3, center1, center2);
                            circumcenterConnectedTriangles[c, 0] = AddTriangle(triangles, p1, center2, center1);
                        }
                    }
                    else if (p2 == q1 || p2 == q2 || p2 == q3)
                    {
                        if (p3 == q1 || p3 == q2 || p3 == q3)
                        {
                            circumcenterConnectedTriangles[c, 3] = AddTriangle(triangles, p2, center1, center2);
                            circumcenterConnectedTriangles[c, 4] = AddTriangle(triangles, p3, center2, center1);
                        }
                    }
                }
            }
        }

        List<Vector3> splitVertices = new List<Vector3>();
        splitVertices.AddRange(vertices);

        for (int c = centersOffset, t = 0; c < vertices.Length; c++, t++) //Loop through each circumcenter
        {
            for (int i = 0; i < 3; i++) //Split it 3 times
            {
                int v = splitVertices.Count; //Index of newly split vertex
                splitVertices.Add(vertices[c]); //Place new vertex at position of circumcenter

                int triIndex = circumcenterConnectedTriangles[t, i * 2]; //Get the first triangle that should be connected to the new vertex

                if (triIndex > -1) //If the connected triangle exists
                {
                    for (int corner = 0; corner < 3; corner++) //Loop through each corner of the triangle to find the corner to replace
                    {
                        if (triangles[triIndex + corner] == c)
                        {
                            triangles[triIndex + corner] = v;
                        }
                    }
                }

                triIndex = circumcenterConnectedTriangles[t, (i * 2) + 1]; //Get the second triangle that should be connected to the new vertex

                if (triIndex > -1)
                {
                    for (int corner = 0; corner < 3; corner++) //Loop through each corner of the triangle to find the corner to replace
                    {
                        if (triangles[triIndex + corner] == c)
                        {
                            triangles[triIndex + corner] = v;
                        }
                    }

                    //Test: slide vertex 10% of the way towards the center of the region
                    splitVertices[v] = Vector3.Lerp(splitVertices[v], splitVertices[triangles[triIndex]], 0.1f);
                }
            }
        }

        vertices = splitVertices.ToArray();

        return triangles.ToArray();
    }

    #endregion
}
