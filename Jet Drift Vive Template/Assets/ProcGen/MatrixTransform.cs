using UnityEngine;
using System.Collections;

public class MatrixTransform {

    private Vector3 _Position;
    private Vector3 _Rotation;
    private Quaternion _QuaternionRotation;
    private Vector3 _Scale;

    private Matrix4x4 positionMatrix;
    private Matrix4x4 rotationMatrix;
    private Matrix4x4 scaleMatrix;

    private Matrix4x4 transformMatrix;

    public Vector3 Position
    {
        get
        {
            return _Position;
        }

        set
        {
            _Position = value;
        }
    }

    public Vector3 Rotation
    {
        get
        {
            return _Rotation;
        }

        set
        {
            _Rotation = value;
        }
    }

    public Vector3 Scale
    {
        get
        {
            return _Scale;
        }

        set
        {
            _Scale = value;
        }
    }

    public Quaternion QuaternionRotation
    {
        get
        {
            return _QuaternionRotation;
        }

        set
        {
            _QuaternionRotation = value;
        }
    }

    public MatrixTransform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        _Position = position;
        //SetPositionMatrix(position);

        _Scale = scale;
        //SetScaleMatrix(scale);

        //_QuaternionRotation = Quaternion.AngleAxis(angle, rotationAxis);
        _QuaternionRotation = Quaternion.Euler(rotation);
        _Rotation = rotation;
        //SetRotationMatrix(QuaternionRotation);

        SetTransformMatrix();

    }

    public Vector3 TransformPoint(Vector3 point)
    {
        Vector3 transformedPoint = point;
        //transformedPoint = scaleMatrix.MultiplyPoint(transformedPoint);
        //transformedPoint = rotationMatrix.MultiplyPoint(transformedPoint);
        //transformedPoint = positionMatrix.MultiplyPoint(transformedPoint);
        transformedPoint = transformMatrix.MultiplyPoint(transformedPoint);
        return transformedPoint;
    }

    #region Initilization

    private void SetPositionMatrix(Vector3 position)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(1f, 0f, 0f, position.x));
        matrix.SetRow(1, new Vector4(0f, 1f, 0f, position.y));
        matrix.SetRow(2, new Vector4(0f, 0f, 1f, position.z));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        positionMatrix = matrix;
    }

    private void SetScaleMatrix(Vector3 scale)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(scale.x, 0f, 0f, 0f));
        matrix.SetRow(1, new Vector4(0f, scale.y, 0f, 0f));
        matrix.SetRow(2, new Vector4(0f, 0f, scale.z, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        scaleMatrix = matrix;
    }

    //Redoing to use angle-axis form
    private void SetRotationMatrix(Quaternion rotation)
    {
        //old code for euler rotation
        //float radX = rotation.x * Mathf.Deg2Rad;
        //float radY = rotation.y * Mathf.Deg2Rad;
        //float radZ = rotation.z * Mathf.Deg2Rad;

        //float sinX = Mathf.Sin(radX);
        //float cosX = Mathf.Cos(radX);
        //float sinY = Mathf.Sin(radY);
        //float cosY = Mathf.Cos(radY);
        //float sinZ = Mathf.Sin(radZ);
        //float cosZ = Mathf.Cos(radZ);

        Matrix4x4 matrix = new Matrix4x4();

        Quaternion unrotated = Quaternion.AngleAxis(0f, Vector3.forward);
        rotation = unrotated * rotation;
        float w = rotation.w;
        float x = rotation.x;
        float y = rotation.y;
        float z = rotation.z;
        //Quaternion rotation matrix
        matrix.SetColumn(0, new Vector4(
            1f - (2f * y * y) - (2f * z * z),
            (2f * x * y) + (2f * w * z),
            (2f * x * z) - (2f * w * y),
            0f));
        matrix.SetColumn(1, new Vector4(
            (2f * x * y) - (2f * w * z),
            1f - (2f * x * x) - (2f * z * z),
            (2f * y * z) - (2f * w * x),
            0f));
        matrix.SetColumn(2, new Vector4(
            (2f * x * z) + (2f * w * y),
            (2f * y * z) + (2f * w * x),
            1f - (2f * x * x) - (2f * y * y),
            0f));
        matrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

        //Old code for euler rotation
        //matrix.SetColumn(0, new Vector4(
        //    cosY * cosZ,
        //    cosX * sinZ + sinX * sinY * cosZ,
        //    sinX * sinZ - cosX * sinY * cosZ, 
        //    0f));
        //matrix.SetColumn(1, new Vector4(
        //    -cosY * sinZ,
        //    cosX * cosZ - sinX * sinY * sinZ,
        //    sinX * cosZ + cosX * sinY * sinZ, 
        //    0f));
        //matrix.SetColumn(2, new Vector4(
        //    sinY,
        //    -sinX * cosY,
        //    cosX * cosY, 
        //    0f));
        //matrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

        rotationMatrix = matrix;
    }

    private void SetTransformMatrix()
    {
        //Why do it all myself
        //Matrix4x4 matrix = Matrix4x4.identity;
        //matrix = scaleMatrix * matrix;
        //matrix = rotationMatrix * matrix;
        //matrix = positionMatrix * matrix;

        //When matrix4x4 has this functionality built in
        Matrix4x4 matrix = Matrix4x4.TRS(Position, QuaternionRotation, Scale);
        transformMatrix = matrix;
    }

    #endregion
}
