using UnityEngine;

/// <summary>
/// Hammerの説明をここに記述
/// </summary>
public class Hammer : PlayerInteractableBase
{
    //ハンマーのY座標は20～22(今回は21で設定)
    //[SerializeField] 
    [SerializeField] GameObject respawnPoint_;

    // 独自のフィールドをここに追加

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        // 初期化処理をここに記述
        transform.position = transform.localPosition;
    }

    /// <summary>
    /// 開始処理（MonoBehaviourのStart相当）
    /// </summary>
    override protected void Start()
    {
        base.Start();
        // 開始処理をここに記述
    }

    /// <summary>
    /// 更新処理（MonoBehaviourのUpdate相当）
    /// </summary>
    void Update()
    {
        // 更新処理をここに記述
        transform.Rotate(0, 0, 0.3f);
    }

    /// <summary>
    /// 移動処理の更新
    /// </summary>
    protected override void UpdateMovement()
    {
        // 移動処理をここに記述
        // 例:
        // splineController_.Move(speed_);
    }

    /// <summary>
    /// 壁との衝突処理
    /// </summary>
    protected override void OnCollideWall()
    {
        // 壁衝突時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// スプライン終端到達時の処理
    /// </summary>
    protected override void OnReachMaxT()
    {
        // スプライン終端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// スプライン始端到達時の処理
    /// </summary>
    protected override void OnReachMinT()
    {
        // スプライン始端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// プレイヤーに踏みつけられた時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnStompedCore(GameObject player)
    {
        // 踏みつけ時の処理をここに記述
        // 例: 
        // OnDamage();
        // PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
    }

    /// <summary>
    /// プレイヤーと横から衝突した時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnSideHitCore(GameObject player)
    {
        PlayerInteractionUtils.GetPlayerController(player).OnSmash(respawnPoint_);
    }
}
