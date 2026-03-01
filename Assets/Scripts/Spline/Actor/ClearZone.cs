using System;
using UnityEngine;

public class ClearZone : PlayerInteractableBase
{
    // クリア後、シーン遷移するまでの猶予時間
    //[SerializeField] float transitionSceneDelay = 1.0f;
    override protected void Start() 
    {
        base.Start();
        GameOutcomeManager.Instance.RegisterGameClearCallback(ClearGame);
    }
    public override void OnSideHitCore(GameObject player)
    {
        GameOutcomeManager.Instance.TriggerGameClear();
    }

    public override void OnStompedCore(GameObject player)
    {
        GameOutcomeManager.Instance.TriggerGameClear();
    }
    private void ClearGame()
    {
        ScoreManager.Instance.EndCountClearTime();
        //TransitionScene.Instance.ToResult(transitionSceneDelay);
    }
}
