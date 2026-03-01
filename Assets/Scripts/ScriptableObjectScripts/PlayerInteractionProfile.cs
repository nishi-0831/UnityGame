using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionProfile", menuName = "Scriptable Objects/PlayerInteractionProfile")]
public class PlayerInteractionProfile : ScriptableObject
{
    [Header("プレイヤーに踏みつけられるかどうか")]
    public bool canBeStomped = true;

    [Header("プレイヤーに与えるダメージ量")]
    public int damageToPlayer = 1;

    [Header("踏みつけ時にプレイヤーに加えられる+Y方向の力")]
    public float stompBounceForce = 5f;

    [Header("撃破時のスコア加算値")]
    public int scoreValue = 100;
}