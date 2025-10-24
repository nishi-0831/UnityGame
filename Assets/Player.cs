using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody playerRb;
    
    [SerializeField] public float jumpSoundCooldown;
    [SerializeField] private AudioClip PlayerJumpSound;
    private bool canPlay = true;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canPlay)
        {
            PlayJumpSound();
        }
    }
    private void PlayJumpSound()
    {
        if (AudioManager.Instance != null && PlayerJumpSound != null)
        {
            //AudioManager経由で再生（同時再生可能）
            AudioManager.Instance.PlaySound(PlayerJumpSound, 0.4f);

            //再生間隔を制御
            StartCoroutine(PlayCooldown());
        }
        else
        {
            Debug.LogWarning("AudioManager または PlayerJumpSound が設定されていません。");
        }
    }

    private IEnumerator PlayCooldown()
    {
        canPlay = false;                          //音を再生不可にする
        yield return new WaitForSeconds(jumpSoundCooldown);
        canPlay = true;                           //再び再生可能にする
    }
}