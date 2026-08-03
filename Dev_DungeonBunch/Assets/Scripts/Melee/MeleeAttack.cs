using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

enum AttackState
{
    Ready,
    Windup,
    Release,
    Cooldown
}

public class MeleeAttack : MonoBehaviour, IRunnable
{
    [Header("Input")]
    [SerializeField] public bool attackInput = false;
    bool debugAttackInput => Input.GetKeyDown(debugKey);

    [Header("State")]
    [SerializeField] AttackState state = AttackState.Ready;
    [SerializeField] HashSet<GameObject> hitTargets = new();
    [SerializeField] private CountdownTimer attackCooldown = new(1);
    [SerializeField] private CountdownTimer windupDuration = new(0.2f);
    [SerializeField] private CountdownTimer releaseDuration = new(0.4f);
    [SerializeField] private CountdownTimer staggerTime = new(0.25f);
    [SerializeField] bool isStaggered = false;

    [Header("Attack values")]
    [SerializeField] uint damage;
    [SerializeField] float knockback;
    [SerializeField] List<DamageTypeSO> damageTypes;
    Attack Attack => new
    (
        source: gameObject,
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
    [SerializeField] private Transform orientation;

    [Header("Debug Keys")]
    [SerializeField] private KeyCode debugKey;

    [Header("Debug UI")]
    [SerializeField] Slider cooldownSlider;
    [SerializeField] private TMP_Text t1;
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite atkCrosshair;
    [SerializeField] private Sprite baseCrosshair;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        orientation =
            orientation != null ? orientation : GetComponentInChildren<CameraController>().gameObject.transform;
        baseCrosshair =
            crosshair != null ? crosshair.sprite : null;

        UpdateUI();
    }

    public void Run()
    {
        if (state == AttackState.Windup)
        {
            // If the windup duration is over, end windup and start release phase
            if (windupDuration.Tick(Time.deltaTime))
            {
                state = AttackState.Release;
                windupDuration.Reset();
            }
        }
        if (state == AttackState.Release)
        {
            HitReg();
            // If the release duration is over, end attack and start cooldown
            if (releaseDuration.Tick(Time.deltaTime))
            {
                state = AttackState.Cooldown;
                releaseDuration.Reset();
            }
        }
        if (state == AttackState.Cooldown)
        {
            // If the attack cooldown is over, set ready
            if (attackCooldown.Tick(Time.deltaTime))
            {
                state = AttackState.Ready;
            }
        }

        if (isStaggered)
        {
            // If the stagger duration is over, end stagger
            if (staggerTime.Tick(Time.deltaTime))
            {
                isStaggered = false;
            }
        }

        // Input
        if (state == AttackState.Ready && (attackInput || debugAttackInput))
        {
            StartAttack();
            attackCooldown.Reset();
        }

        UpdateUI();
    }

    private void HitReg()
    {
        var colliders = Physics.OverlapBox(HitBoxCenter, hitBoxHalfExtents, orientation.rotation, layerMask);
        foreach (var other in colliders)
        {
            if (!other.transform.IsChildOf(this.transform) &&
                hitTargets.Add(other.gameObject))
            {
                Debug.Log(other.name + " - " + other.transform.position);

                // General
                if (other.TryGetComponent(out AttackReceiver receiver))
                {
                    receiver.TakeAttack(Attack);
                }
                /* else
                {
                    //TODO: Implement
                    // Damage 
                    if (other.TryGetComponent(out HealthSystem hs))
                    {
                        hs.Damage(40); //TODO: Parametrize
                    }

                    // Knockback
                    if (other.TryGetComponent(out Rigidbody rb))
                    {
                        rb.AddForce((Vector3.up + 2 * transform.forward).normalized * 10f, ForceMode.VelocityChange); //TODO: Parametrize
                    }

                    // Stagger
                    if (other.TryGetComponent(out MeleeAttack melee))
                    {
                        melee.Stagger(0.5f); //TODO: Parametrize
                    }
                } */
            }
        }
    }

    void StartAttack()
    {
        hitTargets.Clear();
        windupDuration.Reset();
        releaseDuration.Reset();
        state = AttackState.Windup;
    }

    public void Stagger(float time)
    {
        isStaggered = true;
        staggerTime = new(time);
        staggerTime.Reset();
    }

    private void UpdateUI()
    {
        if (cooldownSlider)
        {
            cooldownSlider.value = attackCooldown.ProgressPercent;
        }

        if (t1)
        {
            t1.text = $"{state}";
            t1.color = AttackStateColor();
        }

        if (crosshair)
        {
            crosshair.sprite = state == AttackState.Release ? atkCrosshair : baseCrosshair;
        }
    }

    Color AttackStateColor()
    {
        switch (state)
        {
            case AttackState.Ready:
                return Color.yellowGreen;
            case AttackState.Windup:
                return Color.orange;
            case AttackState.Release:
                return Color.red;
            case AttackState.Cooldown:
                return new Color(1, 1, .6f);
            default:
                return Color.white;
        }
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
