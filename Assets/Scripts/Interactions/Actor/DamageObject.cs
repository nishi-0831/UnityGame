using UnityEngine;

public class DamageObject : MonoBehaviour, IPlayerInteractable
{
    PlayerInteractionProfile profile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStomped(GameObject player)
    {
        //PlayerInteractionUtils.ApplySideBounce(player, transform.position);
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }

    public void OnSideHit(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }
}
