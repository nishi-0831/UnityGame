using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("�J����")]
    private Camera camera_;

    [Header("�����_")]
    [SerializeField] private Transform target_;

    [Header("����")]
    [SerializeField] private Vector3 distance_;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera_ = GetComponent<Camera>();
        if(camera_ == null )
        {
            Debug.LogError("camera_ is NULL");
        }
    }

    // Update is called once per frame
    void Update()
    {
        camera_.transform.position = target_.position - distance_;
    }
}
