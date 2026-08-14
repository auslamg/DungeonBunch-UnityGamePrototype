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
    [SerializeField] private float maxVelocity = 7.5f;
    private float CurrentMaxVelocity => !isCrouching ?
        maxVelocity : isGrounded ?
            maxVelocity * crouchSpeedMultiplier :
            maxVelocity;
    [SerializeField] private float accelerationMultiplier = 1f;
    private float CurrentAcceleration => !isCrouching ?
        accelerationMultiplier : isGrounded ?
            accelerationMultiplier * crouchSpeedMultiplier :
            accelerationMultiplier;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpForceMultiplier = 1f;
    [SerializeField] private bool jumpDamping = false;
    [SerializeField] private CountdownTimer jumpCooldown = new(.25f);
    [SerializeField] private CountdownTimer coyoteTime = new(.25f);
    [SerializeField] private CountdownTimer jumpBuffer = new(.15f);

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private RaycastHit groundCheckHitInfo;
    [SerializeField] private bool isOnSlope = false;
    [SerializeField] private bool isOnSteepSlope = false;

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
    private bool isCrouching = false;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text t1;
    [SerializeField] private TMP_Text t2;
    [SerializeField] private TMP_Text t3;

    [Header("Input")]
    [SerializeField] private Vector3 movementInput;
    private Vector3 MoveInput
    {
        get
        {
            float yaw = view.eulerAngles.y;

            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

            return (
                forward * movementInput.z +
                right * movementInput.x
            ).normalized;
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
                jumpBuffer.Reset();
            }

            crouchPressed = Input.GetButton("Crouch");
        }
    }

    void FixedUpdate()
    {
        // GroundCheck fetch
        isGrounded = groundCheck.Check(out groundCheckHitInfo);
        isCrouching = pose.IsCrouching;
        isOnSlope = groundCheck.isOnSlope;
        isOnSteepSlope = groundCheck.isOnSteepSlope;

        // TODO: Refactor movement input override

        Vector3 slopeProjectedMoveInput = new();
        // TODO: HandleSlope
        // Handle slope
        {
            if (isOnSteepSlope)
            {
                isGrounded = false;
            }

            if (isGrounded && !isOnSteepSlope)
            {
                rb.useGravity = false;
            }
            else
            {
                rb.useGravity = true;
            }

            if (isOnSlope && !isOnSteepSlope)
            {
                slopeProjectedMoveInput =
                    Vector3.ProjectOnPlane(MoveInput, groundCheckHitInfo.normal);

                if ((MoveInput == Vector3.zero || Vector3.Angle(Vector3.up, slopeProjectedMoveInput) <= 90) &&
                    rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector3(
                        rb.linearVelocity.x,
                        0,
                        rb.linearVelocity.z);
                }
            }
        }

        // Handle friction
        {
            dampingTurnOffTimer.Tick(Time.deltaTime);
            if (MoveInput.magnitude != 0 || !isGrounded || jumpDamping || !dampingTurnOffTimer.IsTicking() || isOnSteepSlope)
            {
                rb.linearDamping = 0;
            }
            else
            {
                rb.linearDamping = baseLinearDamping;
            }
        }

        // TODO: HandleSteps

        HandleJump();

        var finalforce = isOnSlope && !isOnSteepSlope ?
            CurrentAcceleration * Time.fixedDeltaTime * slopeProjectedMoveInput :
            CurrentAcceleration * Time.fixedDeltaTime * MoveInput;

        // Apply forces
        rb.AddForce(
            finalforce,
            ForceMode.VelocityChange
        );

        // Clamp velocity
        if (MoveInput != Vector3.zero)
        {
            var clampedFlatVector = Vector3.ClampMagnitude(FlatLinearVelocity, CurrentMaxVelocity);
            rb.linearVelocity = new Vector3(
                clampedFlatVector.x,
                rb.linearVelocity.y,
                clampedFlatVector.z);
        }

        UpdateUI();
    }

    private void HandleJump()
    {
        // FIX: Well-timed double jumped is possible due to coyotetime and cooldown overlap
        if (!isGrounded)
        {
            coyoteTime.Tick(Time.deltaTime);
        }
        else
        {
            coyoteTime.Reset();
        }
        jumpCooldown.Tick(Time.deltaTime);

        if (jumpPressed || !jumpBuffer.Tick(Time.deltaTime))
        {
            jumpPressed = false;

            if ((isGrounded || !coyoteTime.IsTicking()) && jumpCooldown.IsTicking())
            {
                // Jump
                jumpDamping = true;
                rb.linearDamping = 0;
                jumpBuffer.Set(0);

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