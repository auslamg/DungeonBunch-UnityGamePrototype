using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Spherecast")]
    [SerializeField] float radiusMultiplier = 0.8f;
    [SerializeField] float maxDistance = 1f;
    [SerializeField] LayerMask layerMask;
    Vector3 Origin => gameObject.transform.position - new Vector3(0, capsuleCollider.bounds.extents.y / 2, 0);
    Vector3 CastedOrigin => Origin + Vector3.down * ranDistance;
    Vector3 ContactPoint => IsGrounded ? hitInfo.point : CastedOrigin;
    float Radius => capsuleCollider.radius * radiusMultiplier;

    [Header("Cache")]
    [SerializeField] private bool IsGrounded;
    [SerializeField] private RaycastHit hitInfo;
    [SerializeField] private float ranDistance = 0;

    [Header("References")]
    [SerializeField] CapsuleCollider capsuleCollider;

    private Ray DownRay => new(Origin, Vector3.down);

    void OnValidate()
    {
        TryGetComponent(out capsuleCollider);
    }

    public bool isGrounded()
    {
        IsGrounded = Physics.SphereCast(DownRay, Radius, out hitInfo, maxDistance, layerMask);
        ranDistance = IsGrounded ? hitInfo.distance : maxDistance;

        return IsGrounded;
    }

    public bool isGrounded(out RaycastHit hitInfo)
    {
        IsGrounded = Physics.SphereCast(DownRay, Radius, out this.hitInfo, maxDistance, layerMask);
        hitInfo = this.hitInfo;
        ranDistance = IsGrounded ? hitInfo.distance : maxDistance;

        return IsGrounded;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(Origin, 0.02f);

        Handles.DrawDottedLine(Origin, Origin + Vector3.down * ranDistance, 1);

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Handles.color = IsGrounded ? Color.green : Color.red;

        Gizmos.DrawSphere(Origin + Vector3.down * ranDistance, 0.05f);
        Gizmos.DrawWireSphere(Origin + Vector3.down * ranDistance, capsuleCollider.radius * radiusMultiplier);

        if (IsGrounded)
        {
            Handles.DrawDottedLine(Origin + Vector3.down * ranDistance, ContactPoint, 1);
            Gizmos.DrawSphere(ContactPoint, 0.05f);
        }
    }
}
