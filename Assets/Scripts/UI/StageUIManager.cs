using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class StageUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageName;
    [SerializeField] private List<StageSetting> stageSettingList = new List<StageSetting>();
    public StageScoreUI currentStageScoreUI;
    [SerializeField] private int index = 0;

    [Header("UI選択追従用：中央のステージボタンを設定")]
    [SerializeField] private Button stageSelectButton;

    public static StageUIManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        index = Mathf.Clamp(index, 0, stageSettingList.Count - 1);
        currentStageScoreUI?.ApplyStageSetting(stageSettingList[index]);
    }

    private void Start()
    {
        // PauseManagerが存在するかチェックする (MainMenuからの遷移対策)
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStart(() =>
            {
                currentStageScoreUI.ApplyStageSetting(stageSettingList[index]);
                stageName.text = (index + 1).ToString();
            });
        }

        // EventSystemによる初期選択は、必ず実行する
        SelectCurrentStageButton();
    }

    private void SelectCurrentStageButton()
    {
        StartCoroutine(SelectCurrentStageButtonCoroutine());
    }

    private IEnumerator SelectCurrentStageButtonCoroutine()
    {
        // 処理を1フレーム待機
        yield return null;

        if (stageSelectButton == null || EventSystem.current == null) yield break;

        // 確実に選択を解除し、現在のボタンを再選択
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(stageSelectButton.gameObject);
    }

    public StageSetting CurrentStageSetting() => stageSettingList[index];

    public void ChangePrevStageScoreUI()
    {
        bool changed = false;

        if (index > 0) // 0より大きい場合のみ、インデックスを減らす
        {
            currentStageScoreUI.ApplyStageSetting(stageSettingList[--index]);
            stageName.text = (index + 1).ToString();
            changed = true;
        }

        // ステージが変更されたかどうかにかかわらず、必ずボタンを再選択してハイライトをリフレッシュする
        SelectCurrentStageButton();
    }

    public void ChangeNextStageScoreUI()
    {
        bool changed = false;

        // ★ リストの範囲内であれば、インデックスを増やす
        if (index + 1 < stageSettingList.Count)
        {
            currentStageScoreUI.ApplyStageSetting(stageSettingList[++index]);
            stageName.text = (index + 1).ToString();
            changed = true;
        }

        // ★ ステージが変更されたかどうかにかかわらず、必ずボタンを再選択してハイライトをリフレッシュする
        // これにより、リストの末端でボタンを押しても操作不能になることを防ぐ
        SelectCurrentStageButton();
    }

    public void TransitionSelectedStageScene()
    {
        if (index < 0 || index >= stageSettingList.Count)
        {
            Debug.LogWarning("index is invalid");
            return;
        }

        string sceneName = stageSettingList[index].stageSceneName;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetCurrentStage(sceneName, index);
        }

        SceneManager.LoadScene(sceneName);
    }

#if UNITY_EDITOR
    [InitializeOnEnterPlayMode]
    static void ResetStageSettingForPlayMode()
    {
        string[] guids = AssetDatabase.FindAssets("t:StageSetting");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StageSetting stageSetting = AssetDatabase.LoadAssetAtPath<StageSetting>(path);
            if (stageSetting != null)
            {
                stageSetting.InitAchievement();
            }
        }
    }
#endif
}