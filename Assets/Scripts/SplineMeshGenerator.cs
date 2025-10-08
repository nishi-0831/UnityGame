using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Assets.Scripts
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(ProBuilderMesh))]
    public class SplineMeshGenerator : MonoBehaviour
    {
        [Header("断面サイズ設定 (Knot位置を原点として +X に幅, -Y に高さ)")]
        [SerializeField, Min(0.0001f)] private float height = 1.0f; // 下方向(-Y)
        [SerializeField, Min(0.0001f)] private float width = 1.0f;  // +X 方向

        [Header("参照 / オプション")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool generateEndCaps = true;  // Open Spline の両端に蓋を付ける
        [SerializeField] private bool autoGenerateOnPlay = false;

        private ProBuilderMesh pbMesh;

        private void Reset()
        {
            splineContainer = GetComponent<SplineContainer>();
            pbMesh = GetComponent<ProBuilderMesh>();
        }

        private void Awake()
        {
            if (splineContainer == null)
                splineContainer = GetComponent<SplineContainer>();
            if (pbMesh == null)
                pbMesh = GetComponent<ProBuilderMesh>();

            if (autoGenerateOnPlay && Application.isPlaying)
            {
                Generate();
            }
        }

        [ContextMenu("Generate ProBuilder Mesh From Spline")]
        public void Generate()
        {
            if (splineContainer == null || splineContainer.Spline == null)
            {
                Debug.LogError("SplineContainer / Spline が設定されていません");
                return;
            }
            var spline = splineContainer.Spline;
            int knotCount = spline.Count;
            if (knotCount < 2)
            {
                Debug.LogWarning("Knot が 2 つ未満のためメッシュを生成できません");
                return;
            }

            if (pbMesh == null)
                pbMesh = GetComponent<ProBuilderMesh>();

            // 座標は SplineContainer のローカル座標系と一致させる
            // Knotごとに4頂点を作成するため4を掛ける
            List<Vector3> positions = new List<Vector3>(knotCount * 4);
            List<Face> faces = new List<Face>();

            // Knot ごとに 4 頂点 (A:上左, B:上右, C:下左, D:下右) を登録
            // A = P
            // B = P + (width / 2, 0, 0)
            // C = P + (0, -height / 2, 0)
            // D = P + (width / 2, -height / 2, 0)


            for (int i = 0; i < knotCount; i++)
            {
                Vector3 pLocal = spline[i].Position; // 既にローカル座標
                Vector3 A = pLocal + Vector3.left * (width / 2);
                Vector3 B = pLocal + Vector3.right * (width / 2);
                Vector3 C = A + Vector3.down * (height / 2);
                Vector3 D = B + Vector3.down * (height / 2);
                positions.Add(A); // index +0
                positions.Add(B); // +1
                positions.Add(C); // +2
                positions.Add(D); // +3
            }

            bool closed = spline.Closed;
            // Knotの始点と終点がつながっている場合、辺の数は Knotの数 - 1 になる
            int segmentCount = closed ? knotCount : knotCount - 1;

            // セグメントごとの側面 + 上下面
            for (int seg = 0; seg < segmentCount; seg++)
            {
                // インデックスを用意
                int i0 = seg;
                // closedの場合は始点と終点をつなげる
                int i1 = (seg + 1) % knotCount; 

                int base0 = i0 * 4; // A,B,C,D
                int base1 = i1 * 4;

                // A,B,C,Dの点からなる面のインデックス
                int A0 = base0 + 0; int B0 = base0 + 1; int C0 = base0 + 2; int D0 = base0 + 3;
                int A1 = base1 + 0; int B1 = base1 + 1; int C1 = base1 + 2; int D1 = base1 + 3;

                // 上面 (法線 +Y) : A0, A1, B1, B0
                AddQuadFace(faces, A0, A1, B1, B0);
                // 下面 (法線 -Y) : C0, C1, D1, D0  (上面とは逆向きになるように)
                AddQuadFace(faces, D0, D1, C1, C0);
                // 側面1 (幅=0 平面) (法線 -X) : A0, C0, C1, A1
                AddQuadFace(faces, A0, C0, C1, A1);
                // 側面2 (幅=width 平面) (法線 +X) : B0, B1, D1, D0
                AddQuadFace(faces, B0, B1, D1, D0);

                // 始端に面を生成 (Open かつ 最初の辺のみ)
                if (!closed && generateEndCaps && seg == 0)
                {
                    AddQuadFace(faces, A0, B0, D0, C0); // 始端
                }
                // 終端に面を生成 (Open かつ 最後の辺のみ)
                if (!closed && generateEndCaps && seg == segmentCount - 1)
                {
                    AddQuadFace(faces, A1, C1, D1, B1); // 終端
                }
            }

            pbMesh.Clear();
            // 頂点座標と面から構築
            pbMesh.RebuildWithPositionsAndFaces(positions, faces);
            pbMesh.ToMesh();
            pbMesh.Refresh();

            Debug.Log($"Spline Mesh Generated: Knots={knotCount}, Segments={segmentCount}, Vertices={positions.Count}, Faces={faces.Count}");
        }

        private void AddQuadFace(List<Face> faces, int v0, int v1, int v2, int v3)
        {
            faces.Add(new Face(new int[] { v0, v1, v2, v0, v2, v3 }));
        }

#if UNITY_EDITOR
        // エディタで手動プレビューしたい場合に Inspector 変更で都度再生成したい場合はコメント解除
        //private void OnValidate()
        //{
        //    if (!Application.isPlaying)
        //    {
        //        if (splineContainer == null)
        //            splineContainer = GetComponent<SplineContainer>();
        //        if (pbMesh == null)
        //            pbMesh = GetComponent<ProBuilderMesh>();
        //        if (splineContainer != null && splineContainer.Spline != null && splineContainer.Spline.Count >= 2)
        //            Generate();
        //    }
        //}
#endif
    }
}