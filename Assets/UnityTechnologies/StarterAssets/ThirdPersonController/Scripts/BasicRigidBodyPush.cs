using UnityEngine;

public class BasicRigidBodyPush : MonoBehaviour
{
	public LayerMask hitLayers;
    
    public bool canPush;
	[Range(0.5f, 5f)] public float strength = 1.1f;

	//private void OnControllerColliderHit(ControllerColliderHit hit)
	//{
 //       //if (canPush) PushRigidBodies(hit);
 //       var bodyLayerMask = 1 << hit.gameObject.activeLayer_;
 //       if ((bodyLayerMask & hitLayers.value) == 0) return;
 //       Debug.Log($"{hit.gameObject.name}と{this.name}が衝突しました");
 //   }
    private void OnTriggerEnter(Collider other)
    {
        var bodyLayerMask = 1 << other.gameObject.layer;
        if ((bodyLayerMask & hitLayers.value) == 0) return;
        
		Debug.Log($"{other.gameObject.name}と{this.name}が衝突しました");

		if(other.transform.position.y < transform.position.y)
		{
			Debug.Log($"{other.gameObject.name}を踏みつけ成功");
			SplineMovementBase splineMovementBase = other.gameObject.GetComponent<SplineMovementBase>();
			if( splineMovementBase != null )
			{
				splineMovementBase.OnDamage();
			}
		}
    }
    private void PushRigidBodies(ControllerColliderHit hit)
	{
		// https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

		// make sure we hit a non kinematic rigidbody
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		// make sure we only push desired activeLayer_(s)
		var bodyLayerMask = 1 << body.gameObject.layer;
		if ((bodyLayerMask & hitLayers.value) == 0) return;

		// We dont want to push objects below us
		if (hit.moveDirection.y < -0.3f) return;

		// Calculate push direction from move direction, horizontal motion only
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

		// Apply the push and take strength into account
		body.AddForce(pushDir * strength, ForceMode.Impulse);
	}
}