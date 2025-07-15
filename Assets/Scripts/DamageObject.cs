using UnityEngine;

public class DamageObject : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] int damageToPlayer_;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool OnStompedByPlayer(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            //ç∂ï˚å¸Ç÷îÚÇŒÇ∑
            playerController.OnDamage(damageToPlayer_, playerController.T + 0.1f);
            Debug.Log($"Player took {damageToPlayer_} damage!");
        }

        return true;
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            //ç∂ï˚å¸Ç÷îÚÇŒÇ∑
            playerController.OnDamage(damageToPlayer_,playerController.T + 0.1f);
            Debug.Log($"Player took {damageToPlayer_} damage!");
        }
    }
}
