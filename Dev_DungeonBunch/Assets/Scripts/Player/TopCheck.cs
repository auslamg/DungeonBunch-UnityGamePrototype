using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TopCheck : MonoBehaviour
{
    [Header("Spherecast")]
    [SerializeField] float radiusMultiplier = 0.8f;
    [SerializeField] float maxDistance = 1f;
    [SerializeField] LayerMask layerMask;
    Vector3 Origin => capsuleCollider.transform.TransformPoint(capsuleCollider.center) + new Vector3(0, capsuleCollider.bounds.extents.y - capsuleCollider.radius, 0);
    Vector3 CastedOrigin => Origin + Vector3.up * ranDistance;
    Vector3 ContactPoint => isTopped ? hitInfo.point : CastedOrigin;
    float Radius => capsuleCollider.radius * radiusMultiplier;

    [Header("Cache")]
    [SerializeField] private bool isTopped;
    [SerializeField] private RaycastHit hitInfo;
    [SerializeField] private float ranDistance = 0;

    [Header("References")]
    [SerializeField] CapsuleCollider capsuleCollider;

    private Ray UpRay => new(Origin, Vector3.up);

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        TryGetComponent(out capsuleCollider);
    }

    public bool IsTopped()
    {
        isTopped = Physics.SphereCast(UpRay, Radius, out hitInfo, maxDistance, layerMask);
        ranDistance = isTopped ? hitInfo.distance : maxDistance;

        return isTopped;
    }

    public bool IsTopped(out RaycastHit hitInfo)
    {
        isTopped = Physics.SphereCast(UpRay, Radius, out this.hitInfo, maxDistance, layerMask);
        hitInfo = this.hitInfo;
        ranDistance = isTopped ? hitInfo.distance : maxDistance;

        return isTopped;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(Origin, 0.02f);

        Handles.DrawDottedLine(Origin, Origin + Vector3.up * ranDistance, 1);

        Gizmos.color = isTopped ? Color.green : Color.red;
        Handles.color = isTopped ? Color.green : Color.red;

        Gizmos.DrawSphere(Origin + Vector3.up * ranDistance, 0.05f);
        Gizmos.DrawWireSphere(Origin + Vector3.up * ranDistance, capsuleCollider.radius * radiusMultiplier);

        if (isTopped)
        {
            Handles.DrawDottedLine(Origin + Vector3.up * ranDistance, ContactPoint, 1);
            Gizmos.DrawSphere(ContactPoint, 0.05f);
        }
    }
}
