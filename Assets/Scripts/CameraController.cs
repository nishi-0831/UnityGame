using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("ƒJƒƒ‰")]
    private Camera camera_;

    [Header("’‹“_")]
    [SerializeField] private Transform target_;

    [Header("‹——£")]
    [SerializeField] private float distance_;

    [Header("’‹“_‚ÌŒü‚«")]
    public bool isMovingLeft_ { get;  set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera_ = GetComponent<Camera>();
        if(camera_ == null )
        {
            Debug.LogError("camera_ is NULL");
        }
        camera_.transform.position = target_.position + (target_.right * distance_);
        camera_.transform.LookAt(target_.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        Vector3 dir = target_.right;
        if( isMovingLeft_ )
        {
            dir *= -1;
        }
        camera_.transform.position = target_.position + (dir * distance_);
        camera_.transform.LookAt(target_.position);
    }
}
