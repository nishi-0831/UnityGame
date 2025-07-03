using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreData",menuName = "Scriptable Objects/ScoreData")]
public class ScoreData : ScriptableObject
{
    [SerializeField] private int initScore = default;
    [SerializeField] private int maxScore;
    [SerializeField] private int minScore;
    [SerializeField] private float initTime = 0f;
    public int score;
    public float clearTime;

    //スコアの初期化
    public void Initialize()
    {
        score = initScore;
        clearTime = initTime;
    }
}
