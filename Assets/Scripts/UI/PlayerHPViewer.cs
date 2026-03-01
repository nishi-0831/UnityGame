using UnityEngine;
using UnityEngine.UI;

public class PlayerHPViewer : MonoBehaviour
{
    [SerializeField] private Image[] images = new Image[3];
    [SerializeField] private PlayerController playerController;
    int currentHp;
    int maxHp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHp = playerController.Hp;
        currentHp = maxHp;
        playerController.RegisterOnDamageCallback(OnDamage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDamage()
    {
        Debug.Log("OnDamge");
        images[currentHp - 1].enabled = false;
        currentHp -= 1;
    }
}
