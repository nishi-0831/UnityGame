using UnityEngine;
using UnityEngine.Splines;
using StarterAssets;

public class Enemy : SplineMovementBase, IPlayerInteractable
{
    [Header("Enemy Settings")]
    [SerializeField] private bool canBeStomped = true;
    [SerializeField] private int damageToPlayer = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        splineController_.isMovingLeft = false;
    }

    protected override void UpdateMovement()
    {
        splineController_.Move(speed_);
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
    }
}
