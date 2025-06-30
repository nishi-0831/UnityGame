using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(ProBuilderMesh))]
public class SplineKnotsFromProBuilder : MonoBehaviour
{
    // データソースの種類
    public enum SourceType
    {
        Vertices,   // 頂点
        Edges,      // 辺
        Faces       // 面
    }

    [Header("対象メッシュ")]
    [SerializeField] private ProBuilderMesh targetMesh_;

    [Header("スプライン設定")]
    [SerializeField] private SplineContainer splineContainer_;

    [Header("生成設定")]
    [SerializeField] private SourceType sourceType_ = SourceType.Vertices;
    
    [Header("選択した箇所から作るか否か")]
    [SerializeField] private bool createBySelected_ = true;

    [Header("法線による方向フィルタリング")]
    [SerializeField] private bool ignoreDiffDir_ = true;
    [Header("設定する方向をワールド座標系として使うか否か")]
    [SerializeField] private bool useAsWorldDir_ = true;
    [SerializeField] private Vector3 dir_ = Vector3.up;
    [SerializeField, Range(0.1f, 1.0f)] private float dirThreshold_ = 0.9f;

    [Header("デバッグ")]
    [SerializeField] private bool showDebugSpheres_ = true;
    [SerializeField] private Color debugSphereColor_ = Color.red;

    private List<Vector3> KnotPositions_ = new List<Vector3>();

    void Start()
    {
        if(targetMesh_ == null)
            targetMesh_ = GetComponent<ProBuilderMesh>();
        if(splineContainer_ == null)
            splineContainer_ = GetComponent<SplineContainer>();
    }

    private bool InitKnot()
    {
        KnotPositions_.Clear();
        if(splineContainer_ == null)
            splineContainer_ = gameObject.AddComponent<SplineContainer>();
        if(targetMesh_ == null)
            targetMesh_= GetComponent<ProBuilderMesh>();
        
        if (targetMesh_ == null || splineContainer_ == null)
        {
            Debug.LogError("ProBuilderMesh または SplineContainer が設定されていません");
            return false;
        }
        return true;
    }

    [ContextMenu("Create Knots")]
    public void CreateKnots()
    {
        CreateKnots(sourceType_, createBySelected_, ignoreDiffDir_);
    }

    [ContextMenu("Create Knots from Vertices")]
    public void CreateKnotsFromVertices()
    {
        CreateKnots(SourceType.Vertices, createBySelected_, ignoreDiffDir_);
    }

    [ContextMenu("Create Knots from Edges")]
    public void CreateKnotsFromEdges()
    {
        CreateKnots(SourceType.Edges, createBySelected_, ignoreDiffDir_);
    }

    [ContextMenu("Create Knots from Faces")]
    public void CreateKnotsFromFaces()
    {
        CreateKnots(SourceType.Faces, createBySelected_, ignoreDiffDir_);
    }

    public void CreateKnots(SourceType sourceType, bool useSelected, bool filterByDirection)
    {
        if (!InitKnot())
            return;

        var vertices = targetMesh_.positions;
        var normals = targetMesh_.normals;
        var sharedVertices = targetMesh_.sharedVertices;
        //追加済みのsharedGroupsを保持する、重複を許さないコレクション
        HashSet<int> processedSharedGroups = new HashSet<int>();

        switch (sourceType)
        {
            case SourceType.Vertices:
                ProcessVertices(useSelected, filterByDirection, vertices, normals, sharedVertices, processedSharedGroups);
                break;
            case SourceType.Edges:
                ProcessEdges(useSelected, filterByDirection, vertices, normals, sharedVertices, processedSharedGroups);
                break;
            case SourceType.Faces:
                ProcessFaces(useSelected, filterByDirection, vertices, normals);
                break;
        }

        CreateSplineFromKnots();
        Debug.Log($"{KnotPositions_.Count}個のKnotを作成しました（SourceType: {sourceType}, Selected: {useSelected}, FilterByDirection: {filterByDirection}）");
    }

