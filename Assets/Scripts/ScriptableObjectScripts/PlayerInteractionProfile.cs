using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionProfile", menuName = "Scriptable Objects/PlayerInteractionProfile")]
public class PlayerInteractionProfile : ScriptableObject
{
    public bool canBeStomped = true;
    public int damageToPlayer = 1;
    public float stompBounceForce = 5f;
    //public float sideHitKnockbackForce = 0f;
}