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
    [Header("�����_�̍���")]
    [SerializeField] private float height_ = 1.0f;

    [Header("�����_�̌���")]
    public bool isMovingLeft_ { get; set; }
    
    [Header("Y���Ǐ]�ݒ�")]
    [SerializeField][Range(0.0f, 1.0f)] private float verticalFollowSpeed_ = 0.1f;
    [SerializeField] private bool enableVerticalFollow_ = true;
    
    // SplineContainer�ύX���̋���Y�ړ��p
    private float targetBaseY_;
    private bool forceYUpdate_ = false;
    [SerializeField] private float splineChangeYSpeed_ = 2.0f; // SplineContainer�ύX����Y�ړ����x

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera_ = GetComponent<Camera>();
        if (camera_ == null)
        {
            Debug.LogError("camera_ is NULL");
        }
        
        // �����ʒu�ݒ�
        Vector3 initialPos = target_.position + (target_.right * distance_);
        camera_.transform.position = initialPos;
        targetBaseY_ = target_.position.y;
        
        // �����_�ݒ�
        UpdateLookAt();
    }

    
    private void LateUpdate()
    {
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
        
        // Y���W�̍X�V
        float newY;
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
            newY = Mathf.Lerp(currentY, targetY, verticalFollowSpeed_ * Time.deltaTime);
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
    }
    
    private float CalculateTargetY()
    {
        return targetBaseY_ + height_;
    }
    
    private void UpdateLookAt()
    {
        Vector3 lookTarget = new Vector3(target_.position.x, CalculateTargetY(), target_.position.z);
        Vector3 lookDirection = (lookTarget - camera_.transform.position).normalized;
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
}
