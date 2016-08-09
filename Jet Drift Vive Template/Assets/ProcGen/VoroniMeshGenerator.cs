using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VoroniMeshGenerator {

    /// <summary>
    /// Needs to be setup to take a pre-existing list of points as delaunayVerts
    /// Takes a mesh, a size on the [x, z] plane, and an array of vertices of any length, then returns a mesh that represents the Voroni regions of those points.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="regions"></param>
    public static void GenerateVoroniMesh(Mesh original, float width, float height, int regions)
    {
        Vector3[] delaunayVerts;
        Vector3[] circumcenters;
        Vector3[] voroniVerts;
        int[] delaunayTris;
        int[] voroniTris;

        delaunayVerts = GeneratePointsList(width, height, regions);
        delaunayTris = CreateDelaunayTriangles(delaunayVerts, Vector3.zero, new Vector3(width, 0f, height));

        circumcenters = FindCircumcenters(delaunayVerts, delaunayTris);

        List<Vector3> voroniVertsList = new List<Vector3>();
        voroniVertsList.AddRange(delaunayVerts);
        voroniVertsList.AddRange(circumcenters);
        voroniVerts = voroniVertsList.ToArray();

        voroniTris = CreateVoroniTriangles(delaunayTris, ref voroniVerts, circumcenters.Length);

        //Shift verts to be centered on origin
        ShiftToCenter(voroniVerts, width, height);

        //Rotate verts 90 degrees around x-axis so the mesh is flat
        //RotateAroundXAxis(voroniVerts);

        original.Clear();

        original.name = "Voroni Regions";
        original.vertices = voroniVerts;
        original.triangles = voroniTris;
    }

    /// <summary>
    /// Takes a list of points to return as seperate voroni region meshes, as well as a list of control points to constrain the size of the created voroni regions
    /// Can also take a spacing value between 0 and 1, which adds a scaled space between regions
    /// </summary>
    /// <param name="points"></param>
    /// <param name="controlPoints"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="roadWidth"></param>
    /// <returns></returns>
    public static Mesh[] GenerateVoroniIslands(Vector3[] points, Vector3[] controlPoints, Vector3 bottomLeftCorner, Vector3 topRightCorner, float width, float height, float roadWidth, out Vector3[] origins)
    {
        Vector3[] delaunayVerts;
        Vector3[] circumcenters;
        Vector3[] voroniVerts;
        int[] delaunayTris;
        int[] voroniTris;

        List<Vector3> composedPoints = new List<Vector3>();
        composedPoints.AddRange(points);
        composedPoints.AddRange(controlPoints);
        delaunayVerts = composedPoints.ToArray();

        delaunayTris = CreateDelaunayTriangles(delaunayVerts, bottomLeftCorner, topRightCorner);

        circumcenters = FindCircumcenters(delaunayVerts, delaunayTris);

        List<Vector3> voroniVertsList = new List<Vector3>();
        voroniVertsList.AddRange(delaunayVerts);
        voroniVertsList.AddRange(circumcenters);
        voroniVerts = voroniVertsList.ToArray();

        voroniTris = CreateVoroniTriangles(delaunayTris, ref voroniVerts, circumcenters.Length, roadWidth);

        //Shift verts to be centered on origin
        ShiftToCenter(voroniVerts, width, height);

        //Split each group of connected triangles into a seperate mesh
        Vector3[] originsPassedToSplitMethod = new Vector3[0];
        Mesh[] splitMeshes = SplitVoroniIslands(voroniVerts, voroniTris, points.Length, out originsPassedToSplitMethod);
        origins = originsPassedToSplitMethod;
        return splitMeshes;
    }

    public static Vector3[] GeneratePointsList(float width, float height, int length)
    {
        Vector3[] pointsList = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            Vector3 position = new Vector3();
            position.x = Random.Range(0, width);
            position.y = 0;
            position.z = Random.Range(0, height);
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
        Vector3 shift = new Vector3(width / 2f, 0f, height / 2f);
        for (int i = 0; i < points.Length; i++)
        {
            points[i] -= shift;
        }
    }

    private static void RotateAroundXAxis(Vector3[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(points[i].x, 0f, points[i].y);
        }
    }

    #region Delaunay Triangle Generation

    /// <summary>
    /// Uses the Bowyer-Watson Algorithm to generate a Delaunay Triangulation, from which a Voroni Diagram can be derived
    /// </summary>
    private static int[] CreateDelaunayTriangles(Vector3[] vertList, Vector3 bottomLeftCorner, Vector3 topRightCorner)
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
        int vertsLength = vertices.Count;
        vertices.Add(bottomLeftCorner);
        vertices.Add(Vector3.right * topRightCorner.x * 2f);
        vertices.Add(Vector3.forward * topRightCorner.z * 2f);
        AddTriangle(triangles, vertsLength, vertsLength + 1, vertsLength + 2);

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
    /// Only works if the vertices are ordered counterclockwise
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
        float B = a.z - d.z;
        float C = (Mathf.Pow(a.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(a.z, 2) - Mathf.Pow(d.z, 2));
        float D = b.x - d.x;
        float E = b.z - d.z;
        float F = (Mathf.Pow(b.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(b.z, 2) - Mathf.Pow(d.z, 2));
        float G = c.x - d.x;
        float H = c.z - d.z;
        float I = (Mathf.Pow(c.x, 2) - Mathf.Pow(d.x, 2)) + (Mathf.Pow(c.z, 2) - Mathf.Pow(d.z, 2));
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
        if (p2xp3.y > 0f)
        {
            return true;
        }
        else
        {
            return false;
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
            float m1 = (p2.z - p1.z) / (p2.x - p1.x);
            float m2 = (p3.z - p1.z) / (p3.x - p1.x);
            //To find the slope of the perpendicular bisector of a side, take the negative of the inverse
            m1 = -1f / m1;
            m2 = -1f / m2;
            //Average the position of 2 vertices to find the midpoint
            Vector3 bisect1 = (p1 + p2) / 2f;
            Vector3 bisect2 = (p1 + p3) / 2f;
            //Find the y-axis intercept of the perpendicular bisectors
            float b1 = bisect1.z - (m1 * bisect1.x);
            float b2 = bisect2.z - (m2 * bisect2.x);

            //Find the value of x where the bisectors intersect
            float x = (b2 - b1) / (m1 - m2);

            //Find the value of y(x) for one of the bisectors
            float y = (m1 * x) + b1;

            //Create a vector from x and y
            Vector3 pos = new Vector3(x, 0f, y);

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
    private static int[] CreateVoroniTriangles(int[] delTris, ref Vector3[] vertices, int centersLength, float roadWidth = 0f)
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
                    splitVertices[v] = Vector3.Lerp(splitVertices[v], splitVertices[triangles[triIndex]], roadWidth);
                }
            }
        }

        vertices = splitVertices.ToArray();

        return triangles.ToArray();
    }

    #endregion

    #region Island Splitting

    /// <summary>
    /// Takes a triangle and vertex array created by CreateVoroniTriangles() and returns an array of meshes, one for each seperate island in the voroni triangulation
    /// Also takes an output origins list, to record the relative positions of the new mesh array to the original
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    /// <param name="numIslands"></param>
    /// <returns></returns>
    private static Mesh[] SplitVoroniIslands(Vector3[] vertices, int[] triangles, int numIslands, out Vector3[] origins)
    {
        Mesh[] islands = new Mesh[numIslands];
        origins = new Vector3[numIslands];

        //Loop through each vertex used to generate the original mesh, which are always at [0 to numIslands] in the vertex list
        for (int i = 0; i < numIslands; i++)
        {
            List<int> connectedTris = new List<int>(); //Elements of this list refer to indices in vertices
            List<int> remappedTris = new List<int>(); //Elements of this list refer to indices in connectedVertices

            //Get the offset of the new mesh
            Vector3 centerPoint = vertices[i];
            origins[i] = centerPoint;

            //Will be assigned to the maximum value within remappedTris, to set the length of the vertex list
            int maxVertexIndex = 0;

            //Keeps track of which vertex index to assign in remappedTris if the needed vertex index hasn't been used yet
            int firstUnusedIndex = 0;

            //Add all triangles that contain the center vertex i to a list
            for (int t = 0; t < triangles.Length; t += 3)
            {
                if (triangles[t] == i) //If the triangle contains the current islands center vertex index (triangles produced from the CreateVoroniTriangles() method always have the centerpoint as the first triangle vertex)
                {
                    int[] remappedTriangle = new int[3];
                    //Set each corner of remappedTriangle to be the index of the first occourance of triangles[t] in connectedTris, or to the number of unique indices in remappedTris if triangles[t] is not found
                    for (int remappedCorner = 0; remappedCorner < 3; remappedCorner++)
                    {
                        int vertexIndex;
                        if (connectedTris.Contains(triangles[t + remappedCorner]))
                        {
                            vertexIndex = remappedTris[connectedTris.FindIndex(x => x == triangles[t + remappedCorner])];
                        }
                        else
                        {
                            vertexIndex = firstUnusedIndex;
                            firstUnusedIndex++;
                        }
                        remappedTriangle[remappedCorner] = vertexIndex;

                        if (vertexIndex > maxVertexIndex)
                        {
                            maxVertexIndex = vertexIndex;
                        }
                    }

                    //Add the triangle to connectedTris
                    AddTriangle(connectedTris, triangles[t], triangles[t + 1], triangles[t + 2]);
                    //Add the remapped triangle to remappedTris
                    AddTriangle(remappedTris, remappedTriangle[0], remappedTriangle[1], remappedTriangle[2]);
                }
            }

            List<Vector3> connectedVertices = new List<Vector3>();

            //Initialize connectedVertices list
            for (int v = 0; v < maxVertexIndex + 1; v++)
            {
                connectedVertices.Add(Vector3.zero);
            }

            //Assign to connectedVertices, indexed by remappedTris, using the data from vertices, indexed by connectedTris
            for (int v = 0; v < connectedTris.Count; v++)
            {
                connectedVertices[remappedTris[v]] = vertices[connectedTris[v]];
            }

            //if (i == 0) //So unity doesn't crash when I get this right
            //{
            //    Debug.Log((maxVertexIndex + 1) + " vertices, " + (connectedTris.Count / 3) + " tris connected to original, " + (remappedTris.Count / 3) + " tris in new region");
            //    string connectedTrisString = "";
            //    string remappedTrisString = "";
            //    for (int t = 0; t < remappedTris.Count; t++)
            //    {
            //        connectedTrisString += connectedTris[t];
            //        connectedTrisString += ", ";
            //        remappedTrisString += remappedTris[t];
            //        remappedTrisString += ", ";
            //    }

            //    string connectedVertsString = "";
            //    for (int v = 0; v < connectedVertices.Count; v++)
            //    {
            //        connectedVertsString += connectedVertices[v];
            //        connectedVertsString += ", ";
            //    }

            //    Debug.Log(connectedVertsString);
            //    Debug.Log(connectedTrisString);
            //    Debug.Log(remappedTrisString);
            //}

            //Subtract centerPoint from each vertex so the center of the voroni region is the origin of the mesh
            for (int v = 0; v < connectedVertices.Count; v++)
            {
                connectedVertices[v] -= centerPoint;
            }

            //Rearrange vertices so they go in a clockwise order from index 1
            Vector3[] clockwiseVertices = new Vector3[connectedVertices.Count];
            clockwiseVertices[0] = connectedVertices[0]; //To keep the center in the same position
            
            int[] clockwiseVertexIndices = new int[connectedVertices.Count]; //clockwiseVertexIndices[v] stores the index of the vertex that should be ahead of connectedVertices[v]
            clockwiseVertexIndices[0] = 0; //To keep the center in the same place in the list

            //Find the order the existing vertices should be arranged in so that they flow counter-clockwise from index 1
            int nextConnectedIndex = 1;
            for (int v = 1; v < connectedVertices.Count; v++)
            {
                //Find the smallest angle between another vertex where the cross product between the v and vOther has a positive y component
                //First approach used distance instead, but would fail occasionally when the closest vertex was on the other side of the center and farther along the loop than the 'next' vertex
                float smallestVertexAngle = 360f;
                int smallestAngleIndex = -1;
                for (int vOther = 1; vOther < connectedVertices.Count; vOther++) //Foe each other vertex on the region boundary
                {
                    if (vOther != nextConnectedIndex) //If not the same vertex
                    {
                        if (nextConnectedIndex < 0)
                        {
                            Debug.Log("Next connected index was less than 0!");
                        }
                        Vector3 cross = Vector3.Cross(connectedVertices[nextConnectedIndex], connectedVertices[vOther]);
                        if (cross.y >= 0)
                        {
                            float angle = Vector3.Angle(connectedVertices[nextConnectedIndex], connectedVertices[vOther]);
                            if (angle < smallestVertexAngle)
                            {
                                smallestVertexAngle = angle;
                                smallestAngleIndex = vOther;
                            }
                        }
                    }
                }

                //If smallestAngleIndex has not been assigned to
                if (smallestAngleIndex == -1)
                {
                    //Origin of polygon must lie outside boundaries

                }

                clockwiseVertexIndices[v] = nextConnectedIndex;
                nextConnectedIndex = smallestAngleIndex;
            }
            
            //Assign to clockwiseVertices and clockwiseTriangles
            List<int> clockwiseTriangles = new List<int>();
            for (int v = 1; v < clockwiseVertexIndices.Length; v++)
            {
                clockwiseVertices[v] = connectedVertices[clockwiseVertexIndices[v]];

                //Add a new triangle for each outside vertex
                int v1 = 0;
                int v2 = v;
                int wrapVPlusOne = v + 1 < clockwiseVertexIndices.Length ? v + 1 : 1;
                int v3 = wrapVPlusOne;
                AddTriangle(clockwiseTriangles, v1, v2, v3);
            }

            //Create a new mesh from connectedVertices and connectedTris
            Mesh island = new Mesh();
            island.Clear();
            island.name = "Voroni Island";
            island.vertices = clockwiseVertices;
            island.triangles = clockwiseTriangles.ToArray();
            island.RecalculateNormals();

            islands[i] = island;
        }

        return islands;
    }

    #endregion
}
