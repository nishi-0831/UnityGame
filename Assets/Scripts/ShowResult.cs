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
            //�����_�ȉ�2���܂ł̌Œ菬���_�\�L
            //F�́uFixed-point�i�Œ菬���_�j�v
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
            //�����_�ȉ�2���܂ł̌Œ菬���_�\�L
            //F�́uFixed-point�i�Œ菬���_�j�v
            clearTime_.text = scoreData_.clearTime.ToString("F2");
        }
    }
}
