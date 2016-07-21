using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParabolicRaycaster : MonoBehaviour {
    
    public int segments = 10;
    public float time = 1;
    public float velocity;

    private const float g = -9.81f;

    private Vector3[] curvePoints;

    void Update()
    {
        curvePoints = ParabolicPointsList(Vector3.zero, transform.eulerAngles, time, velocity, segments);
        //RaycastHit hit;
        //if (ParabolicRaycast(out hit, transform.eulerAngles, time, velocity, segments))
        //{
        //    Debug.Log("Hit at " + hit.point);
        //}
    }

    public static Vector3[] ParabolicPointsList(Vector3 origin, Vector3 eulerAngles, float travelTime, float initialVelocity, int segmentCount = 10)
    {
        Vector3[] points = new Vector3[segmentCount + 1];

        float verticalVelocity = Mathf.Sin(-eulerAngles.x * Mathf.Deg2Rad) * initialVelocity;
        float horizontalVelocity = Mathf.Cos(-eulerAngles.x * Mathf.Deg2Rad) * initialVelocity;
        float xAxisVelocity = Mathf.Sin(eulerAngles.y * Mathf.Deg2Rad) * horizontalVelocity;
        float zAxisVelocity = Mathf.Cos(eulerAngles.y * Mathf.Deg2Rad) * horizontalVelocity;

        for (int i = 0; i < points.Length; i++)
        {
            float t = ((float)i / (points.Length - 1)) * travelTime;
            points[i] = new Vector3(t * xAxisVelocity, ((g / (2f)) * Mathf.Pow(t, 2)) + (verticalVelocity * t), t * zAxisVelocity) + origin;
        }

        return points;
    }

    /// <summary>
    /// Returns true at the first raycast that hits a collider. Sets hit.distance to the value of t at the first successful raycast.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="eulerAngles"></param>
    /// <param name="travelTime"></param>
    /// <param name="initialVelocity"></param>
    /// <param name="segmentCount"></param>
    /// <returns></returns>
    public bool ParabolicRaycast(out RaycastHit hit, Vector3 origin, Vector3 eulerAngles, float travelTime, float initialVelocity, int segmentCount = 10)
    {
        Vector3[] points = ParabolicPointsList(origin, eulerAngles, travelTime, initialVelocity, segmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = ((float)i / (points.Length - 1)) * travelTime; //Value to pass as travelTime to get an arc that ends just before the hit location
            Vector3 direction = points[i + 1] - points[i];
            float distance = Vector3.Distance(points[i], points[i + 1]);
            Ray segmentRay = new Ray(points[i], direction);

            if (Physics.Raycast(segmentRay, out hit, distance))
            {
                hit.distance = t;
                return true;
            }
        }

        hit = new RaycastHit();
        return false;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
            //Vector3 direction = points[i + 1] - points[i];
            //Gizmos.color = Color.green;
            //Ray segmentRay = new Ray(points[i], direction);
            //Gizmos.DrawRay(segmentRay);
        }
    }
}
