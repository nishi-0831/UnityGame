using UnityEngine;

public class DamageObject : MonoBehaviour, IPlayerInteractable
{
    [SerializeField]private PlayerInteractionProfile profile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.layer = (int)Mathf.Log(SplineLayerSettings.Instance.activeLayer.value, 2);

        if (profile == null)
        {
            // クラス名を取得
            string className = GetType().Name;
            profile = Resources.Load<PlayerInteractionProfile>($"PlayerInteractionProfiles/{className}");

            if (profile == null)
            {
                Debug.LogWarning($"{className} という名前のPlayerInteractionProfileが見当たりませんでした。" +
                              $"Resources/PlayerInteractionProfiles/{className}のように作ってください");
                // フォールバック用のデフォルトの設定を読み込む
                profile = Resources.Load<PlayerInteractionProfile>("PlayerInteractionProfiles/Default");
            }



            if (profile == null)
            {
                Debug.LogError("Resources/PlayerInteractionProfiles/Default が見当たりませんでした");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStomped(GameObject player)
    {
        //PlayerInteractionUtils.ApplySideBounce(player, transform.position);
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }

    public void OnSideHit(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player, profile.damageToPlayer);
    }
}
