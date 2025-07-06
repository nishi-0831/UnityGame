using MySpline;
using UnityEditor.Rendering;
using UnityEngine;

//[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("カメラ")]
    private Camera camera_;

    [Header("注視点")]
    [SerializeField] private Transform target_;

    [Header("距離")]
    [SerializeField] private float distance_;
    
    [SerializeField] private float splineOffsetY = 1.0f;

    [Header("注視点の向き")]
    public bool isMovingLeft_ { get; set; }
    
    [Header("Y軸追従設定")]
    [SerializeField][Range(0.0f, 1.0f)] private float verticalFollowSpeed_ = 0.1f;
    [SerializeField] private bool enableVerticalFollow_ = true;
    
    // SplineContainer変更時の強制Y移動用
    private float targetBaseY_;
    [SerializeField] private bool forceYUpdate_ = false;
    [SerializeField] private float splineChangeYSpeed_ = 2.0f; // SplineContainer変更時のY移動速度
    [SerializeField] private EaseInterpolator ease_;
    private Coroutine coroutine_;
    private EvaluationInfo evaluationInfo_;

    private Vector3 prevPos_;
    private Vector3 nextPos_;

    // Y座標の更新
    [SerializeField] float newY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ease_ = GetComponent<EaseInterpolator>();
        ease_.func = ease_.OutSine;
        ease_.onFinished_ = () => 
        {
            forceYUpdate_ = false;
            StopInterpolation();
        };

        camera_ = GetComponent<Camera>();
        if (camera_ == null)
        {
            Debug.LogError("camera_ is NULL");
        }
        
        // 初期位置設定
        Vector3 initialPos = target_.position + (target_.right * distance_) + new Vector3(0,splineOffsetY,0);
        camera_.transform.position = CalculatePos();
        targetBaseY_ = target_.position.y;
        
       

        // 注視点設定
        UpdateLookAt();
    }

    
    private void LateUpdate()
    {
        
#if true
        // XZ軸は正確に追従
        Vector3 dir = target_.right;
        if (isMovingLeft_)
        {
            dir *= -1;
        }
        Vector3 horizontalPos = target_.position + (dir * distance_);

        // Y軸の処理
        float targetY = CalculateTargetY();
        float currentY = camera_.transform.position.y;
        
        
        if (forceYUpdate_)
        {
            // SplineContainer変更時は強制的に移動
            newY = Mathf.Lerp(currentY, targetY, splineChangeYSpeed_ * Time.deltaTime);
            if (Mathf.Abs(newY - targetY) < 0.1f)
            {
                forceYUpdate_ = false;
            }
        }
        else if (enableVerticalFollow_)
        {
            // 通常のY軸追従
            //newY = Mathf.Lerp(currentY, targetY, verticalFollowSpeed_ * Time.deltaTime);
            newY = targetY;
        }
        else
        {
            // Y軸追従無効
            newY = currentY;
        }

        // カメラ位置更新
        camera_.transform.position = new Vector3(horizontalPos.x, newY, horizontalPos.z);
        
        // 注視点更新
        UpdateLookAt();
        //Cameraは、PlayerのsplineController_.EvaluationInfoを基準に位置を調整します。また、カメラの位置の調整は緩やかに行いたいです。
#endif
    }

    private float CalculateTargetY()
    {
        return evaluationInfo_.position.y + splineOffsetY;
        //return targetBaseY_ + splineOffsetY;
    }
    private Vector3 CalculatePos()
    {
        if(evaluationInfo_.position == null)
        {
            return new Vector3();
        }
        return  evaluationInfo_.position + (target_.right * distance_) + new Vector3(0, splineOffsetY, 0);
    }
    private void UpdateLookAt()
    {
        Vector3 lookDirection = -target_.right;
        if (isMovingLeft_)
        {
            lookDirection *= -1;
        }
        Vector3 lookTarget = camera_.transform.position + (lookDirection * distance_);
        //Vector3 lookDirection = (lookTarget - camera_.transform.position).normalized;
        camera_.transform.rotation = Quaternion.LookRotation(lookDirection);
    }
    
    /// <summary>
    /// SplineContainer変更時にカメラのベースY座標を強制更新
    /// </summary>
    /// <param name="newBaseY">新しいベースY座標</param>
    public void OnSplineContainerChanged(float newBaseY)
    {
        targetBaseY_ = newBaseY;
        forceYUpdate_ = true;
        Debug.Log($"Camera: SplineContainer changed, new base Y: {newBaseY}");
    }
    
    private void interpolation(float a, float b)
    {
        Debug.Log("StartInterpolation");
        //coroutine_ = StartCoroutine(ease_.Interpolation(a, b, ease_.duration, 
        //    (value) => { newY = value; }));
        
        
    }
    private void Interpolation()
    {
        coroutine_ = StartCoroutine(ease_.Interpolation(camera_.transform.position, target_.position, ease_.duration,
        (value) => { camera_.transform.position = value + new Vector3(0, splineOffsetY, 0); }));
    }
    private void StopInterpolation()
    {
        if(coroutine_ != null)
        {
            StopCoroutine(coroutine_);
            Debug.Log("StopInterpolation");
            ease_.Reset();
        }
        
    }
    /// <summary>
    /// 現在のプレイヤー位置をベースY座標として設定
    /// </summary>
    public void UpdateBaseY()
    {
        if (target_ != null)
        {
            OnSplineContainerChanged(target_.position.y);
        }
    }
    public void SetEvaluationInfo(EvaluationInfo info)
    {
        evaluationInfo_ = info;
    }
}
