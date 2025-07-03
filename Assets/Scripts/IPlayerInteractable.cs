using UnityEngine;

/// <summary>
/// プレイヤーとの相互作用を定義するインターフェース
/// </summary>
public interface IPlayerInteractable
{
    /// <summary>
    /// プレイヤーに踏みつけられた時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    /// <returns>踏みつけが成功したかどうか</returns>
    bool OnStompedByPlayer(GameObject player);
    
    /// <summary>
    /// プレイヤーと横から衝突した時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    void OnSideCollisionWithPlayer(GameObject player);
}