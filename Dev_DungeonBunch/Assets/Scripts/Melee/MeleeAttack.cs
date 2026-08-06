using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

enum AttackState
{
    Ready,
    Windup,
    Release,
    Cooldown
}

public class MeleeAttack : MonoBehaviour, IRunnable, IStaggerable, IExclusiveInstanceAction
{
    [Header("Input")]
    [SerializeField] private KeyCode debugKey;
    bool DebugAttackInput => Input.GetKeyDown(debugKey);

    [Header("Action interface")]
    private ActionManager actionManager;
    public ActionManager ActionManager
    {
        get => actionManager;
        set => actionManager = value;
    }
    ActionState IExclusiveAction.State => GetActionState();
    public event EventHandler OnActionStart;
    public event EventHandler OnActionEnd;

    [Header("State")]
    [SerializeField] AttackState state = AttackState.Ready; 
    [SerializeField] HashSet<GameObject> hitTargets = new();
    [SerializeField] private CountdownTimer attackCooldown = new(1);
    [SerializeField] private CountdownTimer windupDuration = new(0.2f);
    [SerializeField] private CountdownTimer releaseDuration = new(0.1f);
    [SerializeField] private CountdownTimer staggerTime = new(0.25f);
    [SerializeField] bool isStaggered = false;

    [Header("Attack values")]
    [SerializeField] uint damage;
    [SerializeField] float knockback;
    [SerializeField] List<DamageTypeSO> damageTypes;
    Attack Attack => new
    (
        owner: actor,
        source: orientation,
        damage: this.damage,
        direction: orientation.transform.forward,
        knockback: knockback,
        damageTypes: damageTypes.ToHashSet()
    );

    [Header("Physics queries")]
    [SerializeField] LayerMask layerMask;
    [SerializeField] Vector3 hitBoxHalfExtents = new Vector3(1.5f, 0.5f, 1.5f);
    float hitBoxOffset => hitBoxHalfExtents.z;
    Vector3 HitBoxCenter => orientation.position + orientation.forward * hitBoxOffset;


    [Header("References")]
    [SerializeField] private GameObject actor;
    [SerializeField] private Transform orientation;

    [Header("Debug UI")]
    [SerializeField] Slider cooldownSlider;
    [SerializeField] private TMP_Text t1;
    [SerializeField] private Sprite actionCrosshair;
    public Sprite ActionCrosshair => actionCrosshair;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (GetComponentInParent<Actor>())
        {
            actor =
            actor != null ?
                actor :
                GetComponentInParent<Actor>().gameObject;

            orientation =
                orientation != null ?
                    orientation :
                    GetComponentInParent<Actor>().GetComponentInChildren<CameraController>().transform; // TODO: Refactor to CharacterView
        }

        windupDuration.Reset();
        releaseDuration.Reset();
        attackCooldown.Reset();

        UpdateUI();
    }

    public bool TryExecuteAction()
    {
        // Input
        if (state == AttackState.Ready &&
            !isStaggered)
        {
            // Start attack
            hitTargets.Clear();
            state = AttackState.Windup;
            OnActionStart?.Invoke(this, EventArgs.Empty);

            return true;
        }
        else return false;
    }

    public void RunUpdate()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        switch (state)
        {
            case AttackState.Ready:
                // Input
                if (DebugAttackInput &&
                    !isStaggered)
                {
                    TryExecuteAction();
                }
                break;

            case AttackState.Windup:
                // If the windup duration is over, end windup and start release phase
                if (windupDuration.Tick(deltaTime))
                {
                    state = AttackState.Release;
                    windupDuration.Reset();
                }
                break;

            case AttackState.Release:
                // Actively hit targets
                HitReg();
                // If the release duration is over, end attack and start cooldown
                if (releaseDuration.Tick(deltaTime))
                {
                    state = AttackState.Cooldown;
                    releaseDuration.Reset();
                    OnActionEnd?.Invoke(this, EventArgs.Empty);
                }
                break;

            case AttackState.Cooldown:
                // If the attack cooldown is over, set ready
                if (attackCooldown.Tick(deltaTime))
                {
                    state = AttackState.Ready;
                    attackCooldown.Reset();
                }
                break;
        }

        if (isStaggered)
        {
            // If the stagger duration is over, end stagger
            if (staggerTime.Tick(deltaTime))
            {
                isStaggered = false;
            }
        }

        UpdateUI();
    }

    private void HitReg()
    {
        var colliders = Physics.OverlapBox(HitBoxCenter, hitBoxHalfExtents, orientation.rotation, layerMask);
        var thisActor = GetComponentInParent<Actor>().gameObject;
        foreach (var other in colliders)
        {
            if (!other.transform.IsChildOf(thisActor.transform) &&
                hitTargets.Add(other.gameObject))
            {
                /* Debug.Log($"[MeleeAttack] HitReg: {other.name} +  -  + {other.transform.position}"); */

                // General
                if (other.TryGetComponent(out AttackReceiver receiver))
                {
                    receiver.TakeAttack(Attack);
                }
            }
        }
    }

    public void Stagger()
    {
        isStaggered = true;
        staggerTime.Reset();
    }

    public ActionState GetActionState()
    {
        return state switch
        {
            AttackState.Ready => ActionState.Ready,
            AttackState.Windup => ActionState.InProgress,
            AttackState.Release => ActionState.Busy,
            AttackState.Cooldown => ActionState.Cooldown,
            _ => ActionState.Ready,
        };
    }

    public void Interrupt()
    {
        windupDuration.Reset();
        releaseDuration.Reset();
        attackCooldown.Reset();
        state = AttackState.Cooldown;
        OnActionEnd?.Invoke(this,EventArgs.Empty);
    }

    private void UpdateUI()
    {
        if (cooldownSlider)
        {
            cooldownSlider.value = state == AttackState.Cooldown ? attackCooldown.ProgressPercent : releaseDuration.RemainingPercent;
            cooldownSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = AttackStateColor();
        }

        if (t1)
        {
            t1.text = $"{state}";
            t1.color = AttackStateColor();
        }
    }

    Color AttackStateColor()
    {
        return state switch
        {
            AttackState.Ready => Color.gold,
            AttackState.Windup => Color.darkOrange,
            AttackState.Release => Color.red,
            AttackState.Cooldown => Color.lemonChiffon,
            _ => Color.white,
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = AttackStateColor();

        // HitBox gizmo

        GizmosUtil.WithGizmoMatrix(
            Matrix4x4.TRS(
                HitBoxCenter,
                orientation.rotation,
                Vector3.one),
                () =>
        {
            Gizmos.DrawWireCube(Vector3.zero, hitBoxHalfExtents * 2);
        });
    }
}