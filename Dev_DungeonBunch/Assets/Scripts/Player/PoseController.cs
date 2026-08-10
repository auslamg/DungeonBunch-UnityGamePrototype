using Unity.VisualScripting;
using UnityEngine;

public class PoseController : MonoBehaviour
{
    [Header("Crouch")]
    [SerializeField] private bool isCrouching = false;
    public bool IsCrouching => isCrouching;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float baseColliderHeight;
    [SerializeField] private float crouchHeightMultiplier = 0.75f;
    private float CrouchColliderHeight => baseColliderHeight * crouchHeightMultiplier;
    Vector3 ViewPosition => capsuleCollider.transform.TransformPoint(capsuleCollider.center) + new Vector3(0, capsuleCollider.bounds.extents.y - capsuleCollider.radius, 0);

    [Header("Top Check Cache")]
    [SerializeField] private bool isTopped = false;
    [SerializeField] private RaycastHit topCheckHitInfo;

    // INPUT
    [SerializeField] private bool crouchPressed;

    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private TopCheck topCheck;
    [SerializeField] private Transform view;
    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        capsuleCollider =
            capsuleCollider != null ?
                capsuleCollider :
                GetComponentInParent<Actor>().GetComponentInChildren<CapsuleCollider>();

        topCheck =
            topCheck != null ?
                topCheck :
                GetComponentInParent<Actor>().GetComponentInChildren<TopCheck>();

        view =
            view != null ?
                view :
                GetComponentInParent<Actor>().GetComponentInChildren<CameraController>().transform; // TODO: Refactor to CharacterView

        baseColliderHeight = capsuleCollider.height;
    }

    enum PoseState { Stand, Crouch, Prone, Ragdoll };

    void Update()
    {
        // INPUT
        {
            crouchPressed = Input.GetButton("Crouch");
        }
    }
    

    private void FixedUpdate()
    {
        // GroundCheck fetch
        isTopped = topCheck.Check(out topCheckHitInfo);
        HandleCrouch();
    }

    private void HandleCrouch()
    {
        if (crouchPressed)
        {
            capsuleCollider.height = CrouchColliderHeight;
        }
        else if (!isTopped)
        {
            capsuleCollider.height = baseColliderHeight;
        }
        isCrouching = crouchPressed;
        view.transform.position = ViewPosition;
    }
}
