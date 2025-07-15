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
        // �G��|������
        Debug.Log($"{gameObject.name} was defeated!");
        Disable();

        animator?.SetTrigger(animIDDie);
    }

    public override void OnRequestDestroy()
    {
        Destroy(gameObject);
    }

    // IPlayerInteractable����
    public bool OnStompedByPlayer(GameObject player)
    {
        if (!canBeStomped || !IsActive_)
            return false;

        Debug.Log($"{gameObject.name} was stomped by player!");
        OnDamage();

        // �v���C���[�ɒ��˕Ԃ���ʂ�^����
        var playerAnimationController = player.GetComponent<AnimationController>();
        if (playerAnimationController != null)
        {
            playerAnimationController.AddVerticalForce(stompBounceForce); // �����W�����v������
        }

        return true; // ���݂�����
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        if (!IsActive_)
            return;

        Debug.Log($"{gameObject.name} damaged player!");

        // �v���C���[�Ƀ_���[�W��^���鏈��
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // �_���[�W�����������Ɏ���
            playerController.OnDamage(damageToPlayer,splineController_.T);
            Debug.Log($"Player took {damageToPlayer} damage!");
        }
    }
   
}
