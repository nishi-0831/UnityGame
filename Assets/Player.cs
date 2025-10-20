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
        // スペースキーが押されたら
        if (Input.GetKeyDown(KeyCode.O))
        {
            //Debug.Log("Jump Pressed!");
            // 上向きの力を加える
            //playerRb.AddForce(Vector3.up * 400);

            // 音を鳴らす
            audioSource.Play();
        }
    }
}