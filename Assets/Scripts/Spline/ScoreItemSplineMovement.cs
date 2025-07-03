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
        // Y軸の振動エフェクト
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
            // スコア加算処理（実際のゲームではScoreManagerなどを使用）
            GiveScoreToPlayer(other.gameObject);
        }
    }
    
    private void GiveScoreToPlayer(GameObject player)
    {
        Debug.Log($"Score +{scoreValue}");
        // ここでScoreManagerに通知する処理を追加
        Destroy(gameObject);
    }
    
    // IPlayerInteractable実装
    public bool OnStompedByPlayer(GameObject player)
    {
        // スコアアイテムは踏みつけでも普通の取得と同じ
        GiveScoreToPlayer(player);
        return false; // 踏みつけエフェクトは不要
    }
    
    public void OnSideCollisionWithPlayer(GameObject player)
    {
        // 横からの衝突でも取得
        GiveScoreToPlayer(player);
    }
}