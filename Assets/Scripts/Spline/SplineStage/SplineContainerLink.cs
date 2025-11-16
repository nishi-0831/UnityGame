using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class SplineContainerLink : MonoBehaviour
{
    //public SplineContainer splineContainer;

    public SplineContainer prev;
    public SplineContainer next;
    public bool adjustVerticalPos = false;
    [ContextMenu("Adjust First And End Knot Pos")]
    void AdjustKnotPos()
    {
        var container = GetComponent<SplineContainer>();
        if (container == null) return;
        Spline spline = container.Spline;

        // Align prev's last knot to this spline's first knot (in world space)
        if (prev)
        {
            int prevLastIndex = Mathf.Max(0, prev.Spline.Count - 1);
            var prevLastKnot = prev.Spline[prevLastIndex];
            var currFirstKnot = spline[0];

            // Current first knot in world space
            Vector3 currFirstWorldPos = container.transform.TransformPoint(currFirstKnot.Position);
            Quaternion currFirstWorldRot = container.transform.rotation * currFirstKnot.Rotation;

            // Convert world -> prev local
            Vector3 prevLocalPos = prev.transform.InverseTransformPoint(currFirstWorldPos);
            Quaternion prevLocalRot = Quaternion.Inverse(prev.transform.rotation) * currFirstWorldRot;

            // Preserve previous vertical position if requested
            if (!adjustVerticalPos)
            {
                prevLocalPos.y = prevLastKnot.Position.y;
            }

            prevLastKnot.Position = prevLocalPos;
            prevLastKnot.Rotation = prevLocalRot;

            prev.Spline[prevLastIndex] = prevLastKnot;
        }

        // Align next's first knot to this spline's last knot (in world space)
        if (next)
        {
            var nextFirstKnot = next.Spline.First();
            var currLastKnot = spline.Last();

            Vector3 currLastWorldPos = container.transform.TransformPoint(currLastKnot.Position);
            Quaternion currLastWorldRot = container.transform.rotation * currLastKnot.Rotation;

            Vector3 nextLocalPos = next.transform.InverseTransformPoint(currLastWorldPos);
            Quaternion nextLocalRot = Quaternion.Inverse(next.transform.rotation) * currLastWorldRot;

            if (!adjustVerticalPos)
            {
                nextLocalPos.y = nextFirstKnot.Position.y;
            }

            nextFirstKnot.Position = nextLocalPos;
            nextFirstKnot.Rotation = nextLocalRot;

            next.Spline[0] = nextFirstKnot;
        }

#if UNITY_EDITOR
        //// Mark dirty so changes persist in editor
        //UnityEditor.EditorUtility.SetDirty(container);
        //if (prev) UnityEditor.EditorUtility.SetDirty(prev);
        //if (next) UnityEditor.EditorUtility.SetDirty(next);
        //UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
    }
}
