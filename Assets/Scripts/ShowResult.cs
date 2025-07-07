using UnityEngine;
using TMPro;
public class ShowResult : MonoBehaviour
{
    [SerializeField] private ScoreData scoreData_;
    [SerializeField] private TextMeshProUGUI score_;
    [SerializeField] private TextMeshProUGUI clearTime_;
    
    void Start()
    {
        if (score_ != null && scoreData_ != null)
        {
            score_.text = scoreData_.score.ToString();
        }
        if (clearTime_ != null && scoreData_ != null)
        {
            //小数点以下2桁までの固定小数点表記
            //Fは「Fixed-point（固定小数点）」
            clearTime_.text = scoreData_.clearTime.ToString("F2");
        }
    }

    void Update()
    {
        if (score_ != null && scoreData_ != null)
        {
            score_.text = scoreData_.score.ToString();
        }
        if (clearTime_ != null && scoreData_ != null)
        {
            //小数点以下2桁までの固定小数点表記
            //Fは「Fixed-point（固定小数点）」
            clearTime_.text = scoreData_.clearTime.ToString("F2");
        }
    }
}
