using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class PlacementKnot : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private bool close;
    [SerializeField] private SplineContainer splineContainer;
    private Spline spline;

    [Header("等間隔配置のサンプリング精度")]
    [SerializeField, Range(4, 1024)] private int samplesPerSegment = 32;

    [ContextMenu("PlacementKnot (等距離)")]
    public void Placement()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainerが見つかりません");
            return;
        }

        spline = splineContainer.Spline;
        if (spline == null || spline.Count == 0)
        {
            Debug.LogError("Splineが空です");
            return;
        }

        int knotCount = spline.Count;
        int segmentCount = spline.Closed ? knotCount : Mathf.Max(1, knotCount - 1);
        int sampleCount = Mathf.Max(4, segmentCount * samplesPerSegment);

        // NativeSpline を作り world 空間でサンプリングし、local に戻す
        var native = new NativeSpline(spline, splineContainer.transform.localToWorldMatrix);

        var ts = new List<float>(sampleCount + 1);
        var positions = new List<Vector3>(sampleCount + 1);
        var cumulative = new List<float>(sampleCount + 1);

        Vector3 prevPos = Vector3.zero;
        float acc = 0f;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Unity.Mathematics.float3 pF, tanF, upF;
            SplineUtility.Evaluate<NativeSpline>(native, t, out pF, out tanF, out upF);
            Vector3 worldPos = (Vector3)pF;
            Vector3 localPos = splineContainer.transform.InverseTransformPoint(worldPos);

            ts.Add(t);
            positions.Add(localPos);

            if (i == 0)
            {
                cumulative.Add(0f);
                prevPos = localPos;
            }
            else
            {
                acc += Vector3.Distance(prevPos, localPos);
                cumulative.Add(acc);
                prevPos = localPos;
            }
        }

        float totalLength = cumulative[cumulative.Count - 1];
        if (totalLength <= Mathf.Epsilon)
        {
            Debug.LogWarning("スプライン長がほぼ0です。等距離配置をスキップします。");
            return;
        }

        float spacing = totalLength / knotCount;

        // 各 Knot を targetDistance に合わせて再配置
        for (int k = 0; k < knotCount; k++)
        {
            float targetDist = spacing * k;

            // binary search で該当区間を見つける
            int idx = cumulative.BinarySearch(targetDist);
            if (idx < 0)
            {
                idx = ~idx;
                // idx は cumulative[idx] が targetDist より大きい最小 index
            }

            int i0 = Mathf.Clamp(idx - 1, 0, cumulative.Count - 1);
            int i1 = Mathf.Clamp(idx, 0, cumulative.Count - 1);

            Vector3 newLocalPos;
            if (i0 == i1)
            {
                newLocalPos = positions[i0];
            }
            else
            {
                float d0 = cumulative[i0];
                float d1 = cumulative[i1];
                float t01 = (d1 - d0) <= Mathf.Epsilon ? 0f : (targetDist - d0) / (d1 - d0);
                float sampleT = Mathf.Lerp(ts[i0], ts[i1], t01);

                // 精密評価（world -> local）
                Unity.Mathematics.float3 pF, tanF, upF;
                SplineUtility.Evaluate<NativeSpline>(native, sampleT, out pF, out tanF, out upF);
                Vector3 worldPos = (Vector3)pF;
                newLocalPos = splineContainer.transform.InverseTransformPoint(worldPos);
            }

            // BezierKnot は struct のため取り出して戻す
            var knot = spline[k];
            knot.Position = newLocalPos;
            spline[k] = knot;
        }

        // 閉じる設定とハンドル調整
        spline.Closed = close;
        spline.SetTangentMode(TangentMode.AutoSmooth);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(splineContainer);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif

        Debug.Log($"PlacementKnot: {knotCount} knots redistributed evenly along spline (totalLength={totalLength:F3})");
    }
}