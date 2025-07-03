using NUnit.Framework;
using StarterAssets;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase, IPlayerInteractable
{
    [Header("����]�����čU�����Ă���G")]


    [Header("���̐ݒ�")]
    [SerializeField] private float attackInterval_ = 5.0f;
    [SerializeField] private float ballMoveSpeed_ = 0.1f;
    [SerializeField] private float ballRollSpeed_ = 360f;
    [SerializeField] private float ballOffset_ = 30.0f;


    [SerializeField] private GameObject ballPrefab_;
    [SerializeField] private float ballRadius_ = 0.5f;
    [SerializeField]private EaseInterpolator easeInterpolator_;

    [SerializeField] private bool canBeStomped = true;
    [SerializeField] private int damageToPlayer = 0;
    protected override void Initialize()
    {
        if(ballPrefab_ != null)
        {
            //ProBuilder��Sphere�v���~�e�B�u�̔��a�̓f�t�H���g�Œ��a1�Ȃ̂ŁA2�Ŋ����Ĕ��a���擾
            ballRadius_ = ballPrefab_.transform.localScale.x / 2f;
        }

        easeInterpolator_ = this.GetComponent<EaseInterpolator>();
        Debug.Assert(easeInterpolator_ != null);

        easeInterpolator_.onFinished_ += GenerateBall;
        easeInterpolator_.Reset();
        easeInterpolator_.duration = attackInterval_;
        //easeInterpolator_
    }

    protected override void UpdateMovement()
    {
        base.UpdateMovement();
        easeInterpolator_.UpdateTime();
    }
    private void GenerateBall()
    {
        easeInterpolator_.Reset();
        if(!IsActive_)
        {
            return;
        }
        Debug.Log($"{this.gameObject.name}:attack");
        GameObject ball = Instantiate(ballPrefab_);
        //ball.transform.position = this.transform.position;
        //���g���灛����ɒu������ : ������t�ł͂Ȃ�����
        //distance��n����t�̒l�ɕϊ��A���g��t�Ƒ���(�����ɂ��)

        float offsetT = splineController_.GetSplineMovementT(Mathf.Abs(ballOffset_));
        if(IsMovingLeft)
        {
            offsetT = -offsetT;
        }
        float ballT = splineController_.T + offsetT;

        Debug.Log($"{gameObject.name}:ballT = {ballT}");

        var ballMovement = ball.GetComponent<RollingBallSplineMovement>();
        Debug.Assert( ballMovement != null );

        
        ballMovement.SetParam(
            splineContainer: splineController_.currentSplineContainer_,
            t: ballT,
            moveSpeed: ballMoveSpeed_,
            rollSpeed: ballRollSpeed_,
            isLeft: IsMovingLeft
            );
    }

    public override void OnDamage()
    {
        base.OnDamage();
        // �G��|������
        Debug.Log($"{gameObject.name} was defeated!");
        Disable();
        Destroy(gameObject, 0.5f); // �����x�����č폜
    }

    // IPlayerInteractable����
    public bool OnStompedByPlayer(GameObject player)
    {
        if (!canBeStomped || !IsActive_)
            return false;

        Debug.Log($"{gameObject.name} was stomped by player!");
        OnDamage();

        // �v���C���[�ɒ��˕Ԃ���ʂ�^����
        var playerThirdPerson = player.GetComponent<ThirdPersonController>();
        if (playerThirdPerson != null)
        {
            playerThirdPerson.AddVerticalForce(5f); // �����W�����v������
        }

        return true; // ���݂�����
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        //�_���[�W�͗^���Ȃ�
#if false
        if (!IsActive_)
            return;

        Debug.Log($"{gameObject.name} damaged player!");

        // �v���C���[�Ƀ_���[�W��^���鏈��
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // �_���[�W�����������Ɏ���
            playerController.OnDamage(damageToPlayer);
            Debug.Log($"Player took {damageToPlayer} damage!");
        }
    
#endif
    }
}
