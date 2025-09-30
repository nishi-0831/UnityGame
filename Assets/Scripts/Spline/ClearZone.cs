using UnityEngine;

public class ClearZone : PlayerInteractableBase
{
    public override void OnSideHitCore(GameObject player)
    {
        ClearGame();
    }

    public override void OnStompedCore(GameObject player)
    {
        ClearGame();
    }
    public void ClearGame()
    {
        ScoreManager.Instance.EndCountClearTime();
        TransitionScene.Instance.ToResult();
    }
   
}
