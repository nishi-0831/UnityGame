using UnityEngine;
using UnityEngine.Splines;
using StarterAssets;

public class Enemy : PlayerInteractableBase
{

    [SerializeField] private AudioClip enemyDieClip;
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
        if(splineController_.currentSplineContainer_.Spline.Closed)
        {
            splineController_.Loop();
        }
        else
        {
            splineController_.Reverse();
        }
    }

    protected override void OnReachMinT()
    {
        if (splineController_.currentSplineContainer_.Spline.Closed)
        {
            splineController_.Loop();
        }
        else
        {
            splineController_.Reverse();
        }
    }

    // IPlayerInteractableé¿ëï
    public override void OnStompedCore(GameObject player)
    {

        if (AudioManager.Instance != null && enemyDieClip != null)
        {
            AudioManager.Instance.PlaySound(enemyDieClip, 0.3f);
        }
        OnDamage();//ìGÇì|ÇµÇΩÇ∆Ç´Å@Ç±Ç±Ç…âπÇâ¡Ç¶ÇΩÇ¢

        // ÉvÉåÉCÉÑÅ[Ç…íµÇÀï‘ÇËå¯â Çó^Ç¶ÇÈ
        PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
    }

    public override void OnSideHitCore(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        PlayerInteractionUtils.ApplySideBounce(player, Progress);
    }
   
}
