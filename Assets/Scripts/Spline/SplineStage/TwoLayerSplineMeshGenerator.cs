using Assets.Scripts;
using UnityEngine;
using UnityEngine.Splines;

public class TwoLayerSplineMeshGenerator : MonoBehaviour
{
    [SerializeField]SplineMeshGenerator top;
    SplineMeshGenerator bottom;
    [SerializeField, Min(0.0001f)] private float height = 1.0f; // ‰º•ûŒü(-Y)
    [SerializeField, Min(0.0001f)] private float width = 1.0f;  // +X •ûŒü
    [SerializeField] public Material topMaterial;
    [SerializeField] public Material bottomMaterial;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField, Range(0.1f, 0.9f)] private float topRatio = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("Generate Two Layer")]
    public void GenerateTwoLayer()
    {
        top.splineContainer = splineContainer;
        top.width = width;
        top.height = height * topRatio;
        top.offset = Vector3.zero;

        bottom.splineContainer = splineContainer;
        bottom.width = width;
        bottom.height = height - top.height;
        bottom.offset = new Vector3(0, -top.height, 0);
    }
    public void SplitSplineMesh(SplineMeshGenerator splineMeshGenerator)
    {
        float width = splineMeshGenerator.width;
        float height = splineMeshGenerator.height;
        float topHeight = height * topRatio;
        float bottomHeight = height - topHeight;
        Vector3 bottomOffset = new Vector3(0, -topHeight, 0);
    }
}
