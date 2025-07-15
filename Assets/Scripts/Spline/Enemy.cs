using UnityEngine;
using UnityEngine.Splines;
using StarterAssets;

public class Enemy : SplineMovementBase, IPlayerInteractable
{
    [Header("Enemy Settings")]
    [SerializeField] private bool canBeStomped = true;
    [SerializeField] private int damageToPlayer = 1;


    [SerializeField] private Animator animator;
    [SerializeField] private float stompBounceForce = 5f;
    private int animIDDie;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        //splineController_.isMovingLeft = false;

        Animator animator = GetComponent<Animator>();
        animIDDie = Animator.StringToHash("Die");
    }

    protected override void UpdateMovement()
    {
        if(Input.GetKey(KeyCode.P))
        {
            OnDamage();
        }
        splineController_.Move(speed_);
    }

    protected override void OnCollideWall()
    {
        base.OnCollideWall();
        //Debug.Log("EnemyOnCollideWall");
        splineController_.Reverse();
    }

    // Update is called once per frame
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        splineController_.Reverse();
    }

    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        splineController_.Reverse();
    }

    public override void OnDamage()
    {
        base.OnDamage();
        // 敵を倒す処理
        Debug.Log($"{gameObject.name} was defeated!");
        Disable();

        animator?.SetTrigger(animIDDie);
    }

    public override void OnRequestDestroy()
    {
        Destroy(gameObject);
    }

    // IPlayerInteractable実装
    public bool OnStompedByPlayer(GameObject player)
    {
        if (!canBeStomped || !IsActive_)
            return false;

        Debug.Log($"{gameObject.name} was stomped by player!");
        OnDamage();

        // プレイヤーに跳ね返り効果を与える
        var playerAnimationController = player.GetComponent<AnimationController>();
        if (playerAnimationController != null)
        {
            playerAnimationController.AddVerticalForce(stompBounceForce); // 少しジャンプさせる
        }

        return true; // 踏みつけ成功
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        if (!IsActive_)
            return;

        Debug.Log($"{gameObject.name} damaged player!");

        // プレイヤーにダメージを与える処理
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // ダメージ処理をここに実装
            playerController.OnDamage(damageToPlayer,splineController_.T);
            Debug.Log($"Player took {damageToPlayer} damage!");
        }
    }
   
}
