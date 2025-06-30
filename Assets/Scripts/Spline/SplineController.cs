using StarterAssets;
using System;

//using System.Numerics;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SplineController : MonoBehaviour
{
    [SerializeField] public GameObject followTarget_;
    [SerializeField] public SplineContainer currentSplineContainer_;
    [SerializeField] public float t_;
    [SerializeField] public int splineDirection_ = 1;
    [SerializeField] public bool isMovingLeft = false;

    [Header("エディターで初期位置表示")]
    [SerializeField] private bool enableEditorPreview = true;
    [SerializeField]
    [Range(0f, 1f)]
    private float firstT_ = 0.0f;


    public Action onMaxT;
    public Action onMinT;

    private float prevT_;
    private bool isFirstFrame_ = true;
    
    
    // followTarget_のプロパティアクセサを追加
    public GameObject FollowTarget 
    { 
        get { return followTarget_; } 
        set { followTarget_ = value; } 
    }

    public float FirstT
    {
        get { return firstT_; }
        set 
        {
            firstT_ = value;
            //t_ = 
#if UNITY_EDITOR
            if(enableEditorPreview && !Application.isPlaying)
            {
                UpdateEditorPreview();
            }
#endif
        }
    }

#if UNITY_EDITOR    
    //Edit Mode/Play Mode問わず呼びだされる、インスペクターのプロパティが変更されたときに呼び出されるメソッド
    private void OnValidate()
    {
        if(enableEditorPreview && !Application.isPlaying)
        {
            //エディタでインスペクターの変更が完了したときに呼ばれるよう設定
            UnityEditor.EditorApplication.delayCall += UpdateEditorPreview;
        }
    }
    private void UpdateEditorPreview()
    {
        //Debug.Assert(followTarget_ != null,"followTarget == null");
        if(followTarget_ == null)
        {
            followTarget_ = this.gameObject;
            Debug.LogWarning("followTargetが見当たらなかったので、thisを対象とします");
        }
        if(currentSplineContainer_ == null)
        {
            Debug.LogError($"{this.gameObject.name}.currentSplineContainer == null");
            return;
        }
        
        if(enableEditorPreview)
        {
            MoveAlongSplineEditorOnly(firstT_);
        }
    }

    private void MoveAlongSplineEditorOnly(float t)
    {
        followTarget_.transform.position = GetSplinePos(t);
        if(!Application.isPlaying)
        {
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        t_ = firstT_;
        if (followTarget_ != null && currentSplineContainer_ != null)
        {
            //UnityEngine.Vector3 pos = currentSplineContainer_.EvaluatePosition(t_);
            //followTarget_.transform.position = pos;
            MoveAlongSpline(t_);
        }
        prevT_ = t_;
        splineDirection_ = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(t_ < 0)
        {
            onMinT?.Invoke();
        }
        else if(t_ > 1.0f)
        {
            onMaxT?.Invoke();
        }
    }

    public void UpdateT(float speed, int moveDir)
    {
        //Math.Clamp(moveDir, -1, 1);
        if (currentSplineContainer_ == null) return;
        
        float movementT = speed / currentSplineContainer_.CalculateLength();

        prevT_ = t_;
        t_ += (movementT * moveDir * splineDirection_);
    }
    public void Move(float speed, int moveDir)
    {
        UpdateT(speed, moveDir);
        MoveAlongSpline(t_);
    }
    public void Move(float speed)
    {
        int dir = 1;
        if(isMovingLeft)
        {
            dir = -1;
        }
        
        Move(speed, dir);
    }
    public void Reverse()
    {
        ClampT();
        if (isMovingLeft)
            isMovingLeft = false;
        else
            isMovingLeft = true;
    }
    /// <summary>
    /// Spline上においてmovementが占める割合を取得
    /// </summary>
    /// <returns></returns>
    public float GetSplineMovementT(float movement)
    {

        return movement / currentSplineContainer_.CalculateLength();
    }
    /// <summary>
    /// Spline上の移動による位置の変化量を取得
    /// </summary>
    /// <returns>前フレームからの移動量</returns>
    public Vector3 GetSplineMovementDelta()
    {
        if (isFirstFrame_)
        {
            isFirstFrame_ = false;
            return Vector3.zero;
        }

        Vector3 currentPosition = GetSplinePos();
        Vector3 delta = currentPosition - GetSplinePos(prevT_);
        
        //// Y軸の移動量は除外（重力処理はThirdPersonControllerで行うため）
        //delta.y = 0;
        
        return delta;
    }

    /// <summary>
    /// Splineの接線方向を取得
    /// </summary>
    /// <returns>正規化された接線ベクトル</returns>
    public Vector3 GetSplineTangent()
    {
        return GetSplineTangent(t_);
    }
    public Vector3 GetSplineTangent(float t)
    {
        if (currentSplineContainer_ == null) return Vector3.forward;
        
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);

        if (isMovingLeft) tangent *= -1;
        tangent *= splineDirection_;

        return math.normalize(tangent);
    }
    public Quaternion GetSplineRot()
    {
        return GetSplineRot(t_);
    }
    public Quaternion GetSplineRot(float t)
    {
        if (currentSplineContainer_ == null) return Quaternion.identity;
        
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        if (isMovingLeft) tangent *= -1;
        tangent *= splineDirection_;
        return  UnityEngine.Quaternion.LookRotation(tangent, upVector);
    }
    public Vector3 GetSplinePos(float t)
    {
        if (currentSplineContainer_ == null) return Vector3.zero;
        
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        return nearestPos;
    }
    public Vector3 GetSplinePos()
    {
        return GetSplinePos(t_);
    }
    public void MoveAlongSpline(float t)
    {
        if (currentSplineContainer_ == null || followTarget_ == null) return;
        
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        if (isMovingLeft) tangent *= -1;
        tangent *= splineDirection_;
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);
        
        followTarget_.transform.rotation = rotation;
        followTarget_.transform.position = new UnityEngine.Vector3(nearestPos.x, followTarget_.transform.position.y, nearestPos.z);
    }

    
    public void ClampT()
    {
        t_ = Mathf.Clamp01(t_);
    }
    public void MoveOtherSplineMinOrMax()
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
        
        var ft = followTarget_.transform;
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
                return;
            }
            {
                Spline currentSpline = currentSplineContainer_.Spline;
                NativeSpline currentNativeSpline = new NativeSpline(currentSpline, currentSplineContainer_.transform.localToWorldMatrix);
                float3 currPos, currTangent, currUp;
                SplineUtility.Evaluate<NativeSpline>(currentNativeSpline, t_, out currPos, out currTangent, out currUp);

                float3 outPos;
                float outT;
                Debug.Log("prevT:" + t_);

                NativeSpline nextNativeSpline = new NativeSpline(nextContainer.Spline, nextContainer.transform.localToWorldMatrix);
                float3 nextTangent, nextUp;
                SplineUtility.GetNearestPoint<NativeSpline>(nextNativeSpline, hit.point, out outPos, out outT);
                SplineUtility.Evaluate<NativeSpline>(nextNativeSpline, outT, out outPos, out nextTangent, out nextUp);

                float dot = math.dot(math.normalize(currTangent), math.normalize(nextTangent));
                if (dot > 0)
                {
                    Debug.Log("同じ向き");
                }
                else if (dot < 0)
                {
                    splineDirection_ *= -1;
                    Debug.Log("逆向き");
                }
                else
                {
                    Debug.Log("直角");
                    float rotY = math.atan2(currTangent.x, currTangent.z);
                    UnityEngine.Quaternion rot = UnityEngine.Quaternion.Euler(0, rotY, 0);
                    UnityEngine.Matrix4x4 rotMat = UnityEngine.Matrix4x4.Rotate(rot);
                    float3 right = rotMat.MultiplyPoint3x4(new UnityEngine.Vector3(1, 0, 0));
                    if (math.dot(right, nextTangent) > 0)
                    {
                        splineDirection_ = 1;
                        Debug.Log("右");
                    }
                    else
                    {
                        splineDirection_ = -1;
                        Debug.Log("左");
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
    
    public void CheckUnderSpline()
    {
        if (followTarget_ != null)
        {
            MoveOtherSpline(followTarget_.transform.position,-followTarget_.transform.up);
        }
    }
}
