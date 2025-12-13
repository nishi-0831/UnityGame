using System.Collections;
using UnityEngine;

public class PressMachine : PlayerInteractableBase
{
    [Header("References")]
    [SerializeField] private LerpPingPong pingPong_;   // プレスの往復移動制御
    [SerializeField] private GameObject respawnPoint_; // プレイヤーのリスポーン地点

    // 移動距離
    [Header("Press Distance")]
    [SerializeField] private float backDistance_ = 0.5f;     // 後ろ方向の移動距離
    [SerializeField] private float forwardDistance_ = 0.5f;  // 前方向の移動距離

    // タイミング設定
    [Header("Timing")]
    [SerializeField] private float waitTime_ = 3f;      // 次のプレスまでの待機時間
    [SerializeField] private float blinkTime_ = 0.6f;   // 点滅している時間
    [SerializeField] private float blinkInterval_ = 0.12f; // 点滅の速さ

    // 点滅の見た目
    [Header("Blink Visual")]
    [SerializeField]
    private Color emissionColor_ =
        new Color(1f, 1f, 0f) * 3f; // HDR 黄色発光

    // 内部変数
    private Vector3 from_;     // プレスの戻り位置
    private Vector3 to_;       // プレスの押し出し位置
    private Material material_; // 発光制御用マテリアル

    // 初期化
    protected override void Start()
    {
        base.Start();

        // 全プレス機の開始フレームを揃えるため Coroutine 開始
        StartCoroutine(InitializeAndStart());
    }

    // 初期化後、プレスのループ処理を開始
    private IEnumerator InitializeAndStart()
    {
        // 1フレーム待つことで複数プレス機を完全同期
        yield return null;

        // LerpPingPong が未設定なら自動取得
        if (!pingPong_)
            pingPong_ = GetComponent<LerpPingPong>();

        // 発光制御用マテリアル取得
        var renderer = FollowTarget.GetComponent<Renderer>();
        material_ = renderer.material;
        material_.EnableKeyword("_EMISSION");
        material_.SetColor("_EmissionColor", Color.black);

        // プレス方向と移動範囲を決定
        SetupPressDirection();

        // LerpPingPong に移動範囲を設定
        pingPong_._from = from_;
        pingPong_._to = to_;

        // メインループ開始
        StartCoroutine(PressLoop());
    }

    // スプライン情報からプレス方向と移動範囲を設定
    private void SetupPressDirection()
    {
        var info = splineController_.EvaluationInfo;

        // スプラインの接線と上方向から右方向を計算
        Vector3 right =
            Vector3.Cross(info.upVector.normalized, info.tangent.normalized).normalized;

        // プレス機の向きを設定
        FollowTarget.transform.rotation =
            Quaternion.LookRotation(right, Vector3.up);

        // 前後の移動範囲を設定
        to_ = info.position + FollowTarget.transform.forward * forwardDistance_;
        from_ = info.position - FollowTarget.transform.forward * backDistance_;

        // 初期位置を中央に
        FollowTarget.transform.position = info.position;
    }

    // プレスのメインループ
    private IEnumerator PressLoop()
    {
        while (true)
        {
            // 待機（点滅前）
            yield return new WaitForSeconds(waitTime_ - blinkTime_);

            // 予兆点滅
            yield return Blink();

            // プレス開始
            pingPong_.StartPingPong();

            // プレス動作が終わるまで待つ
            yield return new WaitUntil(() =>
                pingPong_.CurrentState == MoveState.WAIT);
        }
    }

    // 発光をON/OFFして点滅させる
    private IEnumerator Blink()
    {
        float t = 0f;
        bool on = false;

        while (t < blinkTime_)
        {
            // ON / OFF を切り替え
            on = !on;

            material_.SetColor(
                "_EmissionColor",
                on ? emissionColor_ : Color.black
            );

            yield return new WaitForSeconds(blinkInterval_);
            t += blinkInterval_;
        }

        // 念のため発光を消す
        material_.SetColor("_EmissionColor", Color.black);
    }

    // プレイヤー接触判定
    public override void OnStompedCore(GameObject player)
    {
        // プレス中のみダメージ
        if (pingPong_.CurrentState == MoveState.GOING)
        {
            PlayerInteractionUtils.GetPlayerController(player)
                .OnSmash(respawnPoint_);

            PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        }
    }

    public override void OnSideHitCore(GameObject player)
    {
        // 横から当たってもプレス中ならダメージ
        if (pingPong_.CurrentState == MoveState.GOING)
        {
            PlayerInteractionUtils.GetPlayerController(player)
                .OnSmash(respawnPoint_);

            PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        }
    }
}
