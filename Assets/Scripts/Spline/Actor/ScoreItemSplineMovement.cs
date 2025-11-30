using Benjathemaker;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent (typeof(SimpleGemsAnim))]
public class ScoreItemSplineMovement : PlayerInteractableBase
{
    [Header("Score Item Settings")]
    //[SerializeField] private int scoreValue = 100;
    
    [SerializeField] private float animDuration = 1.0f;
    [SerializeField] private float jumpHeight = 5.0f;
    [SerializeField] private float initialOffsetY = 1.0f;
    [SerializeField] private float endPosOffsetY = 1.0f;
    [SerializeField] private SimpleGemsAnim simpleGemsAnim;
    [SerializeField] private bool animateSpline = false;
    protected override void Initialize()
    {
        FollowTarget.transform.position = FollowTarget.transform.position + new Vector3(0f, initialOffsetY, 0f);

        simpleGemsAnim = FollowTarget.GetComponent<SimpleGemsAnim>();
        simpleGemsAnim.Initialize(FollowTarget);
        
        splineController_.isMovingLeft = false;
    }
    
    protected override void UpdateMovement()
    {
        if (animateSpline)
            return;
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

            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);

            float vertical = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = new Vector3(horizontal.x,horizontal.y + vertical, horizontal.z);

            yield return null;
        }
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance?.ReceiveScore(ScoreValue);
        }
        if (FloatingScoreManager.Instance != null && ScoreValue > 0)
        {
            FloatingScoreManager.Instance.DisplayFloatingScore(ScoreValue, transform.position);
        }
        Destroy(this.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    // スコア加算処理（実際のゲームではScoreManagerなどを使用）
        //    GiveScoreToPlayer(other.gameObject);
        //}
    }
    
    
    private void GiveScoreToPlayer(GameObject player)
    {
        StartCoroutine(DestroyAnim(player));
    }
    
    // IPlayerInteractable実装
    public override void OnStompedCore(GameObject player)
    {
        // スコアアイテムは踏みつけでも普通の取得と同じ
        GiveScoreToPlayer(player);
    }
    
    public override void OnSideHitCore(GameObject player)
    {
        // 横からの衝突でも取得
        GiveScoreToPlayer(player);
    }
}