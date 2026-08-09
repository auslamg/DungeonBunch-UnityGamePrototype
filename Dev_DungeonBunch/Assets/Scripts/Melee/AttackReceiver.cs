using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Component for an <see cref="Actor"/> object that is meant to receive damage. Checks if the actor has a melee block and is currently blocking to negate damage.
/// </summary>
public class AttackReceiver : MonoBehaviour
{
    [Header("Overrides")]
    [SerializeField] private DamageMultiplierSO damageIntakeMultipliers;
    private Dictionary<string, float> DamageOverrides => damageIntakeMultipliers.DamageOverrides;

    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ActionManager actionManager;
    [SerializeField] MeleeBlock meleeBlock;
    [SerializeField] List<IStaggerable> staggerables = new();
    /* [SerializeField] HealthSystem healthSystem; */

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        rb =
            rb != null ?
                rb :
                GetComponentInParent<Actor>().GetComponentInChildren<Rigidbody>();

        if (GetComponentInParent<Actor>() != null && (actionManager = GetComponentInParent<Actor>().GetComponentInChildren<ActionManager>()) != null)
        {
            actionManager.gameObject.GetComponents(staggerables);
        }

        if (GetComponentInParent<Actor>() != null)
        {
            meleeBlock = GetComponentInParent<Actor>().GetComponentInChildren<MeleeBlock>();
        }

        /* TryGetComponent(out healthSystem); */
    }

    /// <summary>
    /// Processes an incoming attack, nullifying it if there is a <see cref="MeleeBlock"/> component presently blocking, or overriding data through <see cref="DamageOverrides"/> entries and applying the effects.
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    public uint TakeAttack(Attack attack)
    {
        if (!CheckBlock(ref attack))
        {
            OverrideDamage(attack);
            /* OverrideKnockback(attack); */
            /* OverrideStagger(attack); */

            /* ApplyAttack(attack); */
            ApplyKnockback(attack);
            ApplyStagger(attack);

            return attack.damage;
        }
        else Debug.Log($"[Attack Receiver]: Block succesful");
        return 0;
    }

    private bool CheckBlock(ref Attack attack)
    {
        if (meleeBlock)
        {
            Debug.Log($"[Attack Receiver]: Found MeleeBlock");
            if (meleeBlock.TryBlock(attack))
            {
                attack.damage = 0;
                attack.knockback = Vector3.zero;
                return true;
            }
        }
        return false;
    }

    public uint OverrideDamage(Attack attack)
    {
        float damage = attack.damage;
        if (damageIntakeMultipliers != null)
        {
            foreach (var damageType in attack.damageTypes)
            {
                if (DamageOverrides.ContainsKey(damageType.Key))
                {
                    damage *= DamageOverrides[damageType.Key];
                }
                if (damage <= 0)
                {
                    break;
                }
            }
        }

        return Math.Max((uint)damage, 0);
    }

    private Vector3 OverrideKnockback(Attack attack)
    {
        return attack.knockback;
    }

    private void ApplyKnockback(Attack attack)
    {
        // Knockback
        if (rb)
        {
            rb.AddForce(attack.knockback, ForceMode.VelocityChange);
        }
    }

    private void OverrideStagger(Attack attack)
    {
        throw new NotImplementedException();
    }

    private void ApplyStagger(Attack attack)
    {
        foreach (var action in staggerables)
        {
            Debug.Log($"[AttackReceiver] Staggerable: {gameObject}+{action}");
            action.Stagger();
        }
    }

}
