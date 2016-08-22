using UnityEngine;
using System.Collections;

public class MatrixTester : MonoBehaviour {

    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public int length, width, height;

    public GameObject prefab;

    private GameObject[] points;
    private Vector3[] positions;
    private MatrixTransform matrix;

    void Start()
    {
        points = new GameObject[length * width * height];
        positions = new Vector3[points.Length];
        int i = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    positions[i] = new Vector3(x, y, z);
                    points[i] = Instantiate(prefab);
                    points[i].transform.position = positions[i];
                    i++;
                }
            }
        }
    }

    void Update()
    {
        matrix = new MatrixTransform(position, rotation, scale);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 transformedPosition = matrix.TransformPoint(positions[i]);
            points[i].transform.position = transformedPosition;
        }
    }
}
