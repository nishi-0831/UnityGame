using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SmashPlayer : MonoBehaviour
{
    [SerializeField] private float stickToScreenOffset_ = 5.0f;
    [SerializeField] private float moveToScreenDuration_ = 0.5f;
    [SerializeField] private float stickToScreenDuration = 1.0f;
    [SerializeField] private float fallLength = 10.0f;
    [SerializeField] private float fallDuration = 1.0f;
    [SerializeField] private float respawnDelay = 2.0f;
    //[SerializeField] private Quaternion playerRot_ ;

    private Camera camera_;
    private GameObject player_;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // カメラの参照を取得
        if (camera_ == null)
        {
            camera_ = Camera.main;
            if (camera_ == null)
            {
                //camera_ = FindObjectOfType<Camera>();
                Debug.LogWarning("camera_ == null");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

   

    public void Smash(GameObject player)
    {
        player_ = player;
        StartCoroutine(SmashRoutine());
    }

    private IEnumerator SmashRoutine()
    {
        //画面へ移動
        Vector3 screenStickPosition = camera_.transform.position;
        screenStickPosition += stickToScreenOffset_ * camera_.transform.forward;
        //player_.transform.up = camera_.transform.forward;
        player_.transform.rotation = Quaternion.LookRotation(camera_.transform.up, camera_.transform.forward);

        float timer = 0.0f;
        Vector3 startPos = player_.transform.position;
        while (timer < moveToScreenDuration_)
        {
            player_.transform.position = Vector3.Lerp(startPos, screenStickPosition, timer / moveToScreenDuration_);
            timer += Time.deltaTime;
            yield return null;
        }
        player_.transform.position = screenStickPosition;

        //画面に張り付く
        yield return new WaitForSeconds(stickToScreenDuration);

        //画面下に落下
        Vector3 startFallPosition = player_.transform.position;
        Vector3 endFallPosition = player_.transform.position + new Vector3(0, -fallLength, 0);

        timer = 0.0f;
        while (timer < fallDuration)
        {
            player_.transform.position = Vector3.Lerp(startFallPosition, endFallPosition, timer / fallDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        player_.transform.position = endFallPosition;

        yield return new WaitForSeconds(respawnDelay);
        //リスポーン
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        //ほんとはPlayerControllerに依存したくない
        var playerController = player_.GetComponent<PlayerController>();
        if (playerController == null) return;

        playerController.Respawn();
    }
}


