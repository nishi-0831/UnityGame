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

		findLayer = EditorGUILayout.LayerField("layer", 0);
		if(GUILayout.Button("FindObjectByLayer"))
		{
            // ì¡íËÉåÉCÉÑÅ[ÇÃÇ›éÊìæ
            generators = FindObjectsByType<SplineMeshGenerator>(FindObjectsSortMode.None).Where(generator => generator.gameObject.layer == (int)Mathf.Log(findLayer, 2)).ToList();
        }
		so.Update();
		EditorGUILayout.PropertyField(generatorsProperty,true);
		so.ApplyModifiedProperties();

		if(GUILayout.Button("SplitSelectedAplineMesh"))
		{
			foreach(SplineMeshGenerator generator in generators)
			{
				SplitSplineMesh(generator);
			}
		}
    }
    public void SplitSplineMesh(SplineMeshGenerator splineMeshGenerator)
    {
        float width = splineMeshGenerator.width;
        float height = splineMeshGenerator.height;
		
        float topHeight = height * topRatio;
        float bottomHeight = height - topHeight;
        Vector3 bottomOffset = new Vector3(0, -topHeight, 0);

		splineMeshGenerator.height = topHeight;
		splineMeshGenerator.material = topMaterial;

		GameObject bottomGameObject = GameObject.Instantiate(splineMeshGenerator.gameObject);
		SplineMeshGenerator bottomSplineMeshGenerator = bottomGameObject.GetComponent<SplineMeshGenerator>();
		bottomSplineMeshGenerator.height = bottomHeight;
		bottomSplineMeshGenerator.material = bottomMaterial;

		splineMeshGenerator.Generate();
		bottomSplineMeshGenerator.Generate();
    }
}
