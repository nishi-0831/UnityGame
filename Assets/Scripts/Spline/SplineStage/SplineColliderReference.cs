using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SplineColliderReference : MonoBehaviour
{
    public SplineContainer splineContainer;
    static SplineLayerSettings layerSettings;
    static bool layerSettingsIsNull = false;
    private void Awake()
    {
        if(layerSettingsIsNull)
        {
            // ‚·‚Å‚É null‚Æ”»–¾‚µ‚Ä‚¢‚é‚Ì‚Å‘¦ return
            return;
        }
        if(layerSettings == null)
        {
            layerSettings = Resources.Load<SplineLayerSettings>("SplineLayerSettings");

            if (layerSettings == null)
            {
                layerSettingsIsNull = true;
                Debug.LogError("SplineLayerSettings‚ªResources‚É‘¶İ‚µ‚Ü‚¹‚ñBlayerSettings‚ªnull‚Å‚·B");
                return;
            }
        }
        
        gameObject.layer = (int)Mathf.Log(layerSettings.groundLayer.value, 2);
    }
}
