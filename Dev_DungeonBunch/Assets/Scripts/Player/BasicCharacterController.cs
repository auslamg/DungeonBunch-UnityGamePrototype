using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class BasicCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float accelerationMultiplier = 1f;
    [SerializeField] private float jumpForceMultiplier = 1f;

    [Header("Ground Check Cache")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private RaycastHit groundCheckHit;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GroundCheck groundCheck;

    private Vector3 movementInput;
    private bool jumpPressed;

    void OnValidate()
    {
        TryGetComponent(out rb);
        TryGetComponent(out capsuleCollider);
        TryGetComponent(out groundCheck);
    }

    void Update()
    {
        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(
            movementInput * accelerationMultiplier * Time.fixedDeltaTime,
            ForceMode.VelocityChange
        );

        if (jumpPressed)
        {
            jumpPressed = false;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForceMultiplier, ForceMode.VelocityChange);
        }
    }

    

    void OnDrawGizmos()
    {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, new(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2));     
    }
}