using Benjathemaker;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent (typeof(SimpleGemsAnim))]
public class ScoreItemSplineMovement : SplineMovementBase, IPlayerInteractable
{
    [Header("Score Item Settings")]
    [SerializeField] private int scoreValue = 100;
    
    [SerializeField] private float animDuration = 1.0f;
    [SerializeField] private float jumpHeight = 5.0f;
    [SerializeField] private float initialOffsetY = 1.0f;
    [SerializeField] private float endPosOffsetY = 1.0f;
    [SerializeField] private SimpleGemsAnim simpleGemsAnim;
    

    protected override void Initialize()
    {
        FollowTarget.transform.position = FollowTarget.transform.position + new Vector3(0f, initialOffsetY, 0f);

        simpleGemsAnim = FollowTarget.GetComponent<SimpleGemsAnim>();
        simpleGemsAnim.Initialize(FollowTarget);
        
        splineController_.isMovingLeft = false;
    }
    
    protected override void UpdateMovement()
    {
        simpleGemsAnim.UpdateRot();
        simpleGemsAnim.UpdatePos();
        simpleGemsAnim.UpdateScale();
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        splineController_.Reverse();
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        splineController_.Reverse();
    }

    private void DestroyItem()
    {
        //Vector3 playerPos = player.transform.position;
        //float verticalVelocity = Mathf.Sqrt(height * -2f * Gravity)
    }
    private IEnumerator DestroyAnim(GameObject player)
    {
        Disable();

        simpleGemsAnim.rotationSpeed = 1080; 

        Vector3 startPos = FollowTarget.transform.position;
        
        float elapsed = 0f;
        while(elapsed < animDuration)
        {
            //回転
            simpleGemsAnim.UpdateRot();

            //座標
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            Vector3 endPos = player.transform.position + new Vector3(0,endPosOffsetY,0);

            Vector3 holizontal = Vector3.Lerp(startPos, endPos, t);

            float vertical = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = new Vector3(holizontal.x,holizontal.y + vertical, holizontal.z);

            yield return null;
        }
        Debug.Log($"Score +{scoreValue}");
        // ここでScoreManagerに通知する処理を追加
        ScoreManager.Instance.ReceiveScore(scoreValue);
        
        
        Destroy(this.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // スコア加算処理（実際のゲームではScoreManagerなどを使用）
            GiveScoreToPlayer(other.gameObject);
        }
    }
    
    
    private void GiveScoreToPlayer(GameObject player)
    {
        StartCoroutine(DestroyAnim(player));
    }
    
    // IPlayerInteractable実装
    public bool OnStompedByPlayer(GameObject player)
    {
        // スコアアイテムは踏みつけでも普通の取得と同じ
        GiveScoreToPlayer(player);
        return false; // 踏みつけエフェクトは不要
    }
    
    public void OnSideCollisionWithPlayer(GameObject player)
    {
        // 横からの衝突でも取得
        GiveScoreToPlayer(player);
    }
}