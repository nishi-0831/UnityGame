using UnityEngine;

/// <summary>
/// <para>- スプライン移動に関するレイヤー設定を保持する ScriptableObject</para>
/// <para>- 有効レイヤーと無効レイヤーおよび床レイヤーを LayerMask で管理する</para>
/// </summary>
[CreateAssetMenu(fileName = "SplineLayerSettings", menuName = "Scriptable Objects/SplineLayerSettings")]
public class SplineLayerSettings : ScriptableObject
{
    private static SplineLayerSettings instance;

    public static SplineLayerSettings Instance
    {
        get
        { 
            if(instance == null)
            {
                instance = Resources.Load<SplineLayerSettings>("SplineLayerSettings");
                if(instance == null)
                {
                    Debug.LogError("SplineLayerSettingsが見当たりません。Resources/SplineLayerSettingsに作ってください");
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// <para>- 当たり判定を有効とするレイヤー</para>
    /// <para>- ゲーム内でアクティブとして扱う対象レイヤー</para>
    /// </summary>
    public LayerMask activeLayer;

    /// <summary>
    /// <para>- 当たり判定を無効とするレイヤー</para>
    /// <para>- 一時的に衝突を除外したい対象レイヤー</para>
    /// </summary>
    public LayerMask disabledLayer;

    /// <summary>
    /// <para>- 地面として扱うレイヤー</para>
    /// <para>- レイキャストや接地判定の対象となるレイヤー</para>
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// 壁として扱うレイヤー
    /// </summary>
    public LayerMask wallLayer;
}
