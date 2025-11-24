using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RotateAroundObject : MonoBehaviour
{
    public List<GameObject> targetObjects; // 回転させたいオブジェクトのリスト
    public GameObject pivotObject;   // 回転の中心となるオブジェクト

    [SerializeField]
    private float rotationSpeed = 30f; // 1秒あたりの回転速度 (度数)

    public Vector3 rotationAxis = Vector3.up;
    [SerializeField] bool rotateLocal = false;
    void Update()
    {
        if (pivotObject != null && targetObjects != null)
        {
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    Vector3 axis = rotationAxis;
                    if(rotateLocal)
                    {
                        axis = pivotObject.transform.TransformDirection(rotationAxis);
                    }
                    obj.transform.RotateAround(pivotObject.transform.position, axis, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}