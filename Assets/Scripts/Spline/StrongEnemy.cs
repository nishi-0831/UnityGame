using NUnit.Framework;
using StarterAssets;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase, IPlayerInteractable
{
    [Header("球を転がして攻撃してくる敵")]


    [Header("球の設定")]
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
            //ProBuilderのSphereプリミティブの半径はデフォルトで直径1なので、2で割って半径を取得
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
        //自身から○○先に置きたい : ○○はtではなく距離
        //distanceを渡してtの値に変換、自身のtと増減(向きによる)

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
        // 敵を倒す処理
        Debug.Log($"{gameObject.name} was defeated!");
        Disable();
        Destroy(gameObject, 0.5f); // 少し遅延して削除
    }

    // IPlayerInteractable実装
    public bool OnStompedByPlayer(GameObject player)
    {
        if (!canBeStomped || !IsActive_)
            return false;

        Debug.Log($"{gameObject.name} was stomped by player!");
        OnDamage();

        // プレイヤーに跳ね返り効果を与える
        var playerThirdPerson = player.GetComponent<ThirdPersonController>();
        if (playerThirdPerson != null)
        {
            playerThirdPerson.AddVerticalForce(5f); // 少しジャンプさせる
        }

        return true; // 踏みつけ成功
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        //ダメージは与えない
#if false
        if (!IsActive_)
            return;

        Debug.Log($"{gameObject.name} damaged player!");

        // プレイヤーにダメージを与える処理
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // ダメージ処理をここに実装
            playerController.OnDamage(damageToPlayer);
            Debug.Log($"Player took {damageToPlayer} damage!");
        }
    
#endif
    }
}
