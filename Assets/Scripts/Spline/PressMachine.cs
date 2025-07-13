using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PressMachine : SplineMovementBase, IPlayerInteractable
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
    void Start()
    {
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
        Debug.Log(rot.eulerAngles);

        to_ = info.position + FollowTarget.transform.forward * forwardDistance_;
        from_ = info.position + -FollowTarget.transform.forward * backDistance_;
    }



    public bool OnStompedByPlayer(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.OnSmash(respawnPoint_);
        }

        return true;
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.OnSmash(respawnPoint_);
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
