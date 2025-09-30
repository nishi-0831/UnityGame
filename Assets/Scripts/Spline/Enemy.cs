using UnityEngine;
using UnityEngine.Splines;
using StarterAssets;

public class Enemy : PlayerInteractableBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void UpdateMovement()
    {
        splineController_.Move(speed_);
    }

    protected override void OnCollideWall()
    {
        splineController_.Reverse();
    }

    // Update is called once per frame
    protected override void OnReachMaxT()
    {
        splineController_.Reverse();
    }

    protected override void OnReachMinT()
    {
        splineController_.Reverse();
    }

   

    // IPlayerInteractableé¿ëï
    public override void OnStompedCore(GameObject player)
    {
        OnDamage();

        // ÉvÉåÉCÉÑÅ[Ç…íµÇÀï‘ÇËå¯â Çó^Ç¶ÇÈ
        PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
    }

    public override void OnSideHitCore(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        PlayerInteractionUtils.ApplySideBounce(player, T);
    }
   
}
