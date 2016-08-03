using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class InsideCircumcircleTester : MonoBehaviour {

    public bool p4InsideCircumcircle;

    public Transform p1;
    public Transform p2;
    public Transform p3;
    public Transform p4;

    void Update()
    {
        p4InsideCircumcircle = PointInsideCircumcircle(p1.position, p2.position, p3.position, p4.position);
    }

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
}
