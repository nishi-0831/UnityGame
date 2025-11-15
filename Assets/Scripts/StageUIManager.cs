using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
public class StageUIManager : MonoBehaviour
{
    public TextMeshProUGUI stageName;
    public List<StageSetting> stageSettingList = new List<StageSetting>();
    public StageScoreUI currentStageScoreUI;
    public int index = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        index = Mathf.Clamp(index, 0, stageSettingList.Count - 1);
        currentStageScoreUI.ApplyStageSetting(stageSettingList[index]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangePrevStageScoreUI()
    {
        if (index == 0)
            return;

        currentStageScoreUI.ApplyStageSetting(stageSettingList[--index]);
        stageName.text = (index + 1).ToString();
    }
    public void ChangeNextStageScoreUI()
    {
        if (index + 1 >= stageSettingList.Count) 
            return;

        currentStageScoreUI.ApplyStageSetting(stageSettingList[++index]);
        stageName.text = (index + 1).ToString();
    }
    public void TransitionSelectedStageScene()
    {
        if(index < 0 &&  index >= stageSettingList.Count)
        {
            Debug.LogWarning("index is invalid");
            return;
        }

        string sceneName = stageSettingList[index].stageSceneName;
        SceneManager.LoadScene(sceneName);
    }
}
