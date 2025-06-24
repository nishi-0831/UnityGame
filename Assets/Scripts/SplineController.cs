using StarterAssets;
using System.Numerics;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

//using static UnityEditor.PlayerSettings;

public class SplineController : MonoBehaviour
{
    [SerializeField] SplineContainer currentSplineContainer_;
    [SerializeField] List<SplineContainer> splineContainers_ = new List<SplineContainer>();

    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;
    [SerializeField] GameObject followTarget_;
    [SerializeField] float t_;
    [SerializeField] float speed_;
    [SerializeField] float duration_;
    [SerializeField] float timer_;

    [SerializeField] private bool isMovingRight = false;
    [SerializeField] private bool isMovingLeft = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSplineContainer_ = GetComponent<SplineContainer>();
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        UnityEngine.Vector3 pos = currentSplineContainer_.EvaluatePosition(0.0f);
        followTarget_.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        int dir = 0;
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            isMovingLeft =true;
            isMovingRight = false;
            dir = -1;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            isMovingRight =true;
            isMovingLeft=false;
            dir = 1;
        }
        
        if(dir != 0)
        {
            float movementT = speed_ / currentSplineContainer_.CalculateLength();
            t_ += (movementT * dir);
            MoveAlongSpline(t_);
        }

        if (t_ < 0.0f)
        {
            //t_ = 1.0f;
            MoveOtherSpline(ref t_);
        }
        else if (t_ > 1.0f)
        {
            t_ = 0.0f;
            MoveOtherSpline(ref t_);
        }

        if (thirdPersonController_ != null)
        {
            UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
            if(dir != 0)
            {
                moveInput.x = dir;
            }
           
            thirdPersonController_.SetSplineMoveInput(moveInput, speed_);
        }
        if(cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = isMovingLeft;
        }
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Rigidbody rigidbody = followTarget_.GetComponent<Rigidbody>();
        //    if (rigidbody != null)
        //    {
        //        UnityEngine.Vector3 forceDir = new UnityEngine.Vector3(0, 1, 0);
        //        float force = 10.0f;
        //        rigidbody.AddForce(forceDir * force, ForceMode.Impulse);
        //    }
        //}

    }

    void MoveAlongSpline(float t)
    {
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        if (isMovingLeft) tangent *= -1;
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);
        
        followTarget_.transform.rotation = rotation;
        followTarget_.transform.position = new UnityEngine.Vector3(nearestPos.x, followTarget_.transform.position.y, nearestPos.z);
    }
    void MoveOtherSpline(ref float t)
    {
        RaycastHit hit;
        
        var ft = followTarget_.transform;
        if(Physics.Raycast(ft.position,-ft.up,out hit,Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log(hitObject.name);

            SplineContainer nextContainer =  hitObject.GetComponent<SplineContainer>();
            if (nextContainer != null)
            {
                currentSplineContainer_ = nextContainer;
            }
            float3 outPos;
            float outT;
            SplineUtility.GetNearestPoint<Spline>(currentSplineContainer_.Spline, ft.position, out outPos, out outT);
            t = outT;
        }
        else
        {
            t = Mathf.Clamp01(t);
        }
    }

}
