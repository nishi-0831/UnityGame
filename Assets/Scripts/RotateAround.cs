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

    void Update()
    {
        if (pivotObject != null && targetObjects != null)
        {
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    obj.transform.RotateAround(pivotObject.transform.position, rotationAxis, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}