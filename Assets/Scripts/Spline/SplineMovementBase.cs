
using UnityEngine;


[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected int hp_ = 1;
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize_ = true;
    [SerializeField] protected string wallTag_ = "Wall";
    [SerializeField]protected SplineController splineController_;
    [SerializeField]protected bool isActive_ = true;
    protected Collider targetCollider_;

    [Header("���C���[�̐ݒ�")]
    [SerializeField] protected SplineLayerSettings layerSettings_;
    [Header("OnDestroy�ōĐ������SE")]
    [SerializeField] private AudioClip onDestroySE_ = null;

    public GameObject FollowTarget
    {
        get { return splineController_.FollowTarget; }
        set { splineController_.FollowTarget = value; }
    }

    //public bool isOffSpline_
    //{
    //    get { return splineController_.isOffSpline_; }
    //    set { splineController_.isOffSpline_ = value; }
    //}

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
    
    protected virtual  void Start()
    {
        if (autoInitialize_)
        {
            if (splineController_ != null)
            {
                //�����ʒu�ݒ�
                splineController_.MoveAlongSpline(splineController_.T);

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
    /// WallTag��Collider�ɏՓ˂������̏��� (�h���N���X�ŃI�[�o�[���C�h�j
    /// </summary>
    protected virtual void OnCollideWall()
    {
        //Debug.Log($"{gameObject.name}: Collide Wall");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(wallTag_))
        {
            OnCollideWall();
        }
    }
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.CompareTag(wallTag_))
    //    {
    //        OnCollideWall();
    //    }
    //    Debug.Log(other.gameObject.name);
    //}
    

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
    /// �j���v���������Ƃ�
    /// </summary>
    public virtual void OnRequestDestroy()
    {
        Debug.Log($"{gameObject.name} : was requested destroy");
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
    /// �Ȑ���̈ړ��A�����蔻���L����
    /// </summary>
    protected void Enable()
    {
        Debug.Log($"Enable:{FollowTarget.name}");
        IsActive_ = true;
        FollowTarget.layer = (int)Mathf.Log(layerSettings_.activeLayer.value, 2);
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
        if(onDestroySE_ != null)
        {
            AudioSource.PlayClipAtPoint(onDestroySE_, FollowTarget.transform.position);
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

    public virtual void OnDamage(int damageValue, float enemyT)
    {

    }
    public virtual void OnDamage(int damageValue,Vector3 enemyPos)
    { }
}