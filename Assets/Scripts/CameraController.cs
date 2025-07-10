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
    [SerializeField] private float distance_ = 4.0f; // �v���C���[����̋���
    [SerializeField] private float polarAngle_ = 70.0f; // Y���Ƃ̊p�x�i�Ɋp�j
    [SerializeField] private float azimuthalAngle_ = 90.0f; // ���ʊp�iXZ���ʂł̊p�x�j
    [SerializeField] private float splineOffsetY_ = 1.0f; // Spline����̐����I�t�Z�b�g

    [Header("Angle Limits")]
    [SerializeField] private float minDistance_ = 1.0f;
    [SerializeField] private float maxDistance_ = 9.0f;
    [SerializeField] private float minPolarAngle_ = 5.0f;
    [SerializeField] private float maxPolarAngle_ = 85.0f;

    [Header("Movement Speed")]
    [SerializeField] private bool interpolateAzimuthalWhenNotTransitioning_ = false; // isTransitioning_��false�̎��ɕ��ʊp���Ԃ��邩
    [SerializeField] private float azimuthalLerpSpeed_ = 2.0f; // ���ʊp��ԑ��x
    [SerializeField] private float polarLerpSpeed_ = 2.0f; // �Ɋp��ԑ��x
    [SerializeField] private float distanceLerpSpeed_ = 2.0f; // ������ԑ��x
    [SerializeField] private float splineChangeHorizontalSpeed = 3.0f; // SplineContainer�ύX���̕�ԑ��x
    [SerializeField] private float splineChangeVerticalSpeed = 3.0f; // SplineContainer�ύX���̕�ԑ��x

    [Header("Player Direction Control")]
    public bool isMovingLeft_ { get; set; }

    // SplineContainer�ύX���̕�Ԑ���
    private bool isTransitioning_ = false;
    private bool forceYUpdate_ = false;
    private float targetAzimuthalAngle_;
    private EvaluationInfo evaluationInfo_;
    private Vector3 previousTangent_;
    private bool isFirstFrame_ = true;
    private float newY;

    // �f�o�b�O�p
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

        // �����ݒ�
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
    /// �J�����̊p�x���X�V
    /// </summary>
    private void UpdateCameraAngles()
    {
        // ���݂̃^���W�F���g����ڕW���ʊp���v�Z
        Vector3 currentTangent = evaluationInfo_.tangent;
        
        if (currentTangent != Vector3.zero)
        {
            // �v���C���[�̐i�s�����ɑ΂��ĉE���̕��ʊp���v�Z
            Vector3 rightDirection = GetRightVector();
            float desiredAzimuthal = CalculateAzimuthalAngle(rightDirection);
            
            // SplineContainer�ύX���̋}���Ȋp�x�ω������o
            if (!isFirstFrame_ && Vector3.Dot(currentTangent.normalized, previousTangent_.normalized) < 0.7f)
            {
                // �傫�Ȋp�x�ω������������ꍇ�A��Ԃ��J�n
                if (!isTransitioning_)
                {
                    StartTransition(desiredAzimuthal);
                }
            }
            else if (!isTransitioning_)
            {
                // �ʏ펞�͖ڕW�p�x�𒼐ڐݒ�
                targetAzimuthalAngle_ = desiredAzimuthal;
            }

            // ���ʊp�̕��
            if (isTransitioning_)
            {
                azimuthalAngle_ = LerpAngle(azimuthalAngle_, targetAzimuthalAngle_, splineChangeHorizontalSpeed * Time.deltaTime);
                
                // ��Ԋ�������
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

        // �f�o�b�O���X�V
        currentTargetAzimuthal_ = targetAzimuthalAngle_;
        tangentAngle_ = CalculateAzimuthalAngle(evaluationInfo_.tangent);
    }

    /// <summary>
    /// �v���C���[�̉E�����x�N�g�����擾
    /// </summary>
    private Vector3 GetRightVector()
    {
        Vector3 tangent = evaluationInfo_.tangent.normalized;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, tangent).normalized;
        
        // ���E�̈ړ������ɉ����ĉE�x�N�g���𒲐�
        if (isMovingLeft_)
        {
            right = -right;
        }

        return right;
    }

    /// <summary>
    /// �x�N�g��������ʊp���v�Z
    /// </summary>
    private float CalculateAzimuthalAngle(Vector3 direction)
    {
        //��������p�x��Ԃ��BXZ���ʂȂ�Z,X�̏��B
        //���ʍ��W�n�̃��W�A���ŕԂ��Ă���
        return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// �p�x�̕�ԁi360�x�Ή��j
    /// </summary>
    private float LerpAngle(float current, float target, float speed)
    {
        //��̊p�x�̊p�x����Ԃ��֐��B360�x�����Ă����m�Ȓl��Ԃ�
        float delta = Mathf.DeltaAngle(current, target);
        return current + delta * speed;
    }

    /// <summary>
    /// SplineContainer�ύX���̕�ԊJ�n
    /// </summary>
    private void StartTransition(float newTargetAngle)
    {
        isTransitioning_ = true;
        targetAzimuthalAngle_ = newTargetAngle;
        Debug.Log($"Camera: Starting transition to angle {newTargetAngle:F1}��");
    }

    /// <summary>
    /// �J�����ʒu���X�V
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        
        // ���ʍ��W�n���璼�����W�n�ւ̕ϊ�
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
    /// �J�����̌������X�V
    /// </summary>
    private void UpdateLookAt()
    {
        //Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        Vector3 lookAtPos = transform.position +  (-GetRightVector() * distance_);
        transform.LookAt(lookAtPos);
    }

    /// <summary>
    /// SplineContainer�ύX���̒ʒm
    /// </summary>
    public void OnSplineContainerChanged(float newBaseY)
    {
        forceYUpdate_ = true;
        Debug.Log($"Camera: SplineContainer changed, new base Y: {newBaseY}");
    }

    /// <summary>
    /// EvaluationInfo�ݒ�
    /// </summary>
    public void SetEvaluationInfo(EvaluationInfo info)
    {
        evaluationInfo_ = info;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void AdjustDistance(float scrollDelta)
    {
        distance_ -= scrollDelta;
        distance_ = Mathf.Clamp(distance_, minDistance_, maxDistance_);
    }

    /// <summary>
    /// �Ɋp����
    /// </summary>
    public void AdjustPolarAngle(float delta)
    {
        polarAngle_ += delta;
        polarAngle_ = Mathf.Clamp(polarAngle_, minPolarAngle_, maxPolarAngle_);
    }

    private void OnDrawGizmosSelected()
    {
        if (target_ == null) return;

        // �J�����ʒu
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // �����_
        Vector3 lookAtPos = evaluationInfo_.position + Vector3.up * splineOffsetY_;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAtPos, 0.3f);

        // ������
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, lookAtPos);

        // ���ʊp�̉���
        Gizmos.color = Color.red;
        Vector3 azimuthalDirection = new Vector3(
            Mathf.Cos(azimuthalAngle_ * Mathf.Deg2Rad),
            0,
            Mathf.Sin(azimuthalAngle_ * Mathf.Deg2Rad)
        );
        Gizmos.DrawRay(lookAtPos, azimuthalDirection * 2f);
    }
}
