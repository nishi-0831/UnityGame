using UnityEngine;
using UnityEditor;
using Assets.Scripts;
using System.Linq;
using System.Collections.Generic;

public class SplineMeshSplitter : EditorWindow
{
	Material topMaterial;
	Material bottomMaterial;
	float topRatio;
	float minVal = 0.1f;
	float maxVal = 0.9f;
	bool findByLayer = false;
	int findLayer = 0;
	SerializedProperty generatorsProperty;
	SerializedObject so;
    public List<SplineMeshGenerator> generators = new List<SplineMeshGenerator>();
    [MenuItem("Tools/SplitMeshSpline")]
	static void Init()
	{
		SplineMeshSplitter splitter = (SplineMeshSplitter)EditorWindow.GetWindow(typeof(SplineMeshSplitter));
		splitter.Show();

         splitter.so = new SerializedObject(splitter);
         splitter.generatorsProperty = splitter.so.FindProperty("generators");
    }

	
    private void OnGUI()
    {
		topMaterial = (Material)EditorGUILayout.ObjectField("TopMaterial", topMaterial, typeof(Material), false);
		bottomMaterial = (Material)EditorGUILayout.ObjectField("BottomMaterial", bottomMaterial, typeof(Material), false);
		topRatio = EditorGUILayout.Slider("TopRatio",topRatio, minVal, maxVal);

		findLayer = EditorGUILayout.LayerField("layer", findLayer);
		if(GUILayout.Button("FindObjectByLayer"))
		{
            // 特定レイヤーのみ取得
            generators = FindObjectsByType<SplineMeshGenerator>(FindObjectsSortMode.None).ToList();
            //generators = FindObjectsByType<SplineMeshGenerator>(FindObjectsSortMode.None).Where(generator => generator.gameObject.layer == (int)Mathf.Log(findLayer, 2)).ToList();
        }
		so.Update();
		EditorGUILayout.PropertyField(generatorsProperty,true);
		so.ApplyModifiedProperties();

		if(GUILayout.Button("SplitSelectedAplineMesh"))
		{
            // Undoグループ開始
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Split SplineMesh");

            foreach (SplineMeshGenerator generator in generators)
			{
				SplitSplineMesh(generator);
			}

            // Undoグループをまとめる
            Undo.CollapseUndoOperations(group);
        }
    }
    private void SplitSplineMesh(SplineMeshGenerator splineMeshGenerator)
    {
        float width = splineMeshGenerator.width;
        float height = splineMeshGenerator.height;
		
        float topHeight = height * topRatio;
        float bottomHeight = height - topHeight;
        Vector3 bottomOffset = new Vector3(0, -topHeight, 0);

        // 既存オブジェクトの変更をUndo登録
        Undo.RecordObject(splineMeshGenerator, "Change SplineMeshGenerator");
        splineMeshGenerator.height = topHeight;
		splineMeshGenerator.material = topMaterial;

		GameObject bottomGameObject = new GameObject($"{splineMeshGenerator.name} bottom");
        Undo.RegisterCreatedObjectUndo(bottomGameObject, "Create Bottom SplineMesh");
        // Transformの値をコピー
        bottomGameObject.transform.position = splineMeshGenerator.transform.position;
        bottomGameObject.transform.rotation = splineMeshGenerator.transform.rotation;
		bottomGameObject.transform.localScale = splineMeshGenerator.transform.localScale;
		bottomGameObject.transform.parent = bottomGameObject.transform.parent;

        // SplineMeshGeneratorを追加、設定
        SplineMeshGenerator bottomSplineMeshGenerator = bottomGameObject.AddComponent<SplineMeshGenerator>();
        Undo.RegisterCreatedObjectUndo(bottomSplineMeshGenerator, "Add SplineMeshGenerator");
        bottomSplineMeshGenerator.width = width;
		bottomSplineMeshGenerator.height = bottomHeight;
		bottomSplineMeshGenerator.offset = bottomOffset;
		bottomSplineMeshGenerator.material = bottomMaterial;
		bottomSplineMeshGenerator.splineContainer = splineMeshGenerator.splineContainer;
		bottomSplineMeshGenerator.addCollider = false;

		// 生成
		splineMeshGenerator.Generate();
		bottomSplineMeshGenerator.Generate();

		EditorUtility.SetDirty(splineMeshGenerator);
    }
}
