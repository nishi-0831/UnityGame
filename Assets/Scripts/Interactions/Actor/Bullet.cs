using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

/// <summary>
/// Bulletの説明をここに記述
/// </summary>
public class Bullet : MonoBehaviour, IPlayerInteractable
{
    public PlayerInteractionProfile profile;
    // 独自のフィールドをここに追加
    void IPlayerInteractable.OnSideHit(GameObject player)
    {
        Debug.Log(player.name);
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }
    void IPlayerInteractable.OnStomped(GameObject player)
    {
        Debug.Log(player.name);
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }
    /// <summary>
    /// 初期化処理
    /// </summary>

    /// <summary>
    /// 更新処理（MonoBehaviourのUpdate相当）
    /// </summary>
    void Update()
    {
       
    }

}
