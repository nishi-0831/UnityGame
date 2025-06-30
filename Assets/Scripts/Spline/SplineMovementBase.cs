using UnityEngine;

[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize = true;
    //[SerializeField] protected float firstT_ = 0.0f;
    protected SplineController splineController_;
    protected GameObject followTarget_;
    protected bool isActive_ = true;
    public bool IsActive_
    {
        get { return isActive_; }
        protected set { isActive_ = value; }
    }
    public bool IsMovingLeft 
    {
        get { return splineController_.isMovingLeft; } 
        protected set { splineController_.isMovingLeft = value; }
    }

    public float FirstT
    {
        get { return splineController_.FirstT; }
        protected set
        {
            if (splineController_ != null)
            {
                splineController_.FirstT = value;
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(splineController_ == null)
        {
            splineController_ = GetComponent<SplineController>();
        }
    }
#endif
    protected virtual void Awake()
    {
        InitializeComponents();
    }
    
    protected void Start()
    {
        if (autoInitialize)
        {
            if (splineController_ != null)
            {
                //�����ʒu�ݒ�
                splineController_.MoveAlongSpline(FirstT);

                // �C�x���g�̓o�^
                splineController_.onMaxT += OnReachMaxT;
                splineController_.onMinT += OnReachMinT;

                Initialize();
            }
        }
    }
    
    protected virtual void Update()
    {
        if (!isActive_)
        {
            return;
        }
        if (splineController_ != null)
        {
            UpdateMovement();
        }
    }
    
    /// <summary>
    /// �K�v�ȃR���|�[�l���g�������擾
    /// </summary>
    protected virtual void InitializeComponents()
    {
        splineController_ = GetComponent<SplineController>();
        if (splineController_ == null)
        {
            Debug.LogError($"{gameObject.name}: SplineController component not found!");
            return;
        }
        
        
        
        //���g��followTarget�Ƃ��Ďg�p
        followTarget_ = gameObject;
        
        
        splineController_.FollowTarget = followTarget_;
    }
    
    /// <summary>
    /// �����������i�h���N���X�ŃI�[�o�[���C�h�\�j
    /// </summary>
    protected virtual void Initialize()
    {
        
    }
    
    /// <summary>
    /// ���t���[���̈ړ�����
    /// </summary>
    protected virtual void UpdateMovement()
    {
        // ��{�̈ړ������͔h���N���X�Ŏ���
        OnUpdateMovement();
    }
    
    /// <summary>
    /// t�l��1.0�𒴂������̏����i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnReachMaxT()
    {
        Debug.Log($"{gameObject.name}: Reached Max T");
    }
    
    /// <summary>
    /// t�l��0.0������������̏����i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnReachMinT()
    {
        Debug.Log($"{gameObject.name}: Reached Min T");
    }
    
    /// <summary>
    /// ���������̏����i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnInitialize()
    {
        // �h���N���X�Ŏ���
    }
    
    /// <summary>
    /// ���t���[���̈ړ������i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnUpdateMovement()
    {
        // �h���N���X�Ŏ���
    }
    
    /// <summary>
    /// �j�����̏���
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (splineController_ != null)
        {
            splineController_.onMaxT -= OnReachMaxT;
            splineController_.onMinT -= OnReachMinT;
        }
    }
}