using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 対象となるオブジェクトを別の場所へ移動させる
/// </summary>
public class LaneChangeZone : PlayerInteractableBase
{
    // 独自のフィールドをここに追加    
    [Header("移動先")]
    [SerializeField] GameObject changeDestination;
    SplineController changeController;
    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        // 初期化処理をここに記述
        speed_ = 0f;
        // 移動先の GameObjectからSplineContainerを取得
        changeDestination.TryGetComponent<SplineController>(out changeController);
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
    }

    /// <summary>
    /// プレイヤーに踏みつけられた時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnStompedCore(GameObject player)
    {
        Debug.Log("LaneChangeZone");
        // 踏みつけ時の処理をここに記述
        var playerController = PlayerInteractionUtils.GetPlayerController(player);
        if (playerController != null)
        {
            playerController.splineController_.SyncToSpline(changeController);
        }
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
    /// プレイヤーと横から衝突した時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnSideHitCore(GameObject player)
    {
        // 横衝突時の処理をここに記述
        // 例:
        // PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
    }
}
