using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    [Header("Clamping")]
    [Range(70, 90)][SerializeField] private float maxYAngle = 89;

    [Header("Lerping")]
    [SerializeField] private float lerpFactor = 5;

    [Header("References")]
    [SerializeField] private Transform target;
    public Transform Target
    {
        get => target;
        set => target = value;
    }
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer mesh;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        rb =
            rb != null ?
                rb :
                GetComponentInParent<Actor>().GetComponentInChildren<Rigidbody>();

        mesh =
            mesh != null ?
                mesh :
                GetComponentInParent<Actor>().GetComponentInChildren<MeshRenderer>();

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.I))
        {
            LookAtPosition(target.transform.position);
            Debug.Log("Looking at target");
        }
        ClampPitch();
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        UpdatePlayerModelDirection();
    }

    private void UpdatePlayerModelDirection()
    {
        // TODO: Implement
        // mesh.transform.rotation = transform.rotation;
    }

    public bool LookAtTarget()
    {
        if (target)
        {
            SetRotation(Quaternion.LookRotation(target.position - transform.position));
            return true;
        }
        return false;
    }

    public bool LookAtTargetLerped()
    {
        if (target)
        {
            SetRotation(
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(target.position - transform.position),
                    Time.deltaTime * lerpFactor));
            return true;
        }
        return false;
    }

    public void LookAtPosition(Vector3 position)
    {
        SetRotation(Quaternion.LookRotation(position - transform.position));
    }

    public void LookAtPositionLerped(Vector3 position)
    {
        SetRotation(
            Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(position - transform.position),
                Time.deltaTime * lerpFactor));
    }

    public void LookAtMovementDirection()
    {
        SetRotation(Quaternion.Euler(rb.linearVelocity));
    }

    public void LookAtMovementDirectionLerped()
    {
        SetRotation(
            Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(rb.linearVelocity),
                Time.deltaTime * lerpFactor));
    }

    /// <summary>
    /// Clamps the camera's pitch to the configured vertical limit.
    /// Unity can sometimes represent the same orientation with a wrapped Euler angle,
    /// so this method normalizes that case before applying the clamp.
    /// </summary>
    private void ClampPitch()
    {
        // Normalize the angle first so Unity's wrapped Euler values do not confuse the pitch range.
        Vector3 eulerRotation = NormalizeWrappedEuler(transform.rotation.eulerAngles);

        // Convert the pitch into a signed value centered around zero before clamping it.
        float signedAngle = Mathf.DeltaAngle(0f, eulerRotation.x);
        signedAngle = Mathf.Clamp(signedAngle, -maxYAngle, maxYAngle);

        // Reapply the clamped pitch to the rotation.
        eulerRotation.x = signedAngle;
        transform.rotation = Quaternion.Euler(eulerRotation);
    }

    /// <summary>
    /// Converts Unity's wrapped Euler representation of a pitch-only rotation back into
    /// a regular angle when the yaw and roll are effectively at 180 degrees.
    /// </summary>
    private Vector3 NormalizeWrappedEuler(Vector3 eulerRotation)
    {
        // Unity can report a pitch-only rotation as (70, 180, 180) instead of (110, 0, 0).
        // In that case, flip the pitch back into the expected range and clear the bogus axes.
        bool isWrappedPitch = Mathf.Abs(eulerRotation.y - 180f) < 0.001f && Mathf.Abs(eulerRotation.z - 180f) < 0.001f;

        if (isWrappedPitch)
        {
            eulerRotation.x = 180f - eulerRotation.x;
            eulerRotation.y = 0f;
            eulerRotation.z = 0f;
        }

        return eulerRotation;
    }
}
