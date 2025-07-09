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
            //��]
            simpleGemsAnim.UpdateRot();

            //���W
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            Vector3 endPos = player.transform.position + new Vector3(0,endPosOffsetY,0);

            Vector3 holizontal = Vector3.Lerp(startPos, endPos, t);

            float vertical = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = new Vector3(holizontal.x,holizontal.y + vertical, holizontal.z);

            yield return null;
        }
        Debug.Log($"Score +{scoreValue}");
        // ������ScoreManager�ɒʒm���鏈����ǉ�
        ScoreManager.Instance.ReceiveScore(scoreValue);
        
        
        Destroy(this.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �X�R�A���Z�����i���ۂ̃Q�[���ł�ScoreManager�Ȃǂ��g�p�j
            GiveScoreToPlayer(other.gameObject);
        }
    }
    
    
    private void GiveScoreToPlayer(GameObject player)
    {
        StartCoroutine(DestroyAnim(player));
    }
    
    // IPlayerInteractable����
    public bool OnStompedByPlayer(GameObject player)
    {
        // �X�R�A�A�C�e���͓��݂��ł����ʂ̎擾�Ɠ���
        GiveScoreToPlayer(player);
        return false; // ���݂��G�t�F�N�g�͕s�v
    }
    
    public void OnSideCollisionWithPlayer(GameObject player)
    {
        // ������̏Փ˂ł��擾
        GiveScoreToPlayer(player);
    }
}