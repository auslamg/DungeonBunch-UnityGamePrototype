using System;
using UnityEditor;
using UnityEngine;

public static class GizmosUtil
{
    public static void DrawWireCapsule(CapsuleCollider capsuleCollider)
    {
        float coreHeight = capsuleCollider.bounds.extents.y - capsuleCollider.radius;

        Vector3 center = 
            capsuleCollider.transform.position + capsuleCollider.center;
        Vector3 top =
            center + coreHeight * Vector3.up;
        Vector3 bottom =
            center - coreHeight * Vector3.up;
        float radius = capsuleCollider.radius;

        // Top hemisphere
        Handles.DrawWireArc(top, Vector3.right, Vector3.forward, -180f, radius);
        Handles.DrawWireArc(top, Vector3.forward, Vector3.right, 180f, radius);
        Handles.DrawWireArc(top, Vector3.up, Vector3.right, 360f, radius);

        // Bottom hemisphere
        Handles.DrawWireArc(bottom, Vector3.right, Vector3.back, -180f, radius);
        Handles.DrawWireArc(bottom, Vector3.forward, Vector3.left, 180f, radius);
        Handles.DrawWireArc(bottom, Vector3.down, Vector3.left, 360f, radius);

        Vector3 wire1 = top + radius * Vector3.forward;
        Vector3 wire2 = top + radius * Vector3.left;
        Vector3 wire3 = top + radius * Vector3.back;
        Vector3 wire4 = top + radius * Vector3.right;

        Gizmos.DrawLine(wire1, wire1 - coreHeight * 2 * Vector3.up);
        Gizmos.DrawLine(wire2, wire2 - coreHeight * 2 * Vector3.up);
        Gizmos.DrawLine(wire3, wire3 - coreHeight * 2 * Vector3.up);
        Gizmos.DrawLine(wire4, wire4 - coreHeight * 2 * Vector3.up);
    }

    public static void WithGizmoMatrix(Matrix4x4 matrix, Action drawAction)
    {
        Matrix4x4 previous = Gizmos.matrix;

        Gizmos.matrix = matrix;
        drawAction?.Invoke();
        Gizmos.matrix = previous;
    }
}
