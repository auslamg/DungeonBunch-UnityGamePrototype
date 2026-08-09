using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float accelerationMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpForceMultiplier = 1f;
    [SerializeField] private CountdownTimer jumpCooldown;
    [SerializeField] private float maxY; // This is only for debug. Remove later
    private bool jumpDamping = false;

    [Header("Ground Check Cache")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private RaycastHit groundCheckHitInfo;

    [Header("Damping")]
    [SerializeField] private CountdownTimer dampingTurnOffTimer = new(.5f);
    private float baseLinearDamping;

    [Header("Movement Getters")]
    private float Velocity => rb.linearVelocity.magnitude;
    private Vector3 FlatLinearVelocity => new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    private float FlatVelocity => FlatLinearVelocity.magnitude;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private Transform orientation;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text t1;
    [SerializeField] private TMP_Text t2;
    [SerializeField] private TMP_Text t3;

    private Vector3 movementInput;
    private Vector3 LocalizedInput
    {
        get
        {
            var vector = orientation.forward * movementInput.z + orientation.right * movementInput.x;
            vector.y = 0;
            vector.Normalize();
            return vector;
        }
    }

    private bool jumpPressed;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        rb =
            rb != null ?
                rb :
                GetComponentInParent<Actor>().GetComponentInChildren<Rigidbody>();

        capsuleCollider =
            capsuleCollider != null ?
                capsuleCollider :
                GetComponentInParent<Actor>().GetComponentInChildren<CapsuleCollider>();

        groundCheck =
            groundCheck != null ?
                groundCheck :
                GetComponentInParent<Actor>().GetComponentInChildren<GroundCheck>();

        orientation =
            orientation != null ?
                orientation :
                GetComponentInParent<Actor>().GetComponentInChildren<CameraController>().transform; // TODO: Refactor to CharacterView

        baseLinearDamping = rb.linearDamping;

        UpdateUI();
    }

    void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"Jump height = {maxY.ToString("f2")}");
            maxY -= 0.1f;
        } */

        movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

    }

    void FixedUpdate()
    {
        // DEBUG
        maxY = Mathf.Max(maxY, transform.position.y);

        // GroundCheck fetch
        isGrounded = groundCheck.isGrounded(out groundCheckHitInfo);

        // Handle friction
        dampingTurnOffTimer.Tick(Time.deltaTime);
        if (movementInput.magnitude != 0f || !isGrounded || jumpDamping || !dampingTurnOffTimer.IsTicking())
        {
            rb.linearDamping = 0;
        }
        else
        {
            rb.linearDamping = baseLinearDamping;
        }

        // Apply forces
        rb.AddForce(
            LocalizedInput * accelerationMultiplier * Time.fixedDeltaTime,
            ForceMode.VelocityChange
        );
        HandleJump();

        // Clamp velocity
        var clampedFlatVector = Vector3.ClampMagnitude(FlatLinearVelocity, maxVelocity);
        rb.linearVelocity = new(clampedFlatVector.x, rb.linearVelocity.y, clampedFlatVector.z);

        UpdateUI();
    }

    private void HandleJump()
    {
        jumpCooldown.Tick(Time.deltaTime);
        if (jumpPressed)
        {
            jumpPressed = false;

            if (isGrounded && jumpCooldown.IsTicking())
            {
                // Jump
                jumpDamping = true;
                rb.linearDamping = 0;

                float gravityTickCompensation = -(Physics.gravity.y * Time.deltaTime) / 2;

                float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight) + gravityTickCompensation;

                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    jumpVelocity,
                    rb.linearVelocity.z
                );

                /* rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForceMultiplier, ForceMode.VelocityChange); */

                jumpCooldown.Reset();
            }
        }
        if (jumpCooldown.IsTicking())
        {
            jumpDamping = false;
        }
    }

    private void UpdateUI()
    {
        if (t1)
        {
            t1.text = $"FlatVelocity = {FlatVelocity.ToString("f2")}";
        }
        if (t2)
        {
            t2.text = $"Velocity = {Velocity.ToString("f2")}";
        }
        if (t3)
        {
            t3.text = $"Velocity = {rb.linearVelocity.magnitude.ToString("f2")}";
        }
    }

    void OnDrawGizmos()
    {
        /* Gizmos.color = Color.limeGreen;
        Gizmos.DrawWireCube(transform.position, new(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2)); */

        Gizmos.color = Color.white;
        GizmosUtil.DrawWireCapsule(capsuleCollider);
    }
}