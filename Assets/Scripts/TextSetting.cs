using UnityEngine;
using System;
using System.Linq;

public class TextSetting : MonoBehaviour
{
    public static TextSetting Instance { get; private set; }
    [SerializeField] private GameObject gameOverUI_;
    [SerializeField] private GameObject gameClearUI_;
    [SerializeField] PlayerController playerController;//PlayerControllerスクリプトへの参照
    //ゲームオーバー／クリア用コールバック

    private void StartGameOverUI()
    {
        if (gameOverUI_ != null)
        {
            gameOverUI_.SetActive(true);
        }
           
        //Debug.Log("ゲームオーバーUI表示！");
    }

    private void StartGameClearUI()
    {
        if (gameClearUI_ != null)
        {
            gameClearUI_.SetActive(true);
        }
           
        //Debug.Log("ゲームクリアUI表示！");
    }

    private void Start()
    {
        //デフォルトでUI表示関数を登録
        playerController.RegisterGameOverCallBack(StartGameOverUI);
        playerController.RegisterGameClearCallBack(StartGameClearUI);
    }

}

