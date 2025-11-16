using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
public class StageUIManager : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI stageName;
    [SerializeField]private List<StageSetting> stageSettingList = new List<StageSetting>();
    public StageScoreUI currentStageScoreUI;
    [SerializeField]private int index = 0;
    public static StageUIManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Destroy(Instance);
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        index = Mathf.Clamp(index, 0, stageSettingList.Count - 1);
        currentStageScoreUI?.ApplyStageSetting(stageSettingList[index]);
    }
    void Start()
    {
        
    }

    public StageSetting CurrentStageSetting () => stageSettingList[index];
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
