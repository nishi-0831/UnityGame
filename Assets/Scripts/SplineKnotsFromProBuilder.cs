using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;
using Unity.Splines.Examples;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class SplineKnotsFromProBuilder : MonoBehaviour
{
    //[SerializeField] private 
    [Header("対象メッシュ")]
    [SerializeField] private ProBuilderMesh targetMesh_;

    [Header("スプライン設定")]
    [SerializeField] private SplineContainer splineContainer_;

    [Header("上向き判定")]
    [SerializeField, Range(0.1f, 1.0f)] private float upwardThreshold_ = 0.5f;

    [Header("デバッグ")]
    [SerializeField] private bool showDebugSpheres_ = true;
    [SerializeField] private Color debugSphereColor_ = Color.red;

    private List<Vector3> KnotPositions_ = new List<Vector3>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(targetMesh_ == null)
        {
            targetMesh_ = GetComponent<ProBuilderMesh>();
        }
        if(splineContainer_ == null)
        {
            splineContainer_ = GetComponent<SplineContainer>();
        }
        //CreateKnotsFromUpwardFaces();
    }
    [ContextMenu("Create Knots from Selected Edges")]
    public void CreateKnotsFromSelectedEdges()
    {
        if (targetMesh_ == null || splineContainer_ == null)
        {
            Debug.LogError("ProBuilderMesh または SplineContainer が設定されていません");
            return;
        }
        KnotPositions_.Clear();

        //ProBuilderMeshから現在選択中の辺データを取得
        var edges = targetMesh_.selectedEdges;
        var vertices = targetMesh_.positions;
        if(edges.Count <= 0)
        {
            Debug.LogError("edgeが選択されていません");
            return;
        }
        for(int i=0;i<edges.Count;i++)
        {
            Vector3 a = vertices[edges[i].a];
            KnotPositions_.Add(targetMesh_.transform.TransformPoint(a));
        }
        Vector3 last = vertices[edges.Last().b];
        KnotPositions_.Add(targetMesh_.transform.TransformPoint(last));

        //スプラインにKnotを追加
        CreateSplineFromKnots();

        Debug.Log($"合計 {KnotPositions_.Count} 個のKnotを作成しました");
    }
    [ContextMenu("Create Knots from Upward Faces")]
    public void CreateKnotsFromUpwardFaces()
    {
        if(targetMesh_ == null || splineContainer_ == null)
        {
            Debug.LogError("ProBuilderMesh または SplineContainer が設定されていません");
            return;
        }

        KnotPositions_.Clear();

        //ProBuilderMeshから面データを取得
        var faces = targetMesh_.faces;
        var vertices = targetMesh_.positions;
        //面法線じゃなくて頂点法線
        var normals = targetMesh_.normals;

        Debug.Log($"面の数:{faces.Count}");
        Debug.Log($"頂点の数:{vertices.Count}");
        Debug.Log($"法線の数:{normals.Count}");
        for( int i = 0;i < faces.Count; i++ )
        {
            Vector3 faceNormal = CalculateFaceNormal(faces[i],normals);
            float dot = Vector3.Dot(Vector3.up,faceNormal);
            
            //定めた閾値以上なら上向きの面と判定
            if (dot >= upwardThreshold_)
            {
                //面の中心位置を計算
                Vector3 faceCenter = CalculateFaceCenter(faces[i], vertices);

                //ワールド座標系に変換
                //scaleの値で変換する。これで拡縮しててもダイジョブ
                Vector3 worldPosition = targetMesh_.transform.TransformPoint(faceCenter);

                KnotPositions_.Add(worldPosition);
                Debug.Log($"上向きの面を発見: 法線={faceNormal},中心={worldPosition}");
            }
            else
            {
                //Debug.Log("Dot:" + dot);
            }
        }
       

        //スプラインにKnotを追加
        CreateSplineFromKnots();

        Debug.Log($"合計 {KnotPositions_.Count} 個のKnotを作成しました");
    }
    //https://docs.unity3d.com/Packages/com.unity.probuilder@4.0/api/UnityEngine.ProBuilder.Face.html

    private Vector3 CalculateFaceCenter(Face face,IList<Vector3> vertices)
    {
        Vector3 center = Vector3.zero;
        //faceのインデックスバッファ的なもの?
        var indexes = face.indexes;

        for(int i = 0; i < indexes.Count; i++)
        {
            
            //念のため範囲外にアクセスしないか
            //verticesがfaceのものでない可能性がある
            if (indexes[i] < vertices.Count)
            {
                center += vertices[indexes[i]];
            }
        }
        //面の各頂点の座標を頂点数で割るとその中心座標が求められる
        return center / indexes.Count;
    }
    private Vector3 CalculateFaceNormal(Face face,IList<Vector3> normals)
    {
        Vector3 nomal = Vector3.zero;
        var indexes = face.indexes;
        
        for (int i = 0; i < indexes.Count; i++)
        {
            if (indexes[i] < normals.Count)
            {
                nomal += normals[indexes[i]];
            }
        }
        return nomal / indexes.Count;
    }

    
    private void CreateSplineFromKnots()
    {
        if(KnotPositions_.Count == 0)
        {
            Debug.LogWarning("作成するKnotがありません");
            return;
        }

        //既存のSplineをクリア
        if(splineContainer_.Spline == null)
        {
            Debug.LogError($"{splineContainer_.name}のSplineがnullです。Splineを作成してください");

        }
        splineContainer_.Spline.Clear();

        //Knotをソート (例 : X座標順)
        //var sortedPositions = knotPositions_.OrderBy(pos => pos.x).ToList();
        //本当はそのメッシュの端から端へ一方向に繋がるようにソートしたいが、まだ方法が定かでない。最悪手作業か?ほんとに最悪だけど
        //現時点ではデフォルトで端から端への順番で面が並べられているので良しとする
        

        //スプラインにKnotを追加
        for (int i = 0;i < KnotPositions_.Count;i++)
        {
            Vector3 localPosition = splineContainer_.transform.InverseTransformPoint(KnotPositions_[i]);

            //BezierKnotを作成
            var Knot = new BezierKnot(localPosition);
            
            //スプラインに追加
            splineContainer_.Spline.Add(Knot);
        }
        Debug.Log($"スプラインに{KnotPositions_.Count}個のKnotを追加しました");
        splineContainer_.Spline.SetTangentMode(TangentMode.AutoSmooth);
    }

    private void OnDrawGizmos()
    {
        if(!showDebugSpheres_ || KnotPositions_.Count == 0)
        {
            return;
        }

        Gizmos.color = debugSphereColor_;

        foreach(var position in KnotPositions_)
        {
            Gizmos.DrawSphere(position, 0.2f);
        }
    }
    [ContextMenu("Reverse Spline Knots")]
    public void ReverseSplineKnots()
    {
        //IEnumerableをListにしてReverse()
        //splineContainer_.Spline.Knots.ToList().Reverse();
        //よくよく考えたらknotのtangent逆にしないとじゃん
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
