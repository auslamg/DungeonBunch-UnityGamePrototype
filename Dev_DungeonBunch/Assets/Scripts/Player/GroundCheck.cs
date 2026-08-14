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
    Vector3 Origin => capsuleCollider.transform.TransformPoint(capsuleCollider.center) - new Vector3(0, capsuleCollider.bounds.extents.y - capsuleCollider.radius, 0);
    Vector3 CastedOrigin => Origin + Vector3.down * ranDistance;
    Vector3 ContactPoint => isGrounded ? hitInfo.point : CastedOrigin;
    float Radius => capsuleCollider.radius * radiusMultiplier;

    [Header("Cache")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private RaycastHit hitInfo;
    [SerializeField] private float ranDistance = 0;
    [SerializeField] private float maxSlopeAngle = 55f;
    [SerializeField] public bool isOnSlope = false;
    [SerializeField] public bool isOnSteepSlope = false;

    [Header("References")]
    [SerializeField] CapsuleCollider capsuleCollider;

    private Ray DownRay => new(Origin, Vector3.down);

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        TryGetComponent(out capsuleCollider);
    }

    public bool Check()
    {
        isGrounded = Physics.SphereCast(DownRay, Radius, out hitInfo, maxDistance, layerMask);
        ranDistance = isGrounded ? hitInfo.distance : maxDistance;

        isOnSlope = Vector3.Angle(Vector3.up, hitInfo.normal) > 0;
        isOnSteepSlope = Vector3.Angle(Vector3.up, hitInfo.normal) > maxSlopeAngle;

        return isGrounded;
    }

    public bool Check(out RaycastHit hitInfo)
    {
        isGrounded = Physics.SphereCast(DownRay, Radius, out this.hitInfo, maxDistance, layerMask);
        hitInfo = this.hitInfo;
        ranDistance = isGrounded ? hitInfo.distance : maxDistance;

        isOnSlope = Vector3.Angle(Vector3.up, hitInfo.normal) > 0;
        isOnSteepSlope = Vector3.Angle(Vector3.up, hitInfo.normal) > maxSlopeAngle;

        return isGrounded;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(Origin, 0.02f);

        Handles.DrawDottedLine(Origin, Origin + Vector3.down * ranDistance, 1);

        Gizmos.color = GroundCheckColor();
        Handles.color = GroundCheckColor();

        Gizmos.DrawSphere(Origin + Vector3.down * ranDistance, 0.05f);
        Gizmos.DrawWireSphere(Origin + Vector3.down * ranDistance, capsuleCollider.radius * radiusMultiplier);

        if (isGrounded)
        {
            Handles.DrawDottedLine(Origin + Vector3.down * ranDistance, ContactPoint, 1);
            Gizmos.DrawSphere(ContactPoint, 0.05f);
        }
    }

    Color GroundCheckColor()
    {
        return !isGrounded ?
            Color.red :

            isOnSteepSlope ?
                Color.darkOrange :

                isOnSlope ?
                    Color.yellowGreen : 

                    // Grounded
                    Color.green;
    }
}
