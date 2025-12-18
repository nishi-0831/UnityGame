using UnityEngine;


public class PressMachine : PlayerInteractableBase
{
    [SerializeField] private LerpPingPong pingPong_;
    [SerializeField] GameObject respawnPoint_;
    [SerializeField] private Vector3 from_;
    [SerializeField] private Vector3 to_;
    [SerializeField] private float backDistance_;
    [SerializeField] private float forwardDistance_;

    [Header("デバッグ用")]
    [SerializeField] private Vector3 halfExtends_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (pingPong_ == null)
        {
            pingPong_ = GetComponent<LerpPingPong>();
        }
    }
    override protected void Start()
    {
        base.Start();
        SetLerpPos();
        pingPong_._from = from_;
        pingPong_._to = to_;
        pingPong_.StartPingPong();
    }
    //protected override void 
    public void SetLerpPos()
    {
        var info = splineController_.EvaluationInfo;

        // splineの接線の右方向をforwardにする
        Vector3 right = Vector3.Cross(info.upVector.normalized, info.tangent.normalized).normalized;
        FollowTarget.transform.rotation = Quaternion.LookRotation(right, Vector3.up);

        var rot = Quaternion.LookRotation(right, Vector3.up);

        to_ = info.position + FollowTarget.transform.forward * forwardDistance_;
        from_ = info.position + -FollowTarget.transform.forward * backDistance_;
    }

    public override void OnStompedCore(GameObject player)
    {
        if (pingPong_.CurrentState == MoveState.GOING)
        {
            PlayerInteractionUtils.GetPlayerController(player).OnSmash(respawnPoint_);
            PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        }
    }

    public override void OnSideHitCore(GameObject player)
    {
        if (pingPong_.CurrentState == MoveState.GOING)
        {
            PlayerInteractionUtils.GetPlayerController(player).OnSmash(respawnPoint_);
            PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = transparentGreen;
        Gizmos.DrawLine(from_, to_);
    }
}