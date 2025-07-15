using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RotateAroundObject : MonoBehaviour
{
    public List<GameObject> targetObjects; // ��]���������I�u�W�F�N�g�̃��X�g
    public GameObject pivotObject;   // ��]�̒��S�ƂȂ�I�u�W�F�N�g

    [SerializeField]
    private float rotationSpeed = 30f; // 1�b������̉�]���x (�x��)

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