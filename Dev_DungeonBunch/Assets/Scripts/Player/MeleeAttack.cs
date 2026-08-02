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
    Hurt,
    Cooldown
}

public class MeleeAttack : MonoBehaviour, IRunnable
{
    [Header("Input")]
    [SerializeField] public bool attackInput = false;
    [SerializeField] bool debugAttackInput = false;

    [Header("State")]
    [SerializeField] AttackState state = AttackState.Ready;
    [SerializeField] bool isStaggered = false;
    [SerializeField] HashSet<GameObject> hitTargets = new();
    [SerializeField] private CountdownTimer attackCooldown = new(1);
    [SerializeField] private CountdownTimer windupDuration = new(0.2f);
    [SerializeField] private CountdownTimer hurtDuration = new(0.4f);
    [SerializeField] private CountdownTimer staggerTime = new(0.25f);

    [Header("References")]
    [SerializeField] private BoxCollider hurtBox;

    [Header("Debug UI")]
    [SerializeField] private KeyCode debugKey;

    [Header("Debug UI")]
    [SerializeField] Slider cooldownSlider;
    [SerializeField] private TMP_Text t1;
    [SerializeField] private TMP_Text t2;
    [SerializeField] private TMP_Text t3;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        TryGetComponent(out hurtBox);
        UpdateUI();
    }    

    public void Run()
    {
        debugAttackInput = Input.GetKeyDown(debugKey);

        if (state == AttackState.Windup)
        {
            // If the windup duration is over, end windup and start hurt phase
            if (windupDuration.Tick(Time.deltaTime))
            {
                state = AttackState.Hurt;
                windupDuration.Reset();
            }
        }
        if (state == AttackState.Hurt)
        {
            // If the attack duration is over, end attack and start cooldown
            if (hurtDuration.Tick(Time.deltaTime))
            {
                state = AttackState.Cooldown;
                hurtDuration.Reset();
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

    void StartAttack()
    {
        hitTargets.Clear();
        windupDuration.Reset();
        hurtDuration.Reset();
        state = AttackState.Windup;
    }

    private void OnTriggerStay(Collider other)
    {
        if (state == AttackState.Hurt)
        {
            if (other.gameObject.layer != gameObject.layer &&
                hitTargets.Add(other.gameObject))
            {
                Debug.Log(other.name + " - " + other.transform.position);

                //TODO: Implement
                // Damage 
                /* if (other.TryGetComponent(out HealthSystem hs))
                {
                    hs.Damage(40); //TODO: Parametrize
                } */

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
            }
        }
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
        if (t2)
        {
        }
        if (t3)
        {
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
            case AttackState.Hurt:
                return Color.red;
            case AttackState.Cooldown:
                return new Color(1,1,.6f);
            default:
                return Color.white;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = AttackStateColor();

        GizmosUtil.WithGizmoMatrix(transform.localToWorldMatrix, () =>
        {
            Gizmos.DrawWireCube(hurtBox.center, hurtBox.size);
        });
    }
}
