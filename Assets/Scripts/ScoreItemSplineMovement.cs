using UnityEngine;

public class ScoreItemSplineMovement : SplineMovementBase
{
    [Header("Score Item Settings")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private bool destroyOnBounds = true;
    [SerializeField] private float oscillationSpeed = 2.0f;
    [SerializeField] private float oscillationAmount = 0.5f;
    
    private float initialY;
   
    protected override void Initialize()
    {
        initialY = followTarget_.transform.position.y;
        splineController_.isMovingLeft = false;
    }
    protected override void UpdateMovement()
    {
        // Y���̐U���G�t�F�N�g
        Vector3 pos = followTarget_.transform.position;
        pos.y = initialY + Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmount;
        followTarget_.transform.position = pos;
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        if (destroyOnBounds)
        {
            DestroyItem();
        }
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        if (destroyOnBounds)
        {
            DestroyItem();
        }
    }
    
    private void DestroyItem()
    {
        Debug.Log($"{gameObject.name}: Score item destroyed at bounds");
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �X�R�A���Z�����i���ۂ̃Q�[���ł�ScoreManager�Ȃǂ��g�p�j
            Debug.Log($"Score +{scoreValue}");
            Destroy(gameObject);
        }
    }
}