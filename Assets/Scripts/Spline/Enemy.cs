using UnityEngine;
using UnityEngine.Splines;

public class Enemy : SplineMovementBase
{
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
        //Reverse();
        splineController_.Reverse();
    }
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        splineController_.Reverse();
    }
    public override void OnDamage()
    {
        base .OnDamage();
    }
}
