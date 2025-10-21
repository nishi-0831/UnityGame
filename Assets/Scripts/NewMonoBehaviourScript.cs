using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public StageSetting stageSetting;
    public ScoreData scoreData;
    public PlayerController player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.Hp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
