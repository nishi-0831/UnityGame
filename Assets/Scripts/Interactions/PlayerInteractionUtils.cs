using UnityEngine;

/// <summary>
/// プレイヤーとインタラクションする際のユーティリティ関数群
/// コントローラー取得やダメージ処理、バウンド処理を提供
/// </summary>
public static class PlayerInteractionUtils
{
    /// <summary>
    /// GameObjectからPlayerControllerを取得
    /// 取得できない場合は nullを返す
    /// </summary>
    public static PlayerController GetPlayerController(GameObject player)
        => player.TryGetComponent(out PlayerController pc) ? pc : null;

    /// <summary>
    /// GameObjectからAnimationControllerを取得
    /// 取得できない場合は nullを返す
    /// </summary>
    public static AnimationController GetAnimationController(GameObject player)
        => player.TryGetComponent(out AnimationController ac) ? ac : null;

    /// <summary>
    /// プレイヤーにダメージを与える
    /// </summary>
    public static void ApplyDamage(GameObject player, int damage)
    {
        PlayerController pc = GetPlayerController(player);
        if (pc != null) pc.OnDamage(damage);
    }

    /// <summary>
    /// プレイヤーに踏みつけバウンドを適用
    /// </summary>
    public static void ApplyStompBounce(GameObject player, float force)
    {
        AnimationController ac = GetAnimationController(player);
        if (ac != null) ac.AddVerticalForce(force);
    }

    public static void ApplySideBounce(GameObject player,float progress)
    {
        PlayerController pc = GetPlayerController(player);
        if (pc != null) pc.SideBounce(progress);
    }
    public static void ApplySideBounce(GameObject player,Vector3 pos)
    {
        PlayerController pc = GetPlayerController(player);
        if (pc != null) pc.SideBounce(pos);
    }
}