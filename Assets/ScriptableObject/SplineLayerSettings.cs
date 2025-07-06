using UnityEngine;

[CreateAssetMenu(fileName = "SplineLayerSettings", menuName = "Scriptable Objects/SplineLayerSettings")]
public class SplineLayerSettings : ScriptableObject
{
    public LayerMask activeLayer;
    public LayerMask disabledLayer;
    public LayerMask groundLayer;
    
   
}
