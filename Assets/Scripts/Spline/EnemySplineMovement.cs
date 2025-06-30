using UnityEngine;

public class EnemySplineMovement : SplineMovementBase
{
    [Header("Enemy Settings")]
    [SerializeField] private bool reverseOnBounds = true;

    protected override void Initialize()
    {
        base.Initialize();
        splineController_.isMovingLeft = false;
    }
    
    protected override void Update()
    {
        base.Update();
        splineController_.Move(speed_);
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        if (reverseOnBounds)
        {
            HandleBoundsReached();
        }
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        if (reverseOnBounds)
        {
            HandleBoundsReached();
        }
    }
    
    private void HandleBoundsReached()
    {
        splineController_.Reverse();
        Debug.Log($"{gameObject.name}: Direction reversed");
    }
}