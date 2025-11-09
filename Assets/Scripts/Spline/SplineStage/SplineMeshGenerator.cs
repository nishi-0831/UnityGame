using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    public class SplineMeshGenerator : MonoBehaviour
    {
        [Header("断面サイズ設定 (Knot位置を原点として +X に幅, -Y に高さ)")]
        [SerializeField, Min(0.0001f)] private float height = 1.0f; // 下方向(-Y)
        [SerializeField, Min(0.0001f)] private float width = 1.0f;  // +X 方向

        [Header("参照 / オプション")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool generateEndCaps = true;  // Open Spline の両端に蓋を付ける
        [SerializeField] private bool autoGenerateOnPlay = false;
        

        [Header("Collider 生成オプション")]
        [SerializeField] private bool addCollider = true;
        private List<Collider> createdColliders = new List<Collider>();

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
            // C = P + (0, -height, 0)
            // D = P + (width / 2, -height, 0)


            for (int i = 0; i < knotCount; i++)
            {
                Vector3 pLocal = spline[i].Position; // 既にローカル座標
                Quaternion rotLocal = spline[i].Rotation;
                Vector3 A = pLocal + rotLocal * Vector3.left * (width / 2);
                Vector3 B = pLocal + rotLocal * Vector3.right * (width / 2);
                Vector3 C = B + rotLocal * Vector3.down * (height);
                Vector3 D = A + rotLocal * Vector3.down * (height);
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
                AddQuadFace(faces, C0, C1, D1, D0);
                // 側面1 (幅=0 平面) (法線 -X) : A0, C0, C1, A1
                AddQuadFace(faces, A1, A0,D0, D1);
                // 側面2 (幅=width 平面) (法線 +X) : B0, B1, D1, D0
                AddQuadFace(faces, C1, C0,B0, B1);

                // 始端に面を生成 (Open かつ 最初の辺のみ)
                if (!closed && generateEndCaps && seg == 0)
                {
                    AddQuadFace(faces, A0, B0, C0, D0); // 始端
                }
                // 終端に面を生成 (Open かつ 最後の辺のみ)
                if (!closed && generateEndCaps && seg == segmentCount - 1)
                {
                    AddQuadFace(faces, D1, C1, B1, A1); // 終端
                }
            }

            pbMesh.Clear();
            // 頂点座標と面から構築
            pbMesh.RebuildWithPositionsAndFaces(positions, faces);
            pbMesh.ToMesh();
            pbMesh.Refresh();

            if(addCollider)
            {
                CleanupCreatedColliders();
                //CreateColliderFromGeneratedMesh(positions, segmentCount);
                BuildSegmentChildColliders();
            }
            //pbMesh.マテリアル...
            EnsureDefaultMaterial();
            Debug.Log($"Spline Mesh Generated: Knots={knotCount}, Segments={segmentCount}, Vertices={positions.Count}, Faces={faces.Count}");
        }

        
        private void AddQuadFace(List<Face> faces, int v0, int v1, int v2, int v3)
        {
            faces.Add(new Face(new int[] { v0, v1, v2, v0, v2, v3 }));
        }

        private void EnsureDefaultMaterial()
        {
            var renderer = pbMesh.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            
            // マテリアルが未設定または nullの場合
            if (renderer.sharedMaterial == null || renderer.sharedMaterials.Length == 0)
            {
                // Resources/DefaultMaterial.matを使う
                Material defaultMat = Resources.Load<Material>("DefaultMaterial");
                if(defaultMat != null)
                {
                    renderer.sharedMaterial = defaultMat;
                }
                else
                {
                    Debug.LogWarning("DefaultMaterialがResourcesに存在しません。マテリアルを手動で設定してください");
                }
            }
        }
        private void CreateColliderFromGeneratedMesh(List<Vector3> positions,int segmentCount)
        {
            int knotCount = positions.Count / 4;
            for(int seg = 0; seg < segmentCount; seg++)
            {
                int i0 = seg;
                int i1 = (seg + 1) % knotCount;

                int base0 = i0 * 4;
                int base1 = i1 * 4;
                Vector3[] verts = new Vector3[8]
                {
                    positions[base0 + 0] ,
                    positions[base0 + 1] ,
                    positions[base0 + 2] ,
                    positions[base0 + 3] ,
                    positions[base1 + 0] ,
                    positions[base1 + 1] ,
                    positions[base1 + 2] ,
                    positions[base1 + 3] ,
                };

                Vector3 min = verts[0];
                Vector3 max = verts[0];
                for(int i = 1; i < verts.Length; i++)
                {
                    min = Vector3.Min(min, verts[i]);
                    max = Vector3.Max(max, verts[i]);
                }

                Vector3 center = (min + max) / 2;
                Vector3 size = max - min;

                BoxCollider boxCollider =  this.AddComponent<BoxCollider>();
                boxCollider.center = center;
                boxCollider.size = size;
                createdColliders.Add(boxCollider);
            }
        }
        private void CleanupCreatedColliders()
        {
            for(int i = createdColliders.Count - 1; i >= 0; i--)
            {
                var c = createdColliders[i];
                if(c != null )
                {
                    // 子オブジェクト全体を削除
                    if(c.gameObject != null)
                    {
                        DestroyImmediate(c.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(c);
                    }
                }
                createdColliders.RemoveAt(i);
            }
        }
        [ContextMenu("Generate Collider")]
        private void BuildSegmentChildColliders()
        {
            var spline = splineContainer.Spline;
            int knotCount = spline.Count;
            bool closed = spline.Closed;
            int segmentCount = closed ? knotCount : knotCount - 1;

            CleanupCreatedColliders();

            for (int seg = 0; seg < segmentCount; seg++)
            {
                // セグメントのKnotインデックス
                int knotIndex0 = seg;
                int knotIndex1 = closed ? (seg + 1) % knotCount : seg + 1;

                // 各セグメントのKnotから直接位置を取得
                BezierKnot knot0 = spline[knotIndex0];
                BezierKnot knot1 = spline[knotIndex1];
                
                Vector3 p0 = knot0.Position;
                Vector3 p1 = knot1.Position;
                
                // セグメントの中心点を計算（2つのKnotの中点）
                Vector3 centerPos = (p0 + p1) * 0.5f;
                
                // セグメントの方向ベクトル（p0からp1へ）
                Vector3 segmentDirection = (p1 - p0).normalized;
                
                // セグメントの長さ
                float segmentLength = Vector3.Distance(p0, p1);
                
                // Y軸（上方向）を維持した回転を計算
                Quaternion rotation = Quaternion.LookRotation(segmentDirection, Vector3.up);
                
                // 子GameObjectを作成
                GameObject child = new GameObject($"SegmentCollider_{seg}");
                child.transform.SetParent(transform, false);
                child.transform.localPosition = centerPos;
                child.transform.localRotation = rotation;

                // 参照用にSplineContainerを渡す
                var refSplineContainer = child.AddComponent<SplineColliderReference>();
                refSplineContainer.splineContainer = splineContainer;

                // BoxColliderを設定
                var box = child.AddComponent<BoxCollider>();
                box.size = new Vector3(width, height, segmentLength);
                box.center = new Vector3(0, -height * 0.5f, 0); // Y中心をメッシュに合わせて調整

                // BoxColliderの参照を保持
                createdColliders.Add(box);
            }
        }

        private void EvaluateSpline(float t , out Vector3 pos,out Vector3 tangent,out Vector3 up)
        {
            var nativeSpline = new NativeSpline(splineContainer.Spline, splineContainer.transform.localToWorldMatrix);
            Unity.Mathematics.float3 p, tan, u;
            SplineUtility.Evaluate(nativeSpline, t, out p, out tan, out u);
            pos = transform.InverseTransformPoint((Vector3)p);
            tangent = transform.InverseTransformDirection((Vector3)tan);
            up = transform.InverseTransformDirection((Vector3)u);
        }

        //private float ApproxSegmentArcLength(float t0,float t1,int samples)
        //{
        //    float len = 0f;
        //    Vector3 prev; Vector3 tmp;
        //    Vector3 tan, up;
        //    EvaluateSpline(t0, out prev, out tan, out up);
        //    //for(int i = 1; i < )
        //}
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