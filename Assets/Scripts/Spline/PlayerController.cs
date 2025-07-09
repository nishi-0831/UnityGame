using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using MySpline;
using StarterAssets;



[RequireComponent(typeof(SplineController))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : SplineMovementBase
{
    [SerializeField] float takeDamageInterval_ = 1.0f; // �_���[�W���󂯂�Ԋu
    private bool canTakeDamage_ = true; // �_���[�W���󂯂��邩�ǂ���

    [SerializeField] private float knockbackLength_ = 5f; // �m�b�N�o�b�N���鋗��
    [SerializeField] private float knockbackForce = 0;
    //������
    [SerializeField] private float attenuationRate_ = 1f;

    [SerializeField] AnimationController playerAnimationController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer�ύX���m�p
    private SplineContainer previousSplineContainer_;
    [SerializeField] private LayerMask groundLayer_;
    private int dir_;
    [Header("�f�o�b�O�p")]
    [SerializeField] private ClearZone clearZone_;
    [SerializeField] private MySpline.EvaluationInfo evaluationInfo_;
    public Vector3 halsExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;
    [SerializeField] private Rigidbody rb_;
    [SerializeField] private CapsuleCollider capsuleCollider_;

    // Spline�̐��������̕ω��ƃW�����v�𓝍����邽�߂̕ϐ�
    private Vector3 previousSplinePosition_;
    private bool isFirstFrame_ = true;

    protected override void Initialize()
    {
        if (playerAnimationController_ == null)
        {
            playerAnimationController_ = GetComponent<AnimationController>();
        }

        splineController_.splineDirection_ = 1;

        // ����SplineContainer���L�^
        previousSplineContainer_ = splineController_.currentSplineContainer_;
        
        // ����Spline�ʒu���L�^
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
    }

    void Update()
    {
        evaluationInfo_ = splineController_.EvaluationInfo;
        InputMovement();
        transform.rotation = splineController_.EvaluationInfo.rotation;

        // Spline�̊�ʒu���擾
        Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();
        
        // Spline�̐��������̕ω��ʂ��v�Z
        Vector3 splineVerticalDelta = Vector3.zero;
        if (!isFirstFrame_)
        {
            Vector3 splineDelta = currentSplinePosition - previousSplinePosition_;
            splineVerticalDelta = new Vector3(0, splineDelta.y, 0); // Y�����̂ݎ擾
        }
        
        // �W�����v�ɂ�鐂�������̈ړ��ʂ��v�Z
        Vector3 jumpVerticalMovement = Vector3.up * playerAnimationController_.VerticalVelocity * Time.deltaTime;
        vertical = jumpVerticalMovement; // �f�o�b�O�p

        // ���݂̃v���C���[�ʒu���琅�������̐������擾
        Vector3 currentHorizontalPosition = new Vector3(currentSplinePosition.x, transform.position.y, currentSplinePosition.z);
        
        // �V�����ʒu = Spline�̐����ʒu + Spline�̐����ω� + �W�����v�̐����ړ�
        Vector3 newPosition = currentHorizontalPosition + splineVerticalDelta + jumpVerticalMovement;
        transform.position = newPosition;

        // �n�ʔ����PlayerAnimationController�ɔ��f
        playerAnimationController_.Grounded = Physics.CheckBox(transform.position, halsExtends_, transform.rotation, groundLayer_);
        
        // �O�t���[���̈ʒu���X�V
        previousSplinePosition_ = currentSplinePosition;
        isFirstFrame_ = false;
        
        CheckSplineContainerChange();
        UpdateCamera();
    }

    public void CheckSpline(SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.Log("checksplineContainer==null");
            return;
        }
        if(splineController_.currentSplineContainer_ != splineContainer)
        {
            splineController_.ChangeOtherSpline(splineContainer);
        }
    }

   
    private void OnCollisionEnter(Collision collision)
    {
        GameObject groundObj = collision.gameObject;
        Debug.Log("groundObj:" + groundObj.name);
        Debug.Log("collisionObject:" + collision.gameObject.name);
        if (groundObj.layer == (int)Mathf.Log(groundLayer_, 2))
        {
            SplineContainer collisionContainer = groundObj.GetComponent<SplineContainer>();
            CheckSpline(collisionContainer);
        }
    }
    private void InputMovement()
    {
        
        if (!playerAnimationController_.IsStunned)
        {
            if(inputs_.move.x == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if(inputs_.move.x == 1)
            {
                splineController_.isMovingLeft = false;
            }

            
            if (Input.GetKeyDown(KeyCode.T))
            {
                playerAnimationController_.TakeDamage();
                OnDamage(0,splineController_.T + 0.5f);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                clearZone_.ClearGame();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                playerAnimationController_.Dying();
                OnTriggerDyingAnim();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerAnimationController_.AddVerticalForce(verticalForce_);
            }
        }

        // Spline���t�X�V
        splineController_.UpdateT(speed_, (int)inputs_.move.x);
        // �A�j���[�V�����p�̓��͐ݒ�
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        //moveInput.x = ;
        playerAnimationController_.SetMoveInput(inputs_.move);
        
        if (knockbackForce > 0)
        {
            knockbackForce += attenuationRate_ * Time.deltaTime;
            splineController_.UpdateT(knockbackForce, dir_);
        }
        else
        {
            knockbackForce = 0;
        }
    }

    protected override void UpdateMovement()
    {
        // Spline�̈ړ���Update���\�b�h�ōs�����߁A�����ł͉������Ȃ�
    }

    private void UpdateCamera()
    {
        if (cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = splineController_.isMovingLeft;
            cameraController_.SetEvaluationInfo(splineController_.EvaluationInfo);
        }
    }

    /// <summary>
    /// SplineContainer�ύX���`�F�b�N���A�J�����ɒʒm
    /// </summary>
    private void CheckSplineContainerChange()
    {
        if (splineController_.currentSplineContainer_ != previousSplineContainer_)
        {
            Debug.Log("SplineContainer changed!");

            // �J������SplineContainer�ύX��ʒm
            if (cameraController_ != null)
            {
                // �V����Spline�̃x�[�X���x���v�Z�i���݂̃v���C���[�ʒu��Y���W���g�p�j
                float newBaseY = splineController_.EvaluationInfo.position.y;
                cameraController_.OnSplineContainerChanged(newBaseY);
            }

            previousSplineContainer_ = splineController_.currentSplineContainer_;
        }
    }

    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        splineController_.MoveOtherSplineMinOrMax();
    }

    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        splineController_.MoveOtherSplineMinOrMax();
    }

    public override void OnDamage()
    {
        base.OnDamage();
    }

    public override void OnDamage(int damageValue)
    {
        if (!canTakeDamage_)
        {
            return; // �_���[�W���󂯂��Ȃ��ꍇ�͉������Ȃ�
        }
        hp_ -= damageValue;
        knockbackForce = Mathf.Sqrt(splineController_.GetSplineMovementT(knockbackLength_) * -2f * attenuationRate_);
        StartCoroutine(WaitCanTakeDamage());
        if (hp_ <= 0)
        {
            //OnTriggerDyingAnim();
        }
    }

    public override void OnDamage(int damageValue, float enemyT)
    {
        dir_ = -(int)Mathf.Sign(enemyT - splineController_.T);
        OnDamage(damageValue);
        playerAnimationController_.TakeDamage();
    }

    private IEnumerator WaitCanTakeDamage()
    {
        if (canTakeDamage_)
        {
            canTakeDamage_ = false;
            yield return new WaitForSeconds(takeDamageInterval_);
            canTakeDamage_ = true;
        }
    }

    public void OnTriggerDyingAnim()
    {
        StartCoroutine(DyingAnim());
    }

    private IEnumerator DyingAnim()
    {
        //�J�ڂɂ����鎞��
        float transitionDuration = 5.0f;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            //�Q�[���I�[�o�[�̕�����\��
            yield return null;
        }
        TransitionScene.Instance.ToGameOver();
    }

    
}
