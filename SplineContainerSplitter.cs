using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
public class SplineContainerSplitter : MonoBehaviour
{
    [Header("分割設定")]
    [SerializeField] private SplineContainer targetContainer;
    [SerializeField] private int splitKnotIndex = 4; // 分割開始位置（このKnotが後ろの区間の最初になる）
    [SerializeField] private string prefixName = "SplitSpline"; // 生成されるGameObjectの接頭辞

    [ContextMenu("Split Spline at Index")]
    public void SplitSplineAtIndex()
    {
        if (targetContainer == null)
        {
            targetContainer = GetComponent<SplineContainer>();
        }

        if (targetContainer == null || targetContainer.Spline == null)
        {
            Debug.LogError("SplineContainer が設定されていません");
            return;
        }

        Spline originalSpline = targetContainer.Spline;
        int totalKnots = originalSpline.Count;

        if (splitKnotIndex <= 0 || splitKnotIndex >= totalKnots)
        {
            Debug.LogError($"無効な分割インデックスです。0 < index < {totalKnots} の範囲で指定してください");
            return;
        }

        // 前の区間: 0 ~ splitKnotIndex (含む)
        // 後ろの区間: splitKnotIndex ~ totalKnots-1 (含む)

        // 1. 前の区間用の新しいSplineContainerを作成
        GameObject firstGameObject = new GameObject($"{prefixName}_First_{0}-{splitKnotIndex}");
        firstGameObject.transform.SetParent(targetContainer.transform.parent);
        firstGameObject.transform.position = targetContainer.transform.position;
        firstGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer firstContainer = firstGameObject.AddComponent<SplineContainer>();
        Spline firstSpline = new Spline();

        for (int i = 0; i <= splitKnotIndex; i++)
        {
            firstSpline.Add(originalSpline[i]);
        }

        firstContainer.Spline = firstSpline;

        // 2. 後ろの区間用の新しいSplineContainerを作成
        GameObject secondGameObject = new GameObject($"{prefixName}_Second_{splitKnotIndex}-{totalKnots - 1}");
        secondGameObject.transform.SetParent(targetContainer.transform.parent);
        secondGameObject.transform.position = targetContainer.transform.position;
        secondGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer secondContainer = secondGameObject.AddComponent<SplineContainer>();
        Spline secondSpline = new Spline();

        for (int i = splitKnotIndex; i < totalKnots; i++)
        {
            secondSpline.Add(originalSpline[i]);
        }
        if(originalSpline.Closed)
        {
            //secondSpline.Add(originalSpline.Last());
        }
        secondContainer.Spline = secondSpline;

        // 3. SplineContainerLinkを設定して繋ぐ
        SplineContainerLink linkComponent = firstGameObject.AddComponent<SplineContainerLink>();
        linkComponent.prev = null; // 元のprevを必要に応じて設定
        linkComponent.next = secondContainer;

        SplineContainerLink secondLinkComponent = secondGameObject.AddComponent<SplineContainerLink>();
        secondLinkComponent.prev = firstContainer;
        secondLinkComponent.next = null; // 元のnextを必要に応じて設定

        // 4. 元のSplineContainerのリンク情報を更新（必要に応じて）
        SplineContainerLink originalLink = targetContainer.GetComponent<SplineContainerLink>();
        if (originalLink != null)
        {
            // 元のprevがあれば、firstContainerに引き継ぐ
            if (originalLink.prev != null)
            {
                linkComponent.prev = originalLink.prev;
                var prevLink = originalLink.prev.GetComponent<SplineContainerLink>();
                if (prevLink != null)
                {
                    prevLink.next = firstContainer;
                }
            }

            // 元のnextがあれば、secondContainerに引き継ぐ
            if (originalLink.next != null)
            {
                secondLinkComponent.next = originalLink.next;
                var nextLink = originalLink.next.GetComponent<SplineContainerLink>();
                if (nextLink != null)
                {
                    nextLink.prev = secondContainer;
                }
            }
        }

        // 5. 元のGameObjectを無効化（またはシーンから削除）
        targetContainer.gameObject.SetActive(false);
        Debug.Log($"Spline分割完了: {firstGameObject.name} と {secondGameObject.name} を作成しました");

        // Hierarchyを整理
        EditorUtility.SetDirty(firstGameObject);
        EditorUtility.SetDirty(secondGameObject);
        EditorUtility.SetDirty(targetContainer.gameObject);
    }

    [ContextMenu("Show Knot Count")]
    public void ShowKnotCount()
    {
        if (targetContainer == null)
        {
            targetContainer = GetComponent<SplineContainer>();
        }

        if (targetContainer != null && targetContainer.Spline != null)
        {
            Debug.Log($"総Knot数: {targetContainer.Spline.Count}");
        }
    }
}
#endif