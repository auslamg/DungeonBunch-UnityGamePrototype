using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    [SerializeField] private Transform view;
    [SerializeField] private PoseController pose;    

    [Header("Crouch")]
    private bool IsCrouching => pose.IsCrouching;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text t1;
    [SerializeField] private TMP_Text t2;
    [SerializeField] private TMP_Text t3;

    [Header("Input")]
    [SerializeField] private Vector3 movementInput;
    private Vector3 LocalizedInput
    {
        get
        {
            var vector = view.forward * movementInput.z + view.right * movementInput.x;
            vector.y = 0;
            vector.Normalize();
            return vector;
        }
    }

    [SerializeField] private bool jumpPressed;

    [SerializeField] private bool crouchPressed;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

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

        view =
            view != null ?
                view :
                GetComponentInParent<Actor>().GetComponentInChildren<ViewController>().transform;

        pose =
            pose != null ?
                pose :
                GetComponentInParent<Actor>().GetComponentInChildren<PoseController>();

        baseLinearDamping = rb.linearDamping;

        UpdateUI();
    }

    void Update()
    {
        // INPUT
        {
            movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

            if (Input.GetButtonDown("Jump"))
            {
                jumpPressed = true;
            }

            crouchPressed = Input.GetButton("Crouch");
        }
    }

    void FixedUpdate()
    {
        // DEBUG
        maxY = Mathf.Max(maxY, transform.position.y);

        // GroundCheck fetch
        isGrounded = groundCheck.IsGrounded(out groundCheckHitInfo);

        // Handle friction
        {
            dampingTurnOffTimer.Tick(Time.deltaTime);
            if (movementInput.magnitude != 0f || !isGrounded || jumpDamping || !dampingTurnOffTimer.IsTicking())
            {
                rb.linearDamping = 0;
            }
            else
            {
                rb.linearDamping = baseLinearDamping;
            }
        }

        // TODO: HandleSlope
        // TODO: HandleSteps

        HandleJump();

        var finalforce =
            IsCrouching ?
                accelerationMultiplier * crouchSpeedMultiplier * Time.fixedDeltaTime * LocalizedInput :
                accelerationMultiplier * Time.fixedDeltaTime * LocalizedInput;

        // Apply forces
        rb.AddForce(
            finalforce,
            ForceMode.VelocityChange
        );

        // Clamp velocity
        {
            var clampedFlatVector =
            IsCrouching ?
                Vector3.ClampMagnitude(FlatLinearVelocity, maxVelocity * crouchSpeedMultiplier) :
                Vector3.ClampMagnitude(FlatLinearVelocity, maxVelocity);

            rb.linearVelocity = new(clampedFlatVector.x, rb.linearVelocity.y, clampedFlatVector.z);
        }

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