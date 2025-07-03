using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(SplineController))]
public class PlayerController : SplineMovementBase
{
    //[SerializeFIeld] private int hp_ = 3;
    [SerializeField] float takeDamageInterval_ = 1.0f; // �_���[�W���󂯂�Ԋu
    private bool canTakeDamage_ = true; // �_���[�W���󂯂��邩�ǂ���

    [SerializeField] private float knockbackLength_ = 5f; // �m�b�N�o�b�N���鋗��
    [SerializeField] private float knockbackForce = 0;
    //������
    [SerializeField] private float attenuationRate_ = 1f;

    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer�ύX���m�p
    private SplineContainer previousSplineContainer_;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        splineController_.splineDirection_ = 1;

        thirdPersonController_.myEvent = splineController_.CheckUnderSpline;

        // ����SplineContainer���L�^
        previousSplineContainer_ = splineController_.currentSplineContainer_;
    }

    // Update is called once per frame
    void Update()
    {
       
        
          
        InputMovement();
        transform.rotation = splineController_.EvaluationInfo.rotation;

        // ThirdPersonController�ɓn���ړ��ʂ��v�Z
        // Spline�̈ړ��� + Y���̏d��/�W�����v����
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        Vector3 verticalMovement = new Vector3(0, thirdPersonController_.VerticalVelocity * Time.deltaTime, 0);

        Vector3 totalMovement = splineMovement + verticalMovement;

        thirdPersonController_.Move(totalMovement);

        
        UpdateCamera();
            
        CheckSplineContainerChange();

    }

    private void InputMovement()
    {
        int inputAxis = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            splineController_.isMovingLeft = true;
            inputAxis = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            splineController_.isMovingLeft = false;
            inputAxis = 1;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            //thirdPersonController_.AddVerticalForce(verticalForce_);
            thirdPersonController_.TakeDamage();
            OnDamage(0);
        }
        if (!thirdPersonController_.IsStunned)
        {
            // Spline���t�X�V
            splineController_.UpdateT(speed_, inputAxis);
        }
        else
        {
            
        }
        // �A�j���[�V�����p�̓��͐ݒ�
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        moveInput.x = inputAxis;
        thirdPersonController_.SetMoveInput(moveInput);

        if(knockbackForce >0)
        {
            knockbackForce += attenuationRate_ * Time.deltaTime;
            splineController_.UpdateT(knockbackForce);
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
            //TransitionScene.Instance.ToGameOver();
        }
    }

    private IEnumerator WaitCanTakeDamage()
    {
        if (canTakeDamage_)
        {
            canTakeDamage_ = false;
            yield return new WaitForSeconds(takeDamageInterval_);
            canTakeDamage_ = true;
            //knockbackForce = 0;
        }
    }
}
