using UnityEngine;

public class ClearZone : SplineMovementBase, IPlayerInteractable
{
    public void OnSideCollisionWithPlayer(GameObject player)
    {
        ClearGame();
    }

    public bool OnStompedByPlayer(GameObject player)
    {
        ClearGame();
        return true;
    }
    public void ClearGame()
    {
        ScoreManager.Instance.EndCountClearTime();
        TransitionScene.Instance.ToResult();
    }
   
}
