using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SplineController))]
public class SplineControllerEditor : Editor
{
    private SplineController splineController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        splineController = (SplineController)target;

        // Inspector に表示された（＝選択された）タイミングで呼ぶ
        splineController.EditorOnSelected();
    }

    private void OnDisable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        // デフォルトのInspectorを描画
        DrawDefaultInspector();

        if(GUILayout.Button("Adjust Progress (Editor)"))
        {
            splineController.EditorOnSelected();
        }
    }
}
