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

using UnityEngine.Events;






#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MySpline
{
    [System.Serializable]
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


    [SerializeField] public SplineLayerSettings splineLayerSettings_;
    [SerializeField] public GameObject followTarget_;
    [SerializeField] public SplineContainer currentSplineContainer_;
    [Range(0f, 1f)]
    [SerializeField] private float t_;
    [SerializeField] public int splineDirection_ = 1;
    [SerializeField] public bool isMovingLeft = false;
    [SerializeField] protected float offsetRayStartPosY = 1.0f;

    [Header("エディターで初期位置表示")]
    [SerializeField] private bool enableEditorPreview = false;
    [SerializeField]
    
    //追加でエディターに反映させたいときにどうぞ
    public UnityEvent onUpdateEditor_;
    [Header("メッシュの半径(上方向)")]
    [SerializeField] private float splineMeshRadius_;
    [SerializeField] public float offsetY_ = 0f;
    [Header("currentSplineContainerがnullの場合、親のSplineContainerを取得するか否か")]
    [SerializeField] private bool autoFindParentSplineContainer_ = true;
    [Header("既存のcurrentSplineContainerを上書きして親のSplineContainerを取得するか否か")]
    [SerializeField] private bool overwriteCurrentWithParentSplineContainer_ = false;
    //[SerializeField] public bool isOffSpline_ = false; // Spline範囲外にいるかどうか
    private bool onceAction_ = false;
    public Action onMaxT;
    public Action onMinT;

    private float prevT_;
    private bool isFirstFrame_ = true;
    private EvaluationInfo prevEvaluationInfo_;
    [SerializeField] private EvaluationInfo evaluationInfo_;
    [Header("自動でInfoをt_で更新する")]
    [SerializeField] private bool autoUpdateInfo_ = true;
    public EvaluationInfo EvaluationInfo 
    { get
        {
            AutoUpdateEvaluationInfo();
            return evaluationInfo_; 
        }
    }
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
            AutoUpdateEvaluationInfo();
            //AutoUpdateEvaluationInfo();
        }
    }
  
    public float PrevT
    {
        get { return prevT_; }
    }

    public float SplineMeshRadius
    {
        get { return splineMeshRadius_; }
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
                    //Debug.Log("FindSplineContainer");
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
    private  void UpdateEditorPreview()
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
            //Debug.Log(t_);
            //Debug.Log(T);
            AutoUpdateEvaluationInfo();

            //AutoUpdateEvaluationInfo();
            MoveAlongSplineEditorOnly(T);
            onUpdateEditor_?.Invoke();
            //MoveAlongSplineEditorOnly(firstT_);
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView.RepaintAll();
            }
        }

    }
    

    private void MoveAlongSplineEditorOnly(float t)
    {
        SetSplineMeshRadius();
        //followTarget_.transform.position = GetEvaluationInfo(t).position;
        MoveAlongSpline(t);
        //Debug.Log("MoveAlongSplineEditorOnly");
    }
#endif
    #endregion
    private void Awake()
    {
        //t_ = firstT_;
        if (CanFindSplineContainer())
        {
            FindParentSplineContainer();
        }

        if (followTarget_ != null && currentSplineContainer_ != null)
        {
            //MoveAlongSpline(t_);
            SetSplineMeshRadius();
        }
        prevT_ = t_;
        splineDirection_ = 1;
        evaluationInfo_ = new EvaluationInfo();
        //evaluationInfo_.ToString();
        prevEvaluationInfo_ = evaluationInfo_;
    }
    public void SetSplineMeshRadius()
    {
        SplineExtrude splineExtrude = currentSplineContainer_.GetComponent<SplineExtrude>();
        if (splineExtrude != null)
        {
            splineMeshRadius_ = splineExtrude.Radius;
        }
        else
        {
            splineMeshRadius_ = 0;
        }
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
        //AutoUpdateEvaluationInfo();
        AutoUpdateEvaluationInfo();

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
        
        float movementT = (speed * Time.deltaTime) / currentSplineContainer_.CalculateLength();
        //float movementT = speed / currentSplineContainer_.CalculateLength();

        prevT_ = t_;
        t_ += (movementT * moveDir * splineDirection_);

        AutoUpdateEvaluationInfo();
    }

    /// <summary>
    /// 実際の移動量からt値を更新する
    /// </summary>
    /// <param name="actualMovement">CharacterController.Moveによって実際に移動したベクトル</param>
    public void UpdateTFromMovement(Vector3 actualMovement)
    {
        if (currentSplineContainer_ == null || actualMovement.sqrMagnitude < 0.001f)
        {
            return;
        }

        // 現在の接線方向（水平成分のみ使用）
        Vector3 tangent = EvaluationInfo.tangent.normalized;

        //// tangentの水平成分のみを使用（Y成分を除去）
        //Vector3 horizontalTangent = new Vector3(tangent.x, 0, tangent.z).normalized;

        //// actualMovementの水平成分のみ使用
        //Vector3 horizontalMovement = new Vector3(actualMovement.x, 0, actualMovement.z);

        //// 水平移動量をSplineの水平接線方向に射影
        //float projectedDistance = Vector3.Dot(horizontalMovement, horizontalTangent);

        float projDistance = Vector3.Dot(actualMovement, tangent);
        // 移動距離をt値の変化量に変換
        //float deltaT = projectedDistance / currentSplineContainer_.CalculateLength();
        float deltaT = projDistance / currentSplineContainer_.CalculateLength();

        prevT_ = t_;
        if (isMovingLeft)
        {
            t_ -= deltaT;
        }
        else
        {
            t_ += deltaT;
        }

        // t値更新後、evaluationInfoも更新
        evaluationInfo_ = GetEvaluationInfo(t_);
    }

    private void AutoUpdateEvaluationInfo()
    {
        if(autoUpdateInfo_)
        {
            evaluationInfo_ = GetEvaluationInfo(t_);
        }
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
        
        if (currentSplineContainer_ == null )
        {
            Debug.LogError("currentSplineContainer_ is null");
            return new EvaluationInfo(Vector3.zero, Vector3.forward, Vector3.up, Quaternion.identity);
        }
        if(followTarget_ == null)
        {
            Debug.LogError("followTarget_ is null");
            return new EvaluationInfo(Vector3.zero, Vector3.forward, Vector3.up, Quaternion.identity);
        }
        
        // t値を0-1の範囲にクランプ
        t = Mathf.Clamp01(t);

        Spline spline = currentSplineContainer_.Spline;
        NativeSpline nativeSpline = new NativeSpline(spline, currentSplineContainer_.transform.localToWorldMatrix);
        float3 nearestPos;
        float3 tangent;
        float3 upVector;

        
        SplineUtility.Evaluate<NativeSpline>(nativeSpline, t, out nearestPos, out tangent, out upVector);
        
        // tangentがゼロベクトルでないかチェック
        if (math.lengthsq(tangent) < 0.0001f)
        {
            //Debug.LogWarning($"Tangent is zero at t={t} for spline {currentSplineContainer_.name}. Using default forward direction.");
            Debug.LogWarning($"Tangent is zero at t={t_} for spline {currentSplineContainer_.name}. Using Clamp01 Tangent");
            //tangent = new float3(0, 0, 1); // デフォルト方向
            
            tangent = GetClamp01Tangent();
        }
        
        // 方向の調整
        if (isMovingLeft) tangent *= -1;
        tangent *= splineDirection_;
        
        // 再度ゼロベクトルチェック
        if (math.lengthsq(tangent) < 0.0001f)
        {
            Debug.LogWarning($"Tangent became zero after direction adjustment{currentSplineContainer_.name}. Using Tangent forward.");
            tangent = GetClamp01Tangent();
        }

        // upVectorもチェック
        if (math.lengthsq(upVector) < 0.0001f)
        {
            Debug.LogWarning($"UpVector became zero after direction adjustment. Using default up direction.");
            upVector = GetClamp01UpVector(); // デフォルトのup方向
        }

        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);

        return new EvaluationInfo(nearestPos + new float3(0,offsetY_,0),tangent,upVector,rotation);
    }
    
   public void MoveAlongSpline()
    {
        MoveAlongSpline(t_);
    }
    public void MoveAlongSpline(float t)
    {
        //Debug.Log($"{followTarget_.name}:MoveAlongSpline");
        EvaluationInfo spline = GetEvaluationInfo(t);
        followTarget_.transform.rotation = spline.rotation;
        followTarget_.transform.position = spline.position + new Vector3(0, splineMeshRadius_ / 2.0f, 0);
    }
    public Vector3 GetSplineMeshPos()
    {
        return EvaluationInfo.position + new Vector3(0, splineMeshRadius_ / 2.0f, 0);
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

        //MoveOtherSpline(ft.position + (move* ft.forward) , -ft.up);
    }

    public void ChangeOtherSpline(SplineContainer nextContainer)
    {
        if(currentSplineContainer_ == nextContainer)
        {
            Debug.Log("SameSpline");
            return;
        }
        if(currentSplineContainer_ == null)
        {
            Debug.Log("currentSplineContainer_ == null");
            return;
        }
        if (nextContainer == null)
        {
            Debug.Log("nextContainer == null");
            return;
        }

        NativeSpline nextNativeSpline = new NativeSpline(nextContainer.Spline, nextContainer.transform.localToWorldMatrix);
        float3 outPos;
        float outT;
        SplineUtility.GetNearestPoint<NativeSpline>(nextNativeSpline, followTarget_.transform.position, out outPos, out outT);
        float nextT = outT;

        // t_値をクランプ
        float currT = Mathf.Clamp01(t_);
        Debug.Log($"Current t_: {currT}");
        
        // tangentを安全に取得
        float3 currTangent = currentSplineContainer_.EvaluateTangent(currT);
        float3 nextTangent = nextContainer.EvaluateTangent(nextT);
        
        // NaNチェック
        if (float.IsNaN(currTangent.x) || float.IsNaN(currTangent.y) || float.IsNaN(currTangent.z))
        {
            Debug.LogError($"Current tangent is NaN at t={currT} for spline {currentSplineContainer_.name}");
            currTangent = GetClamp01Tangent();
        }
        
        if (float.IsNaN(nextTangent.x) || float.IsNaN(nextTangent.y) || float.IsNaN(nextTangent.z))
        {
            Debug.LogError($"Next tangent is NaN at t={nextT} for spline {nextContainer.name}");
            nextTangent = new float3(0, 0, 1); // デフォルト方向
        }
        
        // ゼロベクトルチェック
        if (math.lengthsq(currTangent) < 0.0001f)
        {
            Debug.LogWarning($"Current tangent is zero. Using Clamp01 tangent.");
            currTangent = GetClamp01Tangent();
        }
        
        if (math.lengthsq(nextTangent) < 0.0001f)
        {
            Debug.LogWarning($"Next tangent is zero at t={nextT}. Using default direction.");
            nextTangent = new float3(0, 0, 1);
        }
        
        // 正規化
        currTangent = math.normalize(currTangent);
        nextTangent = math.normalize(nextTangent);
        
        float dot = math.dot(currTangent, nextTangent);
        Debug.Log($"Current spline: {currentSplineContainer_.gameObject.name}");
        Debug.Log($"Next spline: {nextContainer.gameObject.name}");
        Debug.Log($"Dot product: {dot}, currTangent: {currTangent}, nextTangent: {nextTangent}");
        
        if (dot > 0.1f) // 閾値を設けて数値誤差を考慮
        {
            Debug.Log("同じ向き");
        }
        else if (dot < -0.1f)
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
                //splineDirectionは変えない
                splineDirection_ = 1;
                Debug.Log("左");
            }
        }

        currentSplineContainer_ = nextContainer;
        T = outT;
        Debug.Log($"NewT: {outT}");
    }

    /// <summary>
    /// evaluationInfo_は01の範囲で値を保持している
    /// </summary>
    /// <returns></returns>
    private float3 GetClamp01Tangent()
    {
        return evaluationInfo_.tangent;
    }
    private float3 GetClamp01UpVector()
    {
        return evaluationInfo_.upVector;
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

            float3 currTangent = currentSplineContainer_.EvaluateTangent(t_);
            float3 nextTangent = nextContainer.EvaluateTangent(t_);
            float dot = math.dot(currTangent, nextTangent);
            {
                //Spline currentSpline = currentSplineContainer_.Spline;
                //NativeSpline currentNativeSpline = new NativeSpline(currentSpline, currentSplineContainer_.transform.localToWorldMatrix);
                //float3 currPos, currTangent, currUp;
                //SplineUtility.Evaluate<NativeSpline>(currentNativeSpline, t_, out currPos, out currTangent, out currUp);

                //float3 outPos;
                //float outT;
                //Debug.Log("prevT:" + t_);

                //NativeSpline nextNativeSpline = new NativeSpline(nextContainer.Spline, nextContainer.transform.localToWorldMatrix);
                //float3 nextTangent, nextUp;
                //SplineUtility.GetNearestPoint<NativeSpline>(nextNativeSpline, hit.point, out outPos, out outT);
                //SplineUtility.Evaluate<NativeSpline>(nextNativeSpline, outT, out outPos, out nextTangent, out nextUp);

                //float dot = math.dot(math.normalize(currTangent), math.normalize(nextTangent));
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
                        //splineDirection_の変更はしない
                        splineDirection_ = -1;
                        Debug.Log("左");
                    }
                }

                currentSplineContainer_ = nextContainer;
                NativeSpline currentNativeSpline = new NativeSpline(currentSplineContainer_.Spline, currentSplineContainer_.transform.localToWorldMatrix);
                float3 outPos;
                float outT;
                SplineUtility.GetNearestPoint<NativeSpline>(currentNativeSpline, hit.point, out outPos, out outT);
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
    public void RayUnderSpline(Vector3 pos, Vector3 dir)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos + new Vector3(0, offsetRayStartPosY, 0), dir, out hit, Mathf.Infinity, splineLayerSettings_.groundLayer))
        {
            Debug.Log( "ray to "+hit.collider.gameObject.name);
            SplineContainer foundSpline = hit.collider.gameObject.GetComponent<SplineContainer>();
            if (foundSpline != null && foundSpline != currentSplineContainer_)
            {
                // PlayerControllerに新しいSplineの発見を通知
                NotifyPlayerOfNewSpline(foundSpline);

            }
            else
            {
                //Debug.Log("No SplineContainer");
            }
        }
        else
        {
            Debug.Log("no ray");
        }
    }

    /// <summary>
    /// 新しいSplineが見つかった時にPlayerControllerに通知
    /// </summary>
    /// <param name="newSplineContainer">発見されたSplineContainer</param>
    private void NotifyPlayerOfNewSpline(SplineContainer newSplineContainer)
    {
        PlayerController playerController = followTarget_.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.OnFoundNewSpline(newSplineContainer);
        }
    }

    public void CheckUnderSpline()
    {
        if (followTarget_ != null)
        {
            RayUnderSpline(followTarget_.transform.position,-followTarget_.transform.up);
        }
    }

    #endregion
}