    private void ProcessVertices(bool useSelected, bool filterByDirection, IList<Vector3> vertices, 
                                IList<Vector3> normals, IList<SharedVertex> sharedVertices, HashSet<int> processedGroups)
    {
        IEnumerable<int> targetVertices;
        
        if (useSelected)
        {
            targetVertices = targetMesh_.selectedVertices;
            if (!targetVertices.Any())
            {
                Debug.LogError("頂点が選択されていません");
                return;
            }
        }
        else
        {
            targetVertices = Enumerable.Range(0, vertices.Count);
        }

        foreach (int vertexIndex in targetVertices)
        {
            if (filterByDirection && !IsVertexDirectionValid(vertexIndex, normals))
                continue;

            ProcessVertexForKnot(vertexIndex, vertices, sharedVertices, processedGroups);
        }
    }

    private void ProcessEdges(bool useSelected, bool filterByDirection, IList<Vector3> vertices, 
                             IList<Vector3> normals, IList<SharedVertex> sharedVertices, HashSet<int> processedGroups)
    {
        IEnumerable<UnityEngine.ProBuilder.Edge> targetEdges;
        
        if (useSelected)
        {
            targetEdges = targetMesh_.selectedEdges;
            if (!targetEdges.Any())
            {
                Debug.LogError("辺が選択されていません");
                return;
            }
        }
        else
        {
            targetEdges = GetAllEdges();
        }

        foreach (var edge in targetEdges)
        {
            // 辺の両端の頂点を処理
            if (!filterByDirection || IsVertexDirectionValid(edge.a, normals))
                ProcessVertexForKnot(edge.a, vertices, sharedVertices, processedGroups);
            
            if (!filterByDirection || IsVertexDirectionValid(edge.b, normals))
                ProcessVertexForKnot(edge.b, vertices, sharedVertices, processedGroups);
        }
    }

    private void ProcessFaces(bool useSelected, bool filterByDirection, IList<Vector3> vertices, IList<Vector3> normals)
    {
        IEnumerable<Face> targetFaces;
        
        if (useSelected)
        {
            // ProBuilderMeshでは選択された面の取得はできない
            Debug.LogWarning("ProBuilderMeshでは選択された面の取得が制限されています。全ての面を処理します。");
            targetFaces = targetMesh_.faces;
        }
        else
        {
            targetFaces = targetMesh_.faces;
        }

        foreach (var face in targetFaces)
        {
            if (!filterByDirection)
            {
                continue;
            }
            Vector3 faceNormal = CalculateFaceNormal(face, normals);
            Vector3 dir = Vector3.zero;

            if (useAsWorldDir_)
            {
                dir = transform.TransformDirection(dir_);
            }
            else
            {
                dir = dir_;
            }

            float dot = Vector3.Dot(dir.normalized, faceNormal.normalized);    
            if (dot < dirThreshold_)
                continue;
            
            // 面の中心位置を計算
            Vector3 faceCenter = CalculateFaceCenter(face, vertices);
            Vector3 worldPosition = targetMesh_.transform.TransformPoint(faceCenter);
            KnotPositions_.Add(worldPosition);
        }
    }

    private bool IsVertexDirectionValid(int vertexIndex, IList<Vector3> normals)
    {
        if (vertexIndex >= normals.Count)
            return false;

        Vector3 vertexNormal = normals[vertexIndex];
        Vector3 dir = Vector3.zero;
        if(useAsWorldDir_)
        {
            dir = transform.TransformDirection(dir_);
        }
        else
        {
            dir = dir_;
        }
            float dot = Vector3.Dot(dir.normalized, vertexNormal.normalized);
        return dot >= dirThreshold_;
    }

