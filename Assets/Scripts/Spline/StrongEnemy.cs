using NUnit.Framework;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
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
        float ballT = splineController_.t_ + offsetT;

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
}
