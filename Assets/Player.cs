using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody playerRb;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (audioSource != null && audioSource.clip != null)
            {
                AudioManager.Instance.PlaySound(audioSource.clip);
            }
            else
            {
                Debug.LogWarning("AudioSourceÇ‹ÇΩÇÕClipÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            }
        }
    }
}