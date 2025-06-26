using StarterAssets;
//using System.Numerics;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

//using static UnityEditor.PlayerSettings;

public class SplineController : MonoBehaviour
{
    [SerializeField] SplineContainer currentSplineContainer_;
    [SerializeField] Transform camera_;
    [SerializeField] List<SplineContainer> splineContainers_ = new List<SplineContainer>();

    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;
    [SerializeField] GameObject followTarget_;
    [SerializeField] float t_;
    [SerializeField] float speed_;
    [SerializeField] float duration_;
    [SerializeField] float timer_;
    [SerializeField] float d_;
    
    [SerializeField] private int dir_;
    [SerializeField] private bool isMovingRight = false;
    [SerializeField] private bool isMovingLeft = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //currentSplineContainer_ = GetComponent<SplineContainer>();
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        UnityEngine.Vector3 pos = currentSplineContainer_.EvaluatePosition(0.0f);
        followTarget_.transform.position = pos;
        dir_ = 1;
        d_ = 1;
        
    }

    // Update is called once per frame
    void Update()
    {
        ///
        ///���L�[�������Ă��邩��Ƃ����āA�K�������Ȑ��Ƃ͋t�����ɐi��ł���Ƃ͌���Ȃ�
        ///
        int inputAxis = 0;
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            isMovingLeft =true;
            isMovingRight = false;
            inputAxis = -1;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            isMovingRight =true;
            isMovingLeft=false;
            inputAxis = 1;
        }
        
        if(inputAxis != 0)
        {
            float movementT = speed_ / currentSplineContainer_.CalculateLength();
            //t_ += (movementT * inputAxis);
            t_ += (movementT * inputAxis * dir_);
            MoveAlongSpline(t_);
        }

        if (t_ < 0.0f)
        {
            //t_ = 1.0f;
            Debug.Log("t<0.0f");
            MoveOtherSplineMinOrMax();
        }
        else if (t_ > 1.0f)
        {
            //t_ = 0.0f;
            Debug.Log("t > 1.0f");
            MoveOtherSplineMinOrMax();
        }

        if (thirdPersonController_ != null)
        {
            UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
            if(inputAxis != 0)
            {
                //moveInput.x = inputAxis * dir_;
                //moveInput.x = inputAxis * dir_;
                moveInput.x = inputAxis * d_;
            }
           
            thirdPersonController_.SetSplineMoveInput(moveInput, speed_);
        }
        if(cameraController_ != null)
        {
            if(dir_ ==-1)
            {
                cameraController_.isMovingLeft_ = isMovingLeft;
            }
            else
            {
                cameraController_.isMovingLeft_ = isMovingLeft;
            }
            
        }
       

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
        tangent *= dir_;
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);
        
        followTarget_.transform.rotation = rotation;
        followTarget_.transform.position = new UnityEngine.Vector3(nearestPos.x, followTarget_.transform.position.y, nearestPos.z);
    }
    private void MoveOtherSplineMinOrMax()
    {
        float t = 0.0f;
       if(t_ < 0.0f)
        {
            t = math.abs(t_);
        }
       else if(t_ > 1.0f)
       {
            t = t_ - 1.0f;
       }
        
        //RaycastHit hit;

        var ft = followTarget_.transform;
        //�v���C���[�̐��ʕ�����t_���͂ݏo���������L�΂��āA��������^����
        float move =   currentSplineContainer_.CalculateLength() * t;

        ft.position += move * ft.forward;

        MoveOtherSpline(ft.position + (move* ft.forward) , -ft.up);
    }
    private void MoveOtherSpline(Vector3 pos,Vector3 dir)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos, dir, out hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;


            SplineContainer nextContainer = hitObject.GetComponent<SplineContainer>();
            if (nextContainer == null)
            {
                return;
            }
            if (currentSplineContainer_ == nextContainer)
            {
                //t_ = Mathf.Clamp01(t_);
                return;
            }
            {
                //���݂̋Ȑ��̃^���W�F���g
                Spline currentSpline = currentSplineContainer_.Spline;
                NativeSpline currentNativeSpline = new NativeSpline(currentSpline, currentSplineContainer_.transform.localToWorldMatrix);
                float3 currPos, currTangent, currUp;
                SplineUtility.Evaluate<NativeSpline>(currentNativeSpline, t_, out currPos, out currTangent, out currUp);

                float3 outPos;
                float outT;
                Debug.Log("prevT:" + t_);
                //���̋Ȑ��ł�t
               

                NativeSpline nextNativeSpline = new NativeSpline(nextContainer.Spline, nextContainer.transform.localToWorldMatrix);
                float3 nextTangent, nextUp;
                SplineUtility.GetNearestPoint<NativeSpline>(nextNativeSpline, hit.point, out outPos, out outT);
                //���̋Ȑ��̃^���W�F���g
                SplineUtility.Evaluate<NativeSpline>(nextNativeSpline, outT, out outPos, out nextTangent, out nextUp);

                //���݂̂Ƌt������
                float dot = math.dot(math.normalize(currTangent), math.normalize(nextTangent));
                //��������
                if (dot > 0)
                {
                    //dir_ = 1;
                    Debug.Log("��������");
                }
                //�t����
                else if (dot < 0)
                {
                    dir_ *= -1;
                    Debug.Log("�t����");
                }
                //���p(�E������...�ǂ�����...!?)
                //�E�������ŕ�����
                else
                {
                    Debug.Log("���p");
                    float rotY = math.atan2(currTangent.x, currTangent.z);
                    UnityEngine.Quaternion rot = UnityEngine.Quaternion.Euler(0, rotY, 0);
                    UnityEngine.Matrix4x4 rotMat = UnityEngine.Matrix4x4.Rotate(rot);
                    float3 right = rotMat.MultiplyPoint3x4(new UnityEngine.Vector3(1, 0, 0));
                    if (math.dot(right, nextTangent) > 0)
                    {
                        dir_ = 1;
                        Debug.Log("�E");
                    }
                    else
                    {
                        dir_ = -1;
                        Debug.Log("��");
                    }

                }


                currentSplineContainer_ = nextContainer;
                t_ = outT;
                Debug.Log("currT:" + t_);
            }

        }
        else
        {
            Debug.Log("Raycast == false");
            t_ = Mathf.Clamp01(t_);
        }
    }
    
    public void MoveOtherSpline()
    {
        MoveOtherSpline(followTarget_.transform.position,-followTarget_.transform.up);
#if false
        RaycastHit hit;
        
        //var ft = followTarget_.transform;
        var ft = followTarget_.transform;
        if(Physics.Raycast(ft.position,-ft.up,out hit,Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            
            SplineContainer nextContainer =  hitObject.GetComponent<SplineContainer>();
            if(nextContainer == null)
            {
                return;
            }
            if(currentSplineContainer_ == nextContainer)
            {
                //t_ = Mathf.Clamp01(t_);
                return;
            }
            {
                //���݂̋Ȑ��̃^���W�F���g
                Spline currentSpline = currentSplineContainer_.Spline;
                NativeSpline currentNativeSpline = new NativeSpline(currentSpline, currentSplineContainer_.transform.localToWorldMatrix);
                float3 currPos, currTangent, currUp;
                SplineUtility.Evaluate<NativeSpline>(currentNativeSpline,t_,out currPos,out currTangent,out currUp);

                float3 outPos;
                float outT;
                Debug.Log("prevT:" + t_);
                //���̋Ȑ��ł�t
                //SplineUtility.GetNearestPoint<Spline>(nextContainer.Spline, ft.position, out outPos, out outT);

                NativeSpline nextNativeSpline = new NativeSpline(nextContainer.Spline, nextContainer.transform.localToWorldMatrix);
                float3 nextTangent, nextUp;
                SplineUtility.GetNearestPoint<NativeSpline>(nextNativeSpline, hit.point, out outPos, out outT);
                //���̋Ȑ��̃^���W�F���g
                SplineUtility.Evaluate<NativeSpline>(nextNativeSpline, outT,out outPos, out nextTangent, out nextUp);

                //���݂̂Ƌt������
                float dot = math.dot(math.normalize(currTangent), math.normalize(nextTangent));
                //��������
                if(dot > 0)
                {
                    //dir_ = 1;
                    Debug.Log("��������");
                }
                //�t����
                else if(dot < 0)
                {
                    dir_ *= -1;
                    Debug.Log("�t����");
                }
                //���p(�E������...�ǂ�����...!?)
                //�E�������ŕ�����
                else
                {
                    Debug.Log("���p");
                    float rotY = math.atan2(currTangent.x,currTangent.z);
                    UnityEngine.Quaternion rot = UnityEngine.Quaternion.Euler(0, rotY, 0);
                    UnityEngine.Matrix4x4 rotMat = UnityEngine.Matrix4x4.Rotate(rot);
                    float3 right = rotMat.MultiplyPoint3x4(new UnityEngine.Vector3(1,0,0));
                    if(math.dot(right,nextTangent) > 0 )
                    {
                        dir_ = 1;
                        Debug.Log("�E");
                    }
                    else 
                    {
                        dir_ = -1;
                        Debug.Log("��");
                    }

                }


                currentSplineContainer_ = nextContainer;
                t_ = outT;
                Debug.Log("currT:"+t_);
            }
            
        }
        else
        {
            Debug.Log("t = Mathf.Clamp01(t)");
            t_ = Mathf.Clamp01(t_);
        }
#endif
    }

}
