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
    [Header("�Ώۃ��b�V��")]
    [SerializeField] private ProBuilderMesh targetMesh_;

    [Header("�X�v���C���ݒ�")]
    [SerializeField] private SplineContainer splineContainer_;

    [Header("���������")]
    [SerializeField, Range(0.1f, 1.0f)] private float upwardThreshold_ = 0.5f;

    [Header("�f�o�b�O")]
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
            Debug.LogError("ProBuilderMesh �܂��� SplineContainer ���ݒ肳��Ă��܂���");
            return;
        }
        KnotPositions_.Clear();

        //ProBuilderMesh���猻�ݑI�𒆂̕Ӄf�[�^���擾
        var edges = targetMesh_.selectedEdges;
        var vertices = targetMesh_.positions;
        if(edges.Count <= 0)
        {
            Debug.LogError("edge���I������Ă��܂���");
            return;
        }
        for(int i=0;i<edges.Count;i++)
        {
            Vector3 a = vertices[edges[i].a];
            KnotPositions_.Add(targetMesh_.transform.TransformPoint(a));
        }
        Vector3 last = vertices[edges.Last().b];
        KnotPositions_.Add(targetMesh_.transform.TransformPoint(last));

        //�X�v���C����Knot��ǉ�
        CreateSplineFromKnots();

        Debug.Log($"���v {KnotPositions_.Count} ��Knot���쐬���܂���");
    }
    [ContextMenu("Create Knots from Upward Faces")]
    public void CreateKnotsFromUpwardFaces()
    {
        if(targetMesh_ == null || splineContainer_ == null)
        {
            Debug.LogError("ProBuilderMesh �܂��� SplineContainer ���ݒ肳��Ă��܂���");
            return;
        }

        KnotPositions_.Clear();

        //ProBuilderMesh����ʃf�[�^���擾
        var faces = targetMesh_.faces;
        var vertices = targetMesh_.positions;
        //�ʖ@������Ȃ��Ē��_�@��
        var normals = targetMesh_.normals;

        Debug.Log($"�ʂ̐�:{faces.Count}");
        Debug.Log($"���_�̐�:{vertices.Count}");
        Debug.Log($"�@���̐�:{normals.Count}");
        for( int i = 0;i < faces.Count; i++ )
        {
            Vector3 faceNormal = CalculateFaceNormal(faces[i],normals);
            float dot = Vector3.Dot(Vector3.up,faceNormal);
            
            //��߂�臒l�ȏ�Ȃ������̖ʂƔ���
            if (dot >= upwardThreshold_)
            {
                //�ʂ̒��S�ʒu���v�Z
                Vector3 faceCenter = CalculateFaceCenter(faces[i], vertices);

                //���[���h���W�n�ɕϊ�
                //scale�̒l�ŕϊ�����B����Ŋg�k���ĂĂ��_�C�W���u
                Vector3 worldPosition = targetMesh_.transform.TransformPoint(faceCenter);

                KnotPositions_.Add(worldPosition);
                Debug.Log($"������̖ʂ𔭌�: �@��={faceNormal},���S={worldPosition}");
            }
            else
            {
                //Debug.Log("Dot:" + dot);
            }
        }
       

        //�X�v���C����Knot��ǉ�
        CreateSplineFromKnots();

        Debug.Log($"���v {KnotPositions_.Count} ��Knot���쐬���܂���");
    }
    //https://docs.unity3d.com/Packages/com.unity.probuilder@4.0/api/UnityEngine.ProBuilder.Face.html

    private Vector3 CalculateFaceCenter(Face face,IList<Vector3> vertices)
    {
        Vector3 center = Vector3.zero;
        //face�̃C���f�b�N�X�o�b�t�@�I�Ȃ���?
        var indexes = face.indexes;

        for(int i = 0; i < indexes.Count; i++)
        {
            
            //�O�̂��ߔ͈͊O�ɃA�N�Z�X���Ȃ���
            //vertices��face�̂��̂łȂ��\��������
            if (indexes[i] < vertices.Count)
            {
                center += vertices[indexes[i]];
            }
        }
        //�ʂ̊e���_�̍��W�𒸓_���Ŋ���Ƃ��̒��S���W�����߂���
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
            Debug.LogWarning("�쐬����Knot������܂���");
            return;
        }

        //������Spline���N���A
        if(splineContainer_.Spline == null)
        {
            Debug.LogError($"{splineContainer_.name}��Spline��null�ł��BSpline���쐬���Ă�������");

        }
        splineContainer_.Spline.Clear();

        //Knot���\�[�g (�� : X���W��)
        //var sortedPositions = knotPositions_.OrderBy(pos => pos.x).ToList();
        //�{���͂��̃��b�V���̒[����[�ֈ�����Ɍq����悤�Ƀ\�[�g���������A�܂����@���肩�łȂ��B�ň����Ƃ�?�ق�Ƃɍň�������
        //�����_�ł̓f�t�H���g�Œ[����[�ւ̏��ԂŖʂ����ׂ��Ă���̂ŗǂ��Ƃ���
        

        //�X�v���C����Knot��ǉ�
        for (int i = 0;i < KnotPositions_.Count;i++)
        {
            Vector3 localPosition = splineContainer_.transform.InverseTransformPoint(KnotPositions_[i]);

            //BezierKnot���쐬
            var Knot = new BezierKnot(localPosition);
            
            //�X�v���C���ɒǉ�
            splineContainer_.Spline.Add(Knot);
        }
        Debug.Log($"�X�v���C����{KnotPositions_.Count}��Knot��ǉ����܂���");
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
        //IEnumerable��List�ɂ���Reverse()
        //splineContainer_.Spline.Knots.ToList().Reverse();
        //�悭�悭�l������knot��tangent�t�ɂ��Ȃ��Ƃ����
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
