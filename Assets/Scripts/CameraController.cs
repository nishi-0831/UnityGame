using MySpline;
using UnityEditor.Rendering;
using UnityEngine;

//[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("�J����")]
    private Camera camera_;

    [Header("�����_")]
    [SerializeField] private Transform target_;

    [Header("����")]
    [SerializeField] private float distance_;
    
    [SerializeField] private float splineOffsetY = 1.0f;

    [Header("�����_�̌���")]
    public bool isMovingLeft_ { get; set; }
    
    [Header("Y���Ǐ]�ݒ�")]
    [SerializeField][Range(0.0f, 1.0f)] private float verticalFollowSpeed_ = 0.1f;
    [SerializeField] private bool enableVerticalFollow_ = true;
    
    // SplineContainer�ύX���̋���Y�ړ��p
    private float targetBaseY_;
    [SerializeField] private bool forceYUpdate_ = false;
    [SerializeField] private float splineChangeYSpeed_ = 2.0f; // SplineContainer�ύX����Y�ړ����x
    [SerializeField] private EaseInterpolator ease_;
    private Coroutine coroutine_;
    private EvaluationInfo evaluationInfo_;

    private Vector3 prevPos_;
    private Vector3 nextPos_;

    // Y���W�̍X�V
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
        
        // �����ʒu�ݒ�
        Vector3 initialPos = target_.position + (target_.right * distance_) + new Vector3(0,splineOffsetY,0);
        camera_.transform.position = CalculatePos();
        targetBaseY_ = target_.position.y;
        
       

        // �����_�ݒ�
        UpdateLookAt();
    }

    
    private void LateUpdate()
    {
        
#if true
        // XZ���͐��m�ɒǏ]
        Vector3 dir = target_.right;
        if (isMovingLeft_)
        {
            dir *= -1;
        }
        Vector3 horizontalPos = target_.position + (dir * distance_);

        // Y���̏���
        float targetY = CalculateTargetY();
        float currentY = camera_.transform.position.y;
        
        
        if (forceYUpdate_)
        {
            // SplineContainer�ύX���͋����I�Ɉړ�
            newY = Mathf.Lerp(currentY, targetY, splineChangeYSpeed_ * Time.deltaTime);
            if (Mathf.Abs(newY - targetY) < 0.1f)
            {
                forceYUpdate_ = false;
            }
        }
        else if (enableVerticalFollow_)
        {
            // �ʏ��Y���Ǐ]
            //newY = Mathf.Lerp(currentY, targetY, verticalFollowSpeed_ * Time.deltaTime);
            newY = targetY;
        }
        else
        {
            // Y���Ǐ]����
            newY = currentY;
        }

        // �J�����ʒu�X�V
        camera_.transform.position = new Vector3(horizontalPos.x, newY, horizontalPos.z);
        
        // �����_�X�V
        UpdateLookAt();
        //Camera�́APlayer��splineController_.EvaluationInfo����Ɉʒu�𒲐����܂��B�܂��A�J�����̈ʒu�̒����͊ɂ₩�ɍs�������ł��B
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
    /// SplineContainer�ύX���ɃJ�����̃x�[�XY���W�������X�V
    /// </summary>
    /// <param name="newBaseY">�V�����x�[�XY���W</param>
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
    /// ���݂̃v���C���[�ʒu���x�[�XY���W�Ƃ��Đݒ�
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
