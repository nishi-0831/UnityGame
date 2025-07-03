using UnityEngine;

public class ScoreItemSplineMovement : SplineMovementBase, IPlayerInteractable
{
    [Header("Score Item Settings")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private bool destroyOnBounds = true;
    [SerializeField] private float oscillationSpeed = 2.0f;
    [SerializeField] private float oscillationAmount = 0.5f;
    
    private float initialY;
   
    protected override void Initialize()
    {
        initialY = FollowTarget.transform.position.y;
        splineController_.isMovingLeft = false;
    }
    
    protected override void UpdateMovement()
    {
        // Y���̐U���G�t�F�N�g
        Vector3 pos = FollowTarget.transform.position;
        pos.y = initialY + Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmount;
        FollowTarget.transform.position = pos;
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        if (destroyOnBounds)
        {
            DestroyItem();
        }
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        if (destroyOnBounds)
        {
            DestroyItem();
        }
    }
    
    private void DestroyItem()
    {
        Debug.Log($"{gameObject.name}: Score item destroyed at bounds");
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �X�R�A���Z�����i���ۂ̃Q�[���ł�ScoreManager�Ȃǂ��g�p�j
            GiveScoreToPlayer(other.gameObject);
        }
    }
    
    private void GiveScoreToPlayer(GameObject player)
    {
        Debug.Log($"Score +{scoreValue}");
        // ������ScoreManager�ɒʒm���鏈����ǉ�
        Destroy(gameObject);
    }
    
    // IPlayerInteractable����
    public bool OnStompedByPlayer(GameObject player)
    {
        // �X�R�A�A�C�e���͓��݂��ł����ʂ̎擾�Ɠ���
        GiveScoreToPlayer(player);
        return false; // ���݂��G�t�F�N�g�͕s�v
    }
    
    public void OnSideCollisionWithPlayer(GameObject player)
    {
        // ������̏Փ˂ł��擾
        GiveScoreToPlayer(player);
    }
}