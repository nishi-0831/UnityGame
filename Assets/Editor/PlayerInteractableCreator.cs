using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// PlayerInteractableBaseを継承したクラスのテンプレートを作成するエディタ拡張
/// </summary>
public class PlayerInteractableCreator : EditorWindow
{
    private string className = "NewPlayerInteractable";
    private string savePath = "Assets/Scripts";

    [MenuItem("Assets/Create/Player Interactable Script", false, 80)]
    public static void CreatePlayerInteractableScript()
    {
        // 選択されたフォルダのパスを取得
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(selectedPath))
        {
            selectedPath = "Assets";
        }
        else if (Path.GetExtension(selectedPath) != "")
        {
            selectedPath = Path.GetDirectoryName(selectedPath);
        }

        PlayerInteractableCreator window = GetWindow<PlayerInteractableCreator>();
        window.savePath = selectedPath;
        window.titleContent = new GUIContent("Create Player Interactable");
        window.minSize = new Vector2(400, 150);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Player Interactable Script", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Class Name:");
        className = EditorGUILayout.TextField(className);
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Save Path:");
        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", savePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // プロジェクトの相対パスに変換
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create"))
        {
            CreateScript();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void CreateScript()
    {
        if (string.IsNullOrEmpty(className))
        {
            EditorUtility.DisplayDialog("Error", "Class name cannot be empty.", "OK");
            return;
        }

        if (!IsValidClassName(className))
        {
            EditorUtility.DisplayDialog("Error", "Invalid class name. Please use a valid C# identifier.", "OK");
            return;
        }

        string fullPath = Path.Combine(savePath, className + ".cs");

        if (File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", "A file with this name already exists.", "OK");
            return;
        }

        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(savePath);

        string scriptContent = GenerateScriptContent(className);

        File.WriteAllText(fullPath, scriptContent);
        AssetDatabase.Refresh();

        // 作成されたスクリプトを選択状態にする
        Object createdScript = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        Selection.activeObject = createdScript;
        EditorGUIUtility.PingObject(createdScript);

        Close();
    }

    private bool IsValidClassName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (char.IsDigit(name[0])) return false;
        
        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }
        
        return true;
    }

    private string GenerateScriptContent(string className)
    {
        return $@"using UnityEngine;

/// <summary>
/// {className}の説明をここに記述
/// </summary>
public class {className} : PlayerInteractableBase
{{
    
    // 独自のフィールドをここに追加

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Initialize()
    {{
        base.Initialize();
        // 初期化処理をここに記述
    }}

    /// <summary>
    /// 開始処理（MonoBehaviourのStart相当）
    /// </summary>
    override protected void Start()
    {{
        base.Start();
        // 開始処理をここに記述
    }}

    /// <summary>
    /// 更新処理（MonoBehaviourのUpdate相当）
    /// </summary>
    void Update()
    {{
        // 更新処理をここに記述
    }}

    /// <summary>
    /// 移動処理の更新
    /// </summary>
    protected override void UpdateMovement()
    {{
        // 移動処理をここに記述
        // 例:
        // splineController_.Move(speed_);
    }}

    /// <summary>
    /// 壁との衝突処理
    /// </summary>
    protected override void OnCollideWall()
    {{
        // 壁衝突時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }}

    /// <summary>
    /// スプライン終端到達時の処理
    /// </summary>
    protected override void OnReachMaxT()
    {{
        // スプライン終端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }}

    /// <summary>
    /// スプライン始端到達時の処理
    /// </summary>
    protected override void OnReachMinT()
    {{
        // スプライン始端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }}

    /// <summary>
    /// プレイヤーに踏みつけられた時の処理
    /// </summary>
    /// <param name=""player"">プレイヤーのGameObject</param>
    public override void OnStompedCore(GameObject player)
    {{
        // 踏みつけ時の処理をここに記述
        // 例: 
        // OnDamage();
        // PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
    }}

    /// <summary>
    /// プレイヤーと横から衝突した時の処理
    /// </summary>
    /// <param name=""player"">プレイヤーのGameObject</param>
    public override void OnSideHitCore(GameObject player)
    {{
        // 横衝突時の処理をここに記述
        // 例:
        // PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
    }}
}}
";
    }
}