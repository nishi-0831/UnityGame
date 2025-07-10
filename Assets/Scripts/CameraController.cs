using MySpline;
using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    private Camera camera_;

    [Header("Target")]
    [SerializeField] private Transform target_;

    [Header("Spherical Coordinate Settings")]
    [SerializeField] private float distance_ = 4.0f; // プレイヤーからの距離
    [SerializeField] private float polarAngle_ = 70.0f; // Y軸との角度（極角）
    [SerializeField] private float azimuthalAngle_ = 90.0f; // 方位角（XZ平面での角度）
    [SerializeField] private float splineOffsetY_ = 1.0f; // Splineからの垂直オフセット

    [Header("Angle Limits")]
    [SerializeField] private float minDistance_ = 1.0f;
    [SerializeField] private float maxDistance_ = 9.0f;
    [SerializeField] private float minPolarAngle_ = 5.0f;
    [SerializeField] private float maxPolarAngle_ = 85.0f;

    [Header("Movement Speed")]
    [SerializeField] private bool interpolateAzimuthalWhenNotTransitioning_ = false; // isTransitioning_がfalseの時に方位角を補間するか
    [SerializeField] private float azimuthalLerpSpeed_ = 2.0f; // 方位角補間速度
    [SerializeField] private float polarLerpSpeed_ = 2.0f; // 極角補間速度
    [SerializeField] private float distanceLerpSpeed_ = 2.0f; // 距離補間速度
    [SerializeField] private float splineChangeHorizontalSpeed = 3.0f; // SplineContainer変更時の補間速度
    [SerializeField] private float splineChangeVerticalSpeed = 3.0f; // SplineContainer変更時の補間速度

    [Header("Player Direction Control")]
    public bool isMovingLeft_ { get; set; }

    // SplineContainer変更時の補間制御
    private bool isTransitioning_ = false;
    private bool forceYUpdate_ = false;
    private float targetAzimuthalAngle_;
    private EvaluationInfo evaluationInfo_;
    private Vector3 previousTangent_;
    private bool isFirstFrame_ = true;
    private float newY;

    // デバッグ用
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo_ = false;
    [SerializeField] private float currentTargetAzimuthal_;
    [SerializeField] private float tangentAngle_;

    void Start()
    {
        camera_ = GetComponent<Camera>();
        if (camera_ == null)
        {
            Debug.LogError("Camera component not found!");
        }

        // 初期設定
        if (target_ != null)
        {
            UpdateCameraPosition();
        }
    }

    private void LateUpdate()
    {
        if (target_ == null || evaluationInfo_.position == Vector3.zero) return;

        UpdateCameraAngles();
        UpdateCameraPosition();
        UpdateLookAt();

        if (showDebugInfo_)
        {
            Debug.DrawRay(target_.position, evaluationInfo_.tangent * 3f, Color.red);
            Debug.DrawRay(target_.position, GetRightVector() * 3f, Color.blue);
        }
    }

    /// <summary>
    /// カメラの角度を更新
    /// </summary>
    private void UpdateCameraAngles()
    {
        // 現在のタンジェントから目標方位角を計算
        Vector3 currentTangent = evaluationInfo_.tangent;
        
        if (currentTangent != Vector3.zero)
        {
            // プレイヤーの進行方向に対して右側の方位角を計算
            Vector3 rightDirection = GetRightVector();
            float desiredAzimuthal = CalculateAzimuthalAngle(rightDirection);
            
            // SplineContainer変更時の急激な角度変化を検出
            if (!isFirstFrame_ && Vector3.Dot(currentTangent.normalized, previousTangent_.normalized) < 0.7f)
            {
                // 大きな角度変化が発生した場合、補間を開始
                if (!isTransitioning_)
                {
                    StartTransition(desiredAzimuthal);
                }
            }
            else if (!isTransitioning_)
            {
                // 通常時は目標角度を直接設定
                targetAzimuthalAngle_ = desiredAzimuthal;
            }

            // 方位角の補間
            if (isTransitioning_)
            {
                azimuthalAngle_ = LerpAngle(azimuthalAngle_, targetAzimuthalAngle_, splineChangeHorizontalSpeed * Time.deltaTime);
                
                // 補間完了判定
                if (Mathf.Abs(Mathf.DeltaAngle(azimuthalAngle_, targetAzimuthalAngle_)) < 1f)
                {
                    isTransitioning_ = false;
                }
            }
            else if (interpolateAzimuthalWhenNotTransitioning_)
            {
                azimuthalAngle_ = LerpAngle(azimuthalAngle_, targetAzimuthalAngle_, azimuthalLerpSpeed_ * Time.deltaTime);
            }
            else
            {
                azimuthalAngle_ = targetAzimuthalAngle_;
            }

            float targetY = evaluationInfo_.position.y + splineOffsetY_;
            if (forceYUpdate_)
            {
                newY = Mathf.Lerp(camera_.transform.position.y, targetY, splineChangeVerticalSpeed * Time.deltaTime);
                if(Mathf.Abs(newY - targetY) < 0.1f)
                {
                    forceYUpdate_ = false;
                }
            }
            else
            {
                newY = targetY;
            }

            transform.position = new Vector3(camera_.transform.position.x, newY, camera_.transform.position.z);

            previousTangent_ = currentTangent;
            isFirstFrame_ = false;
        }

        // デバッグ情報更新
        currentTargetAzimuthal_ = targetAzimuthalAngle_;
        tangentAngle_ = CalculateAzimuthalAngle(evaluationInfo_.tangent);
    }

    /// <summary>
    /// プレイヤーの右方向ベクトルを取得
    /// </summary>
    private Vector3 GetRightVector()
    {
        Vector3 tangent = evaluationInfo_.tangent.normalized;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, tangent).normalized;
        
        // 左右の移動方向に応じて右ベクトルを調整
        if (isMovingLeft_)
        {
            right = -right;
        }

        return right;
    }

    /// <summary>
    /// ベクトルから方位角を計算
    /// </summary>
    private float CalculateAzimuthalAngle(Vector3 direction)
    {
        //引数から角度を返す。XZ平面ならZ,Xの順。
        //球面座標系のラジアンで返ってくる
        return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 角度の補間（360度対応）
    /// </summary>
    private float LerpAngle(float current, float target, float speed)
    {
        //二つの角度の角度差を返す関数。360度超えても正確な値を返す
        float delta = Mathf.DeltaAngle(current, target);
        return current + delta * speed;
    }

    /// <summary>
    /// SplineContainer変更時の補間開始
    /// </summary>
    private void StartTransition(float newTargetAngle)
    {
        isTransitioning_ = true;
        targetAzimuthalAngle_ = newTargetAngle;
        Debug.Log($"Camera: Starting transition to angle {newTargetAngle:F1}°");
    }

    /// <summary>
    /// カメラ位置を更新
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        
        // 球面座標系から直交座標系への変換
        float azimuthalRad = azimuthalAngle_ * Mathf.Deg2Rad;
        float polarRad = polarAngle_ * Mathf.Deg2Rad;

        Vector3 sphericalPosition = new Vector3(
            lookAtPos.x + distance_ * Mathf.Sin(polarRad) * Mathf.Cos(azimuthalRad),
            //lookAtPos.y + distance_ * Mathf.Cos(polarRad),
            transform.position.y,
            lookAtPos.z + distance_ * Mathf.Sin(polarRad) * Mathf.Sin(azimuthalRad)
        );

        transform.position = sphericalPosition;
    }

    /// <summary>
    /// カメラの向きを更新
    /// </summary>
    private void UpdateLookAt()
    {
        //Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        Vector3 lookAtPos = transform.position +  (-GetRightVector() * distance_);
        transform.LookAt(lookAtPos);
    }

    /// <summary>
    /// SplineContainer変更時の通知
    /// </summary>
    public void OnSplineContainerChanged(float newBaseY)
    {
        forceYUpdate_ = true;
        Debug.Log($"Camera: SplineContainer changed, new base Y: {newBaseY}");
    }

    /// <summary>
    /// EvaluationInfo設定
    /// </summary>
    public void SetEvaluationInfo(EvaluationInfo info)
    {
        evaluationInfo_ = info;
    }

    /// <summary>
    /// 距離調整
    /// </summary>
    public void AdjustDistance(float scrollDelta)
    {
        distance_ -= scrollDelta;
        distance_ = Mathf.Clamp(distance_, minDistance_, maxDistance_);
    }

    /// <summary>
    /// 極角調整
    /// </summary>
    public void AdjustPolarAngle(float delta)
    {
        polarAngle_ += delta;
        polarAngle_ = Mathf.Clamp(polarAngle_, minPolarAngle_, maxPolarAngle_);
    }

    private void OnDrawGizmosSelected()
    {
        if (target_ == null) return;

        // カメラ位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 注視点
        Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAtPos, 0.3f);

        // 距離線
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, lookAtPos);

        // 方位角の可視化
        Gizmos.color = Color.red;
        Vector3 azimuthalDirection = new Vector3(
            Mathf.Cos(azimuthalAngle_ * Mathf.Deg2Rad),
            0,
            Mathf.Sin(azimuthalAngle_ * Mathf.Deg2Rad)
        );
        Gizmos.DrawRay(lookAtPos, azimuthalDirection * 2f);
    }
}
