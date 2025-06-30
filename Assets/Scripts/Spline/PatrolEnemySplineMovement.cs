using UnityEngine;

public class PatrolEnemySplineMovement : SplineMovementBase
{
    [Header("Patrol Enemy Settings")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 4.0f;
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private bool canChasePlayer = true;
    
    private bool isChasing = false;
    private Transform playerTransform;
    
    protected override void Initialize()
    {
        base.Initialize();
        splineController_.isMovingLeft = false;
        
        // プレイヤーを検索
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    protected override void Update()
    {
        base.Update();
        if (canChasePlayer && playerTransform != null)
        {
            CheckPlayerDistance();
        }
        
        float currentSpeed = isChasing ? chaseSpeed : patrolSpeed;
        splineController_.Move(currentSpeed);
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        if (!isChasing)
        {
            HandlePatrolBounds();
        }
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        if (!isChasing)
        {
            HandlePatrolBounds();
        }
    }
    
    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool shouldChase = distance <= detectionRange;
        
        if (shouldChase != isChasing)
        {
            isChasing = shouldChase;
            Debug.Log($"{gameObject.name}: {(isChasing ? "Started chasing" : "Stopped chasing")} player");
        }
    }
    
    private void HandlePatrolBounds()
    {
        splineController_.Reverse();
        Debug.Log($"{gameObject.name}: Patrol direction reversed");
    }
    
    private void OnDrawGizmosSelected()
    {
        // 検出範囲を可視化
        Gizmos.color = isChasing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}