using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;



#if UNITY_EDITOR
public class SplineContainerSplitter : MonoBehaviour
{
    [Header("分割設定")]
    [SerializeField] private SplineContainer targetContainer;
    [SerializeField] private int splitKnotFirstIndex = 4; // 分割開始位置（このKnotが後ろの区間の最後になる）
    [SerializeField] private int splitKnotLastIndex = 5; // 分割開始位置（このKnotが後ろの区間の最初になる）
    [SerializeField] private string prefixName = "SplitSpline"; // 生成されるGameObjectの接頭辞
    [SerializeField] private bool destroyTarget = false;

    // 分割結果を格納する構造体
    private struct DivisionResult
    {
        public List<(GameObject gameObject, SplineContainer container)> containers;
    }

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

        if (splitKnotFirstIndex < 0 || splitKnotFirstIndex >= splitKnotLastIndex || splitKnotLastIndex <= splitKnotFirstIndex || splitKnotLastIndex >= totalKnots)
        {
            Debug.LogError($"無効な分割インデックスです。0 <= firstIndex < lastIndex < {totalKnots} の範囲で指定してください");
            return;
        }

        // 分割処理を実行
        DivisionResult result;
        if (splitKnotFirstIndex == 0 || splitKnotLastIndex == totalKnots - 1)
        {
            result = TwoDivision();
        }
        else
        {
            result = ThreeDivision();
        }

        // リンク情報の設定
        LinkContainers(result.containers);

        // 元のSplineContainerのリンク情報を引き継ぐ
        UpdateOriginalLinks(result.containers);

        // メッシュ生成
        GenerateMeshes(result.containers);

        // 元のGameObjectを無効化または削除
        FinalizeOriginalContainer();

        // デバッグログ
        string containerNames = string.Join(", ", result.containers.Select(c => c.gameObject.name));
        Debug.Log($"Spline分割完了: {containerNames} を作成しました");

        // Hierarchyを整理
        foreach (var container in result.containers)
        {
            EditorUtility.SetDirty(container.gameObject);
        }
        EditorUtility.SetDirty(targetContainer.gameObject);
    }

    private DivisionResult TwoDivision()
    {
        Spline originalSpline = targetContainer.Spline;
        int totalKnots = originalSpline.Count;

        // 前の区間: splitKnotFirstIndex ~ splitKnotLastIndex (含む)
        // 後ろの区間: splitKnotLastIndex ~ totalKnots-1 または 0 ~ splitKnotFirstIndex (含む)

        GameObject firstGameObject = new GameObject($"{prefixName}_First_{splitKnotFirstIndex}-{splitKnotLastIndex}");
        firstGameObject.transform.SetParent(targetContainer.transform.parent);
        firstGameObject.transform.position = targetContainer.transform.position;
        firstGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer firstContainer = firstGameObject.AddComponent<SplineContainer>();
        Spline firstSpline = new Spline();

        for (int i = splitKnotFirstIndex; i <= splitKnotLastIndex; i++)
        {
            firstSpline.Add(originalSpline[i]);
        }

        firstContainer.Spline = firstSpline;

        GameObject secondGameObject = new GameObject()
        {
            transform =
        {
          parent = targetContainer.transform.parent,
          position = targetContainer.transform.position,
          rotation = targetContainer.transform.rotation
        }
        };

        SplineContainer secondContainer = secondGameObject.AddComponent<SplineContainer>();
        Spline secondSpline = new Spline();

        // splitKnotFirstIndex が 0 の場合: splitKnotLastIndex から末尾
        if (splitKnotFirstIndex == 0)
        {
            secondGameObject.name = ($"{prefixName}_Second_{splitKnotLastIndex}-{totalKnots - 1}");
            for (int i = splitKnotLastIndex; i < totalKnots; i++)
            {
                secondSpline.Add(originalSpline[i]);
            }
            if(originalSpline.Closed)
            {
                secondSpline.Add(originalSpline[0]);
            }
        }
        // splitKnotLastIndex が totalKnots - 1 の場合: 0 から splitKnotFirstIndex
        else if (splitKnotLastIndex == totalKnots - 1)
        {
            secondGameObject.name = ($"{prefixName}_Second_{splitKnotLastIndex}-{splitKnotFirstIndex}");
            if (originalSpline.Closed)
            {
                // 閉じたスプラインの場合、splitKnotLastIndex から始まり、splitKnotFirstIndex で終わる
                for (int i = splitKnotLastIndex; i <= totalKnots + splitKnotFirstIndex; i++)
                {
                    secondSpline.Add(originalSpline[i % totalKnots]);
                }
            }
            else
            {
                secondGameObject.name = ($"{prefixName}_Second_{0}-{splitKnotFirstIndex}");

                // 開いたスプラインの場合、0 から splitKnotFirstIndex まで
                for (int i = 0; i <= splitKnotFirstIndex; i++)
                {
                    secondSpline.Add(originalSpline[i]);
                }
            }
        }

        secondContainer.Spline = secondSpline;
        var result = new DivisionResult
        {
            containers = new List<(GameObject, SplineContainer)>
            {
  (firstGameObject, firstContainer),
                (secondGameObject, secondContainer)
            }
        };

        return result;
    }

    private DivisionResult ThreeDivision()
    {
        Spline originalSpline = targetContainer.Spline;
        int totalKnots = originalSpline.Count;

        // 前の区間: 0 ~ splitKnotFirstIndex (含む)
        // 真ん中の区間: splitKnotFirstIndex ~ splitKnotLastIndex (含む)
        // 後ろの区間: splitKnotLastIndex ~ totalKnots-1 (含む)

        GameObject firstGameObject = new GameObject($"{prefixName}_First_{0}-{splitKnotFirstIndex}");
        firstGameObject.transform.SetParent(targetContainer.transform.parent);
        firstGameObject.transform.position = targetContainer.transform.position;
        firstGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer firstContainer = firstGameObject.AddComponent<SplineContainer>();
        Spline firstSpline = new Spline();

        for (int i = 0; i <= splitKnotFirstIndex; i++)
        {
            firstSpline.Add(originalSpline[i]);
        }

        firstContainer.Spline = firstSpline;

        GameObject secondGameObject = new GameObject($"{prefixName}_Second_{splitKnotFirstIndex}-{splitKnotLastIndex}");
        secondGameObject.transform.SetParent(targetContainer.transform.parent);
        secondGameObject.transform.position = targetContainer.transform.position;
        secondGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer secondContainer = secondGameObject.AddComponent<SplineContainer>();
        Spline secondSpline = new Spline();

        for (int i = splitKnotFirstIndex; i <= splitKnotLastIndex; i++)
        {
            secondSpline.Add(originalSpline[i]);
        }

        secondContainer.Spline = secondSpline;

        GameObject thirdGameObject = new GameObject($"{prefixName}_Third_{splitKnotLastIndex}-{totalKnots - 1}");
        thirdGameObject.transform.SetParent(targetContainer.transform.parent);
        thirdGameObject.transform.position = targetContainer.transform.position;
        thirdGameObject.transform.rotation = targetContainer.transform.rotation;

        SplineContainer thirdContainer = thirdGameObject.AddComponent<SplineContainer>();
        Spline thirdSpline = new Spline();

        for (int i = splitKnotLastIndex; i < totalKnots; i++)
        {
            thirdSpline.Add(originalSpline[i]);
        }

        thirdContainer.Spline = thirdSpline;

        var result = new DivisionResult
        {
            containers = new List<(GameObject, SplineContainer)>
            {
      (firstGameObject, firstContainer),
       (secondGameObject, secondContainer),
                (thirdGameObject, thirdContainer)
   }
        };

        return result;
    }

    /// <summary>
    /// 分割されたコンテナ同士をリンク
    /// </summary>
    private void LinkContainers(List<(GameObject gameObject, SplineContainer container)> containers)
    {
        Spline originalSpline = targetContainer.Spline;

        for (int i = 0; i < containers.Count; i++)
        {
            var link = containers[i].gameObject.AddComponent<SplineContainerLink>();

            // prevを設定
            if (i > 0)
            {
                link.prev = containers[i - 1].container;
            }
            else if (originalSpline.Closed && containers.Count > 1)
            {
                // 閉じたスプラインの場合、最初のコンテナのprevは最後のコンテナ
                link.prev = containers[containers.Count - 1].container;
            }
            else
            {
                link.prev = null;
            }

            // nextを設定
            if (i < containers.Count - 1)
            {
                link.next = containers[i + 1].container;
            }
            else if (originalSpline.Closed && containers.Count > 1)
            {
                // 閉じたスプラインの場合、最後のコンテナのnextは最初のコンテナ
                link.next = containers[0].container;
            }
            else
            {
                link.next = null;
            }
        }
    }

    /// <summary>
    /// 元のSplineContainerのリンク情報を新しいコンテナに引き継ぐ
    /// </summary>
    private void UpdateOriginalLinks(List<(GameObject gameObject, SplineContainer container)> containers)
    {
        SplineContainerLink originalLink = targetContainer.GetComponent<SplineContainerLink>();

        if (originalLink == null)
            return;

        // 最初のコンテナにoriginalLink.prevを引き継ぐ
        if (originalLink.prev != null)
        {
            var firstLink = containers[0].gameObject.GetComponent<SplineContainerLink>();
            firstLink.prev = originalLink.prev;

            var prevLink = originalLink.prev.GetComponent<SplineContainerLink>();
            if (prevLink != null)
            {
                prevLink.next = containers[0].container;
            }
        }

        // 最後のコンテナにoriginalLink.nextを引き継ぐ
        if (originalLink.next != null)
        {
            var lastLink = containers[containers.Count - 1].gameObject.GetComponent<SplineContainerLink>();
            lastLink.next = originalLink.next;

            var nextLink = originalLink.next.GetComponent<SplineContainerLink>();
            if (nextLink != null)
            {
                nextLink.prev = containers[containers.Count - 1].container;
            }
        }
    }

    /// <summary>
    /// 分割されたコンテナのメッシュを生成
    /// </summary>
    private void GenerateMeshes(List<(GameObject gameObject, SplineContainer container)> containers)
    {
        SplineMeshGenerator originalMeshGen = targetContainer.GetComponent<SplineMeshGenerator>();

        foreach (var (gameObject, container) in containers)
        {
            SplineMeshGenerator meshGen = gameObject.AddComponent<SplineMeshGenerator>();

            // 元のメッシュジェネレータがあれば設定をコピー
            if (originalMeshGen != null)
            {
                meshGen.CopySetting(originalMeshGen);
            }

            meshGen.Generate();
        }
    }

    /// <summary>
    /// 元のGameObjectを無効化または削除
    /// </summary>
    private void FinalizeOriginalContainer()
    {
        if (destroyTarget)
        {
            DestroyImmediate(targetContainer.gameObject);
        }
        else
        {
            targetContainer.gameObject.SetActive(false);
        }
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