    private IEnumerable<UnityEngine.ProBuilder.Edge> GetAllEdges()
    {
        // すべての辺を取得する（ProBuilderMeshから）
        var allEdges = new HashSet<UnityEngine.ProBuilder.Edge>();
        var faces = targetMesh_.faces;
        
        foreach (var face in faces)
        {
            var edges = face.edges;
            foreach (var edge in edges)
            {
                allEdges.Add(edge);
            }
        }
        
        return allEdges;
    }

    private void ProcessVertexForKnot(int vertexIndex, IList<Vector3> positions, 
                                     IList<SharedVertex> sharedVertices, HashSet<int> processedGroups)
    {
        int sharedGroupIndex = FindSharedVertexGroup(vertexIndex, sharedVertices);

        //不正、もしくはすでに作成済みでないグループなら
        if (sharedGroupIndex != -1 && !processedGroups.Contains(sharedGroupIndex))
        {
            //作成済みに追加
            processedGroups.Add(sharedGroupIndex);

            // sharedGroupIndex番目のsharedVertexの0番目(代表頂点)
            int representativeVertex = sharedVertices[sharedGroupIndex][0];
            Vector3 worldPosition = targetMesh_.transform.TransformPoint(positions[representativeVertex]);
            KnotPositions_.Add(worldPosition);
        }
    }

    private int FindSharedVertexGroup(int vertexIndex, IList<SharedVertex> sharedVertices)
    {
        //自身が所属しているsharedVertexの番号を探す
        for (int i = 0; i < sharedVertices.Count; i++)
        {
            if (sharedVertices[i].Contains(vertexIndex))
                return i;
        }
        return -1;
    }

    private Vector3 CalculateFaceCenter(Face face, IList<Vector3> vertices)
    {
        Vector3 center = Vector3.zero;
        var indexes = face.indexes;

        for (int i = 0; i < indexes.Count; i++)
        {
            if (indexes[i] < vertices.Count)
                center += vertices[indexes[i]];
        }
        
        return center / indexes.Count;
    }

    private Vector3 CalculateFaceNormal(Face face, IList<Vector3> normals)
    {
        Vector3 normal = Vector3.zero;
        var indexes = face.indexes;
        
        for (int i = 0; i < indexes.Count; i++)
        {
            if (indexes[i] < normals.Count)
                normal += normals[indexes[i]];
        }
        
        return normal / indexes.Count;
    }

    private void CreateSplineFromKnots()
    {
        if (KnotPositions_.Count == 0)
        {
            Debug.LogWarning("作成するKnotがありません");
            return;
        }

        if (splineContainer_.Spline == null)
        {
            Debug.LogError($"{splineContainer_.name}のSplineがnullです。Splineを作成してください");
            return;
        }

        splineContainer_.Spline.Clear();

        // スプラインにKnotを追加
        for (int i = 0; i < KnotPositions_.Count; i++)
        {
            Vector3 localPosition = splineContainer_.transform.InverseTransformPoint(KnotPositions_[i]);
            var knot = new BezierKnot(localPosition);

            splineContainer_.Spline.Add(knot);
        }

        splineContainer_.Spline.SetTangentMode(TangentMode.AutoSmooth);
        Debug.Log($"スプラインに{KnotPositions_.Count}個のKnotを追加しました");
    }

    private void OnDrawGizmos()
    {
        if (!showDebugSpheres_ || KnotPositions_.Count == 0)
            return;

        Gizmos.color = debugSphereColor_;
        foreach (var position in KnotPositions_)
        {
            Gizmos.DrawSphere(position, 0.2f);
        }
    }

    [ContextMenu("Reverse Spline Knots")]
    public void ReverseSplineKnots()
    {
        if (splineContainer_.Spline == null || splineContainer_.Spline.Count == 0)
            return;

        var knotsList = splineContainer_.Spline.ToArray().Reverse().ToArray();
        splineContainer_.Spline.Clear();
        
        foreach (var knot in knotsList)
        {
            splineContainer_.Spline.Add(knot);
        }
        
        Debug.Log("スプラインのKnotを逆順にしました");
    }

    void Update()
    {
        // 実行時更新が必要な場合
    }
}
