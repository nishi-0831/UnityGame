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

    [SerializeField] AnimationController animController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer�ύX���m�p
    private SplineContainer previousSplineContainer_;
    [SerializeField] private LayerMask groundLayer_;
    private int dir_;
    [Header("�f�o�b�O�p")]
    [SerializeField] private ClearZone clearZone_;
    [SerializeField] private MySpline.EvaluationInfo evaluationInfo_;
    public Vector3 center_;
    public Vector3 halfExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;
    [SerializeField] private Rigidbody rb_;
    [SerializeField] private CapsuleCollider capsuleCollider_;

    // Spline�̐��������̕ω��ƃW�����v�𓝍����邽�߂̕ϐ�
    private Vector3 previousSplinePosition_;
    private Vector3 previousOffSplinePosition_;
    private bool isFirstFrame_ = true;

    // Spline�͈͊O�ł̈ړ�����
    [SerializeField] private bool isOffSpline_ = false; // Spline�͈͊O�ɂ��邩�ǂ���
    [SerializeField] private Vector3 offSplineVelocity_; // Spline�͈͊O�ł̈ړ����x
    [SerializeField] private Vector3 lastValidTangent_; // �Ō�̗L���ȃ^���W�F���g
    private float lastValidDir = 0f;

    protected override void Initialize()
    {
        if (animController_ == null)
        {
            animController_ = GetComponent<AnimationController>();
        }

        splineController_.splineDirection_ = 1;

        // ����SplineContainer���L�^
        previousSplineContainer_ = splineController_.currentSplineContainer_;

        // ����Spline�ʒu���L�^
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
    }

    void Update()
    {
#if true
        evaluationInfo_ = splineController_.EvaluationInfo;
        InputMovement();
        

        HandleSplineMovement();

        // �n�ʔ����AnimationController�ɔ��f
        animController_.Grounded = Physics.CheckBox(transform.position + center_, halfExtends_, transform.rotation, groundLayer_);

        // �O�t���[���̈ʒu���X�V
        if (!isOffSpline_)
        {
            previousSplinePosition_ = splineController_.GetSplineMeshPos();
        }
        else
        {
            previousOffSplinePosition_ = transform.position;
        }
            isFirstFrame_ = false;

        CheckSplineContainerChange();
        UpdateCamera();

        Debug.DrawRay(transform.position, offSplineVelocity_ * 1000f);
#endif
        //InputMovement();
        // �󒆂ɂ���ꍇ�͉�������Spline���`�F�b�N
        if (!animController_.Grounded)
        {
            splineController_.CheckUnderSpline();
        }
    }

    private void HandleSplineMovement()
    {
        float currentT = splineController_.T;

        // Spline�͈͓��̏ꍇ
        if (currentT >= 0f && currentT <= 1f && !isOffSpline_)
        {
            // �ʏ��Spline�ړ�
            transform.rotation = splineController_.EvaluationInfo.rotation;

            Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();

            // Spline�̐��������̕ω��ʂ��v�Z
            Vector3 splineVerticalDelta = Vector3.zero;
            if (!isFirstFrame_)
            {
                Vector3 splineDelta = currentSplinePosition - previousSplinePosition_;
                splineVerticalDelta = new Vector3(0, splineDelta.y, 0);
            }

            // �W�����v�ɂ�鐂�������̈ړ��ʂ��v�Z
            Vector3 jumpVerticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;
            

            // ���݂̃v���C���[�ʒu���琅�������̐������擾
            Vector3 currentHorizontalPosition = new Vector3(currentSplinePosition.x, transform.position.y, currentSplinePosition.z);

            // �V�����ʒu = Spline�̐����ʒu + Spline�̐����ω� + �W�����v�̐����ړ�
            Vector3 newPosition = currentHorizontalPosition + splineVerticalDelta + jumpVerticalMovement;
            transform.position = newPosition;
            //rb_.MovePosition(newPosition);
        }
        // Spline�͈͊O�̏ꍇ
        else
        {
            //isOffSpline_ = true;
            HandleOffSplineMovement(currentT);
        }
    }

    private void HandleOffSplineMovement(float currentT)
    {
        Vector3 offSplineVerticalDelta = Vector3.zero;
        if (!isOffSpline_)
        {
            
            isOffSpline_ = true;
            // �Ō�̗L���ȃ^���W�F���g��ۑ�
            if (currentT > 1f)
            {
                //lastValidTangent_ = splineController_.GetEvaluationInfo(1f).tangent;
                lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 1f);
                lastValidDir = 1f;
            }
            else if (currentT < 0f)
            {
                lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 0);
                lastValidDir = -1f;
            }
            
            if (!isFirstFrame_)
            {
                Vector3 offSplineDelta = transform.position - previousOffSplinePosition_;
                offSplineVerticalDelta = new Vector3(0, offSplineDelta.y, 0);
            }
            // �������x��ݒ�i����������Spline�̃^���W�F���g�A���������͌��݂̐������x�j
            offSplineVelocity_ = new Vector3(transform.forward.x, 0, transform.forward.z);
            
            offSplineVelocity_ = offSplineVelocity_.normalized * (speed_);

            Debug.Log($"Off Spline! Last tangent");
            

        }
        // Spline�͈͊O�ł̈ړ�
        Vector3 horizontalMovement = new Vector3(offSplineVelocity_.x, 0, offSplineVelocity_.z);
        
        Vector3 verticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;

        transform.position += horizontalMovement + verticalMovement;
        // ��]���^���W�F���g�����ɐݒ�
        if (lastValidTangent_ != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lastValidDir * lastValidTangent_, Vector3.up);
        }

       
    }

    // SplineController����Ă΂�闎�����̐V����Spline��������
    public void OnFoundNewSpline(SplineContainer newSplineContainer)
    {
#if true
        if ( newSplineContainer != null)
        {
            Debug.Log($"Found new spline: {newSplineContainer.name}");
            isOffSpline_ = false;

            // �V����Spline�Ɉڍs
            splineController_.ChangeOtherSpline(newSplineContainer);

            // �ʒu��V����Spline�ɍ��킹�Ē���
            Vector3 newSplinePosition = splineController_.GetSplineMeshPos();
            transform.position = new Vector3(newSplinePosition.x, transform.position.y, newSplinePosition.z);

            previousSplinePosition_ = newSplinePosition;
        }
#endif
        //splineController_.ChangeOtherSpline(newSplineContainer);
        //isOffSpline_ = false;
    }

    public void CheckSpline(SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.Log("checksplineContainer==null");
            return;
        }
        if (splineController_.currentSplineContainer_ != splineContainer)
        {
            splineController_.ChangeOtherSpline(splineContainer);
        }
    }

   

    private void InputMovement()
    {
        if (!animController_.IsStunned)
        {
            if (inputs_.move.x == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if (inputs_.move.x == 1)
            {
                splineController_.isMovingLeft = false;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                animController_.TakeDamage();
                OnDamage(0, splineController_.T + 0.5f);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                clearZone_.ClearGame();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                animController_.Dying();
                OnTriggerDyingAnim();
            }
            if (inputs_.jump && animController_.Grounded)
            {
                animController_.AddVerticalForce(verticalForce_);
                //rb_.AddForce(new Vector3(0, verticalForce_, 0), ForceMode.Force);
            }
        }

        // Spline���t�X�V�i�͈͊O�ł��X�V�𑱂���j
        splineController_.UpdateT(speed_, (int)inputs_.move.x);

        // �A�j���[�V�����p�̓��͐ݒ�
        animController_.SetMoveInput(inputs_.move);

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
        // Spline�͈͊O�ړ��Ɉڍs���邽�߁A���N���X�̏����̓X�L�b�v
        Debug.Log($"{gameObject.name}: Reached Max T - transitioning to off-spline movement");
        //splineController_.MoveOtherSplineMinOrMax();

    }

    protected override void OnReachMinT()
    {
        // Spline�͈͊O�ړ��Ɉڍs���邽�߁A���N���X�̏����̓X�L�b�v
        Debug.Log($"{gameObject.name}: Reached Min T - transitioning to off-spline movement");
        //splineController_.MoveOtherSplineMinOrMax();
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
        animController_.TakeDamage();
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

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (animController_ != null && animController_.Grounded)
            Gizmos.color = transparentGreen;
        else
            Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawCube(transform.position + center_, halfExtends_);
    }
}
