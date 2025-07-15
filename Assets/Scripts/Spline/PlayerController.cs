using MySpline;
using StarterAssets;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineController))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : SplineMovementBase
{
    [SerializeField] float takeDamageInterval_ = 1.0f; // �_���[�W���󂯂�Ԋu
    [SerializeField] private bool canTakeDamage_ = true; // �_���[�W���󂯂��邩�ǂ���

    [SerializeField] private float knockbackLength_ = 5f; // �m�b�N�o�b�N���鋗��
    [SerializeField] private float knockbackForce = 0;
    //������
    [SerializeField] private float attenuationDelta_ = 1f;

    [SerializeField] AnimationController animController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer�ύX���m�p
    private SplineContainer previousSplineContainer_;
    [SerializeField] private LayerMask groundLayer_;
    private int knockbackDir_;
    [Header("�f�o�b�O�p")]
    [SerializeField] private ClearZone clearZone_;
    public Vector3 center_;
    public Vector3 halfExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;
    
    [SerializeField] private CapsuleCollider capsuleCollider_;

    // Spline�̐��������̕ω��ƃW�����v�𓝍����邽�߂̕ϐ�
    [SerializeField]private Vector3 previousSplinePosition_;
    private Vector3 previousOffSplinePosition_;
    private bool isFirstFrame_ = true;

    // Spline�͈͊O�ł̈ړ�����
    [SerializeField] private bool isOffSpline_ = false; // Spline�͈͊O�ɂ��邩�ǂ���
    [SerializeField] private Vector3 offSplineVelocity_; // Spline�͈͊O�ł̈ړ����x
    [SerializeField] private Vector3 lastValidTangent_; // �Ō�̗L���ȃ^���W�F���g
    private float lastValidDir = 0f;
    private bool isSmashed = false;
    [SerializeField] private SmashPlayer smashPlayer_;
    [SerializeField] private Rigidbody rb_;
    // �X�}�b�V����ԊǗ�
    [SerializeField] private bool isBeingSmashed_ = false;
    [SerializeField] private GameObject respawnPoint_;
    [SerializeField]
    Vector3 actualMovement;
    [SerializeField]Vector3 desiredMovement = Vector3.zero;
    Vector3 splineVerticalDelta = Vector3.zero;
    [SerializeField] Vector3 splineDelta;
    [SerializeField] private CharacterController characterController_;
    [SerializeField]Vector3 jumpVerticalMovement;
    [SerializeField] Vector3 newPosition;
    [SerializeField] Vector3 actualNoVerticalMovementPos;
    [SerializeField]Vector3 inputMovement;
    [SerializeField]Vector3 currentHorizontalPosition;
    [SerializeField]Vector3 knockbackMovement = Vector3.zero;
    public float T { get { return splineController_.T; } }
    protected override void Initialize()
    {
        if (animController_ == null)
        {
            animController_ = GetComponent<AnimationController>();
        }
        
        characterController_ = GetComponent<CharacterController>();
        splineController_.splineDirection_ = 1;

        // ����SplineContainer���L�^
        previousSplineContainer_ = splineController_.currentSplineContainer_;

        // ����Spline�ʒu���L�^
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
    }

    

    private void HandleSplineMovement()
    {
        float currentT = splineController_.T;
        desiredMovement = Vector3.zero;
        actualMovement = Vector3.zero;
        
        // Spline�͈͓��̏ꍇ
        if (currentT >= 0f && currentT <= 1f && !isOffSpline_)
        {
            // �ʏ��Spline�ړ�
            transform.rotation = splineController_.EvaluationInfo.rotation;

            Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();

            // Spline�̐��������̕ω��ʂ��v�Z�i�O�t���[���̈ʒu�Ɣ�r�j
            if (!isFirstFrame_)
            {
                splineDelta = currentSplinePosition - previousSplinePosition_;
                splineVerticalDelta = new Vector3(0, splineDelta.y, 0);
            }
            else
            {
                splineVerticalDelta = Vector3.zero;
                splineDelta = Vector3.zero;
            }

            // �W�����v�ɂ�鐂�������̈ړ��ʂ��v�Z
            jumpVerticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;

            // ���͂ɂ�鐅���ړ��ʂ��v�Z
            int inputDir = 0;
            if (!animController_.IsStunned && !isBeingSmashed_)
            {
                if (inputs_.move.x != 0)
                {
                    inputDir = (int)Mathf.Sign(inputs_.move.x);
                }
            }

            // �m�b�N�o�b�N�ɂ��ړ��ʂ��v�Z
            if (knockbackForce > 0)
            {
                Vector3 knockbackDirection = splineController_.EvaluationInfo.tangent.normalized;
                if (splineController_.isMovingLeft)
                {
                    knockbackDirection *= -1;
                }
                knockbackDirection *= knockbackDir_;
                knockbackMovement = knockbackDirection * knockbackForce * Time.deltaTime;
            }
            else
            {
                knockbackMovement = Vector3.zero;
            }

            // ���͂ɂ��ړ��ʂ��v�Z�iSpline�ɉ����������ړ��̂݁j
            inputMovement = Vector3.zero;
            if (inputDir != 0)
            {
                Vector3 inputDirection = splineController_.EvaluationInfo.tangent.normalized;
                if (splineController_.isMovingLeft)
                {
                    inputDirection *= -1;
                }
                inputDirection *= inputDir * splineController_.splineDirection_;
                inputMovement = inputDirection * speed_ * Time.deltaTime;
            }

            // ���ۂ̈ړ��v�Z���@��ύX
            // ���݈ʒu���x�[�X�ɁA�e�ړ����������Z
            Vector3 horizontalSplineMovement = new Vector3(splineDelta.x, 0, splineDelta.z);
            
            newPosition = transform.position + 
                         splineVerticalDelta + 
                         jumpVerticalMovement + 
                         inputMovement + 
                         knockbackMovement + 
                         horizontalSplineMovement;

            desiredMovement = newPosition - transform.position;
            
            // ���ɏ������ړ��ʂ͖���
            if (desiredMovement.magnitude <= 0.001f)
            {
                desiredMovement = Vector3.zero;
            }
            
            // CharacterController�ňړ�
            Vector3 startPos = transform.position;
            if (desiredMovement != Vector3.zero)
            {
                characterController_.Move(desiredMovement);
            }
            actualMovement = transform.position - jumpVerticalMovement - startPos;

            // Spline�ɉ������ړ��݂̂�t�l�X�V�i�����ړ������O�j
            //Vector3 horizontalActualMovement = new Vector3(actualMovement.x, 0, actualMovement.z);
            splineController_.UpdateTFromMovement(actualMovement);
        }
        // Spline�͈͊O�̏ꍇ
        else
        {
            desiredMovement = HandleOffSplineMovement(currentT);
            
            Vector3 startPos = transform.position;
            if(desiredMovement != Vector3.zero)
            {
                characterController_.Move(desiredMovement);
            }
            actualMovement = transform.position - startPos;
            
            // �I�tSpline���͎��ۂ̈ړ��ʂ�t�l�X�V
            splineController_.UpdateTFromMovement(actualMovement);
        }
    }

    private Vector3 HandleOffSplineMovement(float currentT)
    {
        Vector3 offSplineVerticalDelta = Vector3.zero;
        if (!isOffSpline_)
        {

            isOffSpline_ = true;
            // �Ō�̗L���ȃ^���W�F���g��ۑ�
            if (currentT > 1f)
            {
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

            offSplineVelocity_ = offSplineVelocity_.normalized * speed_ * Time.deltaTime;

            Debug.Log($"Off Spline! Last tangent");


        }
        // Spline�͈͊O�ł̈ړ�
        Vector3 horizontalMovement = new Vector3(offSplineVelocity_.x, 0, offSplineVelocity_.z);
        
        Vector3 verticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;

        // ��]���^���W�F���g�����ɐݒ�
        if (lastValidTangent_ != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lastValidDir * lastValidTangent_, Vector3.up);
        }

        return horizontalMovement + verticalMovement;
    }

    protected override void OnReachMaxT()
    {
        //lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 1f);
        //lastValidDir = 1f;
        //CalculateOffSplineVelocity_();
    }

    protected override void OnReachMinT()
    {
        //lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 0);
        //lastValidDir = -1f;
        //CalculateOffSplineVelocity_();
    }

    private void CalculateOffSplineVelocity_()
    {
        
        // �������x��ݒ�i����������Spline�̃^���W�F���g�A���������͌��݂̐������x�j
        offSplineVelocity_ = new Vector3(transform.forward.x, 0, transform.forward.z);

        offSplineVelocity_ = offSplineVelocity_.normalized * speed_ * Time.deltaTime;

        Debug.Log($"Off Spline! Last tangent");
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

            //�V����SplineMesh��Radius���擾
            splineController_.SetSplineMeshRadius();
            // �ʒu��V����Spline�ɍ��킹�Ē���
            Vector3 newSplinePosition = splineController_.GetSplineMeshPos();
            transform.position = new Vector3(newSplinePosition.x, transform.position.y, newSplinePosition.z);

            previousSplinePosition_ = newSplinePosition;
        }
#endif
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
        // �X�}�b�V�����͓��͂����S�ɖ�����
        if (isBeingSmashed_)
        {
            return;
        }

        int dir = 0;
        if (!animController_.IsStunned)
        {
            if(inputs_.move.x != 0)
            {
                dir = (int)Mathf.Sign(inputs_.move.x);
            }

            if(dir == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if(dir ==1)
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
            }
        }

        // �A�j���[�V�����p�̓��͐ݒ�
        animController_.SetMoveInput(inputs_.move);

        // �m�b�N�o�b�N�����̌���
        if (knockbackForce > 0)
        {
            knockbackForce -= attenuationDelta_ * Time.deltaTime;
            if (knockbackForce < 0)
            {
                knockbackForce = 0;
            }
        }
    }

    protected override void UpdateMovement()
    {
        // �X�}�b�V�����̓J�����X�V�̂ݒ�~�A���̏����͌p��
        if (isBeingSmashed_)
        {
            // �X�}�b�V�����͈ړ��������X�L�b�v
            return;
        }

        InputMovement();

        // �O�t���[���̈ʒu���ړ������O�ɍX�V
        if (!isOffSpline_)
        {
            previousSplinePosition_ = splineController_.GetSplineMeshPos();
        }
        else
        {
            previousOffSplinePosition_ = transform.position;
        }

        HandleSplineMovement();

        // �n�ʔ����AnimationController�ɔ��f
        animController_.Grounded = Physics.CheckBox(transform.position + center_, halfExtends_, transform.rotation, groundLayer_);
        
        isFirstFrame_ = false;

        CheckSplineContainerChange();
        UpdateCamera();

        Debug.DrawRay(transform.position, offSplineVelocity_ * 1000f);
        // �󒆂ɂ���ꍇ�͉�������Spline���`�F�b�N
        if (!animController_.Grounded)
        {
            splineController_.CheckUnderSpline();
        }
    }

    private void UpdateCamera()
    {
        // �X�}�b�V�����̓J�����X�V���~
        if (isBeingSmashed_)
        {
            return;
        }

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
        // �X�}�b�V������SplineContainer�ύX��������~
        if (isBeingSmashed_)
        {
            return;
        }

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
        knockbackForce = Mathf.Sqrt(knockbackLength_ * -2f * attenuationDelta_);
        StartCoroutine(WaitCanTakeDamage());
        if (hp_ <= 0)
        {
            //OnTriggerDyingAnim();
        }
    }

    public override void OnDamage(int damageValue, float enemyT)
    {
        knockbackDir_ = -(int)Mathf.Sign(enemyT - splineController_.T);
        OnDamage(damageValue);
        animController_.TakeDamage();
    }
    public override void OnDamage(int damageValue,Vector3 enemyPos)
    {
            float dot = Vector3.Dot(splineController_.EvaluationInfo.tangent.normalized, (enemyPos - transform.position).normalized);
        knockbackDir_ = -(int)Mathf.Sign(dot);
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
        if(!isSmashed)
        {
            StartCoroutine(DyingAnim());
        }
        Debug.Log("The player is dead. Probably.");
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

    public void OnSmash(GameObject respawnPoint)
    {
        respawnPoint_ = respawnPoint;
        isBeingSmashed_ = true; // �X�}�b�V����Ԃ��J�n
        
        // �J�����ɃX�}�b�V����Ԃ�ʒm
        if (cameraController_ != null)
        {
            cameraController_.SetPlayerSmashState(true);
        }
        
        StopAndReset();
        animController_.OnSmash();
        smashPlayer_.Smash(this.gameObject);
    }

    public void Respawn()
    {
        // �X�}�b�V����Ԃ��I��
        isBeingSmashed_ = false;
        
        // �J�����ɃX�}�b�V����ԏI����ʒm
        if (cameraController_ != null)
        {
            cameraController_.SetPlayerSmashState(false);
        }
        
        // ���͏�Ԃ�������
        inputs_.jump = false;
        inputs_.move = Vector2.zero;
        
        // ������Ԃ�������
        animController_.ResetVerticalVelocity();
        animController_.FinishSmash();
        
        // �m�b�N�o�b�N��Ԃ����Z�b�g
        knockbackForce = 0;
        
        // �I�t�X���C����Ԃ����Z�b�g
        isOffSpline_ = false;
        offSplineVelocity_ = Vector3.zero;
        
        // �e��t���O�����Z�b�g
        canTakeDamage_ = true;
        isSmashed = false;
        
        Enable();
        
        //����Ȃǂ��ēx�L��������
        //splineMovementBase�Ȃǂ��擾����t��evaluationInfo����...
        var respawnPointSpline = respawnPoint_.GetComponent<SplineController>();
        if (respawnPointSpline == null) return;

        splineController_.T = respawnPointSpline.T;
        
        // �ʒu�ƃt���[����Ԃ����Z�b�g
        isFirstFrame_ = true;
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
        transform.position = previousSplinePosition_;
        previousOffSplinePosition_ = transform.position;
    }

    /// <summary>
    /// �����Փ˔��蓙�S�Ė������A���Z�b�g����
    /// </summary>
    private void StopAndReset()
    {
        inputs_.jump = false;
        animController_.ResetVerticalVelocity();
        Disable();
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
        //Vector3 worldCenter = transform.TransformPoint(center_);
        Gizmos.DrawCube(transform.position + center_, halfExtends_);
    }
}
