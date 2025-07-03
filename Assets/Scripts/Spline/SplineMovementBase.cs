using UnityEngine;


[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected int hp_ = 1;
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize_ = true;
    
    [SerializeField]protected SplineController splineController_;
    protected bool isActive_ = true;
    protected Collider targetCollider_;

    [Header("���C���[�̐ݒ�")]
    [SerializeField] protected SplineLayerSettings layerSettings_;
    
    public GameObject FollowTarget
    {
        get { return splineController_.FollowTarget; }
        set { splineController_.FollowTarget = value; }
    }
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
    private void Awake()
    {
        InitializeComponents();
        Initialize();
    }
    
    private  void Start()
    {
        if (autoInitialize_)
        {
            if (splineController_ != null)
            {
                //�����ʒu�ݒ�
                splineController_.MoveAlongSpline(FirstT);

                // �C�x���g�̓o�^
                splineController_.onMaxT += OnReachMaxT;
                splineController_.onMinT += OnReachMinT;
            }
        }
    }
    
    private void Update()
    {
        if (!isActive_)
        {
            return;
        }

        UpdateMovement();
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
        FollowTarget = gameObject;
        targetCollider_ = FollowTarget.GetComponent<Collider>();
        //includelayer����ݒ�...
        //if(layerSettings_ != null)
        {
            targetCollider_.excludeLayers = layerSettings_.disabledLayer;

            FollowTarget.layer = (int)Mathf.Log(layerSettings_.activeLayer.value, 2);
            
        }

        
        
    }
   
   

    /// <summary>
    /// ����������
    /// </summary>
    protected virtual void Initialize()
    {

    }

    /// <summary>
    /// ���t���[���̈ړ�����
    /// </summary>
    protected virtual void UpdateMovement()
    {
       
    }
    
    /// <summary>
    /// t�l��1.0�𒴂������̏����i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnReachMaxT()
    {
        //Debug.Log($"{gameObject.name}: Reached Max T");
    }
    
    /// <summary>
    /// t�l��0.0������������̏����i�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnReachMinT()
    {
        //Debug.Log($"{gameObject.name}: Reached Min T");
    }
   
    
    protected void CancelOnReachMinT()
    {
        splineController_.onMinT -= OnReachMinT;
    }

    protected void CancelOnReachMaxT()
    {
        splineController_.onMaxT -= OnReachMaxT;
    }

    /// <summary>
    /// �Ȑ���̈ړ��A�����蔻��𖳌���
    /// </summary>
    protected void Disable()
    {
        Debug.Log($"Disable:{FollowTarget.name}");
        IsActive_ = false;
        FollowTarget.layer = (int)Mathf.Log(layerSettings_.disabledLayer.value, 2);
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

    /// <summary>
    /// �_���[�W���󂯂����̏���
    /// </summary>
    public virtual void OnDamage()
    {
        Debug.Log($"{FollowTarget.name}���_���[�W��H�����");
    }
    public virtual void OnDamage(int damageValue)
    {
        Debug.Log($"{FollowTarget.name}��{damageValue}�̃_���[�W���󂯂�");
    }
}