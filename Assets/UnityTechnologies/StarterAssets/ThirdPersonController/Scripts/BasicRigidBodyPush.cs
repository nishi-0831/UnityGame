using UnityEngine;
using UnityEngine.Splines;

public class BasicRigidBodyPush : MonoBehaviour
{
    [SerializeField] private SplineLayerSettings splineLayerSettings;
    //public LayerMask hitLayer;
    //public LayerMask groundLayer;
    public bool canPush;
    [Range(0.5f, 5f)] public float strength = 1.1f;
    [Range(0.0f, 2.0f)] public float stompThreshold = 0.5f; // 踏みつけ判定の閾値

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"{other.gameObject.name}と{this.name}が衝突しました");
        var bodyLayerMask = 1 << other.gameObject.layer;
        if ((bodyLayerMask & splineLayerSettings.activeLayer.value) == 0) return;
        
        // プレイヤーとSplineMovementBaseオブジェクトの相互作用を処理
        IPlayerInteractable interactable = other.gameObject.GetComponent<IPlayerInteractable>();
        if (interactable != null)
        {
            HandlePlayerInteraction(other, interactable);
        }
        else
        {
            // 従来の押し処理など
            Debug.Log($"No interaction interface found on {other.gameObject.name}");
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }
    private void HandlePlayerInteraction(Collider other, IPlayerInteractable interactable)
    {
        Debug.Log($"{transform.position.y} > {other.transform.position.y}");
        // 踏みつけ判定：プレイヤーが相手より上にいるかチェック
        bool isStomping = transform.position.y > other.transform.position.y + stompThreshold;
        
        //// プレイヤーの速度も考慮（下向きに移動中かチェック）
        //var playerRigidbody = GetComponent<Rigidbody>();
        //bool isMovingDown = playerRigidbody != null && playerRigidbody.linearVelocity.y < -1f;
        
        if (isStomping)
        {
            // 踏みつけ処理
            bool stompSuccessful = interactable.OnStompedByPlayer(this.gameObject);
            if (stompSuccessful)
            {
                Debug.Log($"踏みつけ成功: {other.gameObject.name}");

            }
        }
        else
        {
            // 横からの衝突処理
            interactable.OnSideCollisionWithPlayer(this.gameObject);
        }
    }

    private void PushRigidBodies(ControllerColliderHit hit)
    {
        // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

        // make sure we hit a non kinematic rigidbody
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // make sure we only push desired layers
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & splineLayerSettings.activeLayer.value) == 0) return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f) return;

        // Calculate push direction from move direction, horizontal motion only
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        // Apply the push and take strength into account
        body.AddForce(pushDir * strength, ForceMode.Impulse);
    }
}