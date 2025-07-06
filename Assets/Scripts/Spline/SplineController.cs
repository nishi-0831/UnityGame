using StarterAssets;
using System;

//using System.Numerics;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using JetBrains.Annotations;
using MySpline;
using TMPro;



#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MySpline
{
    public struct EvaluationInfo
    {

        public Vector3 position;
        public Vector3 tangent;
        public Vector3 upVector;
        public Quaternion rotation;
        public EvaluationInfo(Vector3 pos, Vector3 tan, Vector3 up, Quaternion rot)
        {

            position = pos;
            tangent = tan;
            upVector = up;
            rotation = rot;
        }
       
    }
}
public class SplineController : MonoBehaviour
{


    [SerializeField] private SplineLayerSettings splineLayerSettings_;
    [SerializeField] public GameObject followTarget_;
    [SerializeField] public SplineContainer currentSplineContainer_;
    [SerializeField] private float t_;
    [SerializeField] public int splineDirection_ = 1;
    [SerializeField] public bool isMovingLeft = false;
    [SerializeField] protected float offsetRayStartPosY = 1.0f;

    [Header("エディターで初期位置表示")]
    [SerializeField] private bool enableEditorPreview = false;
    [SerializeField]
    [Range(0f, 1f)]
    private float firstT_ = 0.0f;
    [SerializeField] float offsetY_ = 0f;
    [Header("currentSplineContainerがnullの場合、親のSplineContainerを取得するか否か")]
    [SerializeField] private bool autoFindParentSplineContainer_ = true;
    [Header("既存のcurrentSplineContainerを上書きして親のSplineContainerを取得するか否か")]
    [SerializeField] private bool overwriteCurrentWithParentSplineContainer_ = false;

    private bool onceAction_ = false;
    public Action onMaxT;
    public Action onMinT;

    private float prevT_;
    private bool isFirstFrame_ = true;
    private EvaluationInfo prevEvaluationInfo_;
    private EvaluationInfo evaluationInfo_;
    
    public EvaluationInfo EvaluationInfo { get { return evaluationInfo_; } }
    // followTarget_のプロパティアクセサを追加
    public GameObject FollowTarget 
    { 
        get { return followTarget_; } 
        set { followTarget_ = value; } 
    }
    public float T
    {
        get { return t_; }
        set 
        {
            t_ = value;
            evaluationInfo_ = GetEvaluationInfo(t_);
        }
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
    public float PrevT
    {
        get { return prevT_; }
    }

    #region EditModePreview
#if UNITY_EDITOR
    /// <summary>
    /// Edit Mode/Play Mode問わず呼びだされる、インスペクターのプロパティが変更されたときに呼び出されるメソッド
    /// </summary>
    private void OnValidate()
    {
        if(enableEditorPreview && !Application.isPlaying)
        {
            //エディタでインスペクターの変更が完了したときに呼ばれるよう設定
            UnityEditor.EditorApplication.delayCall += () =>
            {
                //delayCallがnullの時は呼び出しをスキップ
                if (this == null)
                {
                    return;
                }

                if (!Application.isPlaying)
                {
                    UpdateEditorPreview();
                }

                if (CanFindSplineContainer())
                {
                    Debug.Log("FindSplineContainer");
                    FindParentSplineContainer();
                }
            };
        }
    }
    private void OnDisable()
    {
        //delayCallの削除
        if(!Application.isPlaying)
        {
            EditorApplication.delayCall = null;
        }
    }
    private void UpdateEditorPreview()
    {
        if (this == null || !this.gameObject.activeInHierarchy)
        {
            return;
        }
        //Debug.Assert(FollowTarget != null,"followTarget == null");
        if(followTarget_ == null)
        {
            followTarget_ = this.gameObject;
            Debug.LogWarning($"{gameObject.name}:followTargetが見当たらなかったので、thisを対象とします");
        }
        if(currentSplineContainer_ == null)
        {
            Debug.LogWarning($"{gameObject.name}:currentSplineContainer == null");
            return;
        }
        
        if(enableEditorPreview)
        {
            MoveAlongSplineEditorOnly(firstT_);
        }
    }

    private void MoveAlongSplineEditorOnly(float t)
    {
        followTarget_.transform.position = GetEvaluationInfo(t).position;
        Debug.Log("MoveAlongSplineEditorOnly");
        if(!Application.isPlaying)
        {
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif
    #endregion
    private void Awake()
    {
        t_ = firstT_;
        if (CanFindSplineContainer())
        {
            FindParentSplineContainer();
        }

        if (followTarget_ != null && currentSplineContainer_ != null)
        {
            //UnityEngine.Vector3 pos = currentSplineContainer_.EvaluatePosition(t_);
            //FollowTarget.transform.position = pos;
            MoveAlongSpline(t_);
        }
        prevT_ = t_;
        splineDirection_ = 1;
        evaluationInfo_ = new EvaluationInfo();
        //evaluationInfo_.ToString();
        prevEvaluationInfo_ = evaluationInfo_;
    }

    protected bool CanFindSplineContainer()
    {
        bool ret = false;
        if (autoFindParentSplineContainer_)
        {
            if (overwriteCurrentWithParentSplineContainer_ || currentSplineContainer_ == null)
            {
               ret = true;
            }
        }
        return ret;
    }
    protected void FindParentSplineContainer()
    {
        SplineContainer[] components = GetComponentsInParent<SplineContainer>();
        if (components.Length > 0)
        {
            currentSplineContainer_ = components[0];
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(t_ < 0)
        {
            onMinT?.Invoke();
            if(onceAction_)
            {
               foreach(var d in onMinT.GetInvocationList())
                {
                    onMinT -= (Action)d;
                }
            }
        }
        else if(t_ > 1.0f)
        {
            onMaxT?.Invoke();
            if (onceAction_)
            {
                foreach (var d in onMaxT.GetInvocationList())
                {
                    onMaxT -= (Action)d;
                }
            }
        }    
        evaluationInfo_ = GetEvaluationInfo(t_);
       
    }
    public void UpdateT(float speed)
    {
        int dir = 1;
        if(isMovingLeft)
        {
            dir = -1;
        }
        UpdateT(speed, dir);
    }
    public void UpdateT(float speed, int moveDir)
    {
        //Math.Clamp(moveDir, -1, 1);
        if (currentSplineContainer_ == null) return;
        
        float movementT = speed / currentSplineContainer_.CalculateLength();

        prevT_ = t_;
        t_ += (movementT * moveDir * splineDirection_);

        evaluationInfo_ = GetEvaluationInfo(t_);
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

    public float GetCurrSplineLength()
    {
        return currentSplineContainer_.CalculateLength();
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

        Vector3 currentPosition = evaluationInfo_.position;
        Vector3 delta = currentPosition - GetEvaluationInfo(prevT_).position;
        
        
        return delta;
    }

    public EvaluationInfo GetEvaluationInfo(float t)
    {
        if (currentSplineContainer_ == null || followTarget_ == null)
        {}
        
        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        if (isMovingLeft) tangent *= -1;
        tangent *= splineDirection_;
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);

        return new EvaluationInfo(nearestPos + new float3(0,offsetY_,0),tangent,upVector,rotation);
    }
    
   
    public void MoveAlongSpline(float t)
    {
        Debug.Log($"{followTarget_.name}:MoveAlongSpline");
        EvaluationInfo spline = GetEvaluationInfo(t);
        followTarget_.transform.rotation = spline.rotation;
        followTarget_.transform.position = spline.position;
    }
    public void ClampT()
    {
        T = Mathf.Clamp01(t_);
    }
    #region 他のSplineContainerへの移動関連
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
        
        if (Physics.Raycast(pos + new Vector3(0,offsetRayStartPosY,0), dir, out hit, Mathf.Infinity, splineLayerSettings_.groundLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            SplineContainer nextContainer = hitObject.GetComponent<SplineContainer>();
            var hitLayerMask = (int)Mathf.Log(splineLayerSettings_.groundLayer, 2);
            if ((hitLayerMask != hitObject.layer))
            {
                Debug.Log(hitLayerMask);
                Debug.Log($"{hitObject.name}:{hitObject.layer}");
                return; 
            }
            if (nextContainer == null)
            {
                ClampT();
                MoveAlongSpline(t_);
                return;
            }
            if(nextContainer == currentSplineContainer_)
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
                T = outT;
                Debug.Log("currT:" + t_);
            }
        }
        else
        {
            Debug.Log("Raycast == false");
            ClampT();
            MoveAlongSpline(t_);
        }
    }
    
    public void CheckUnderSpline()
    {
        if (followTarget_ != null)
        {
            MoveOtherSpline(followTarget_.transform.position,-followTarget_.transform.up);
        }
    }
    #endregion
}
