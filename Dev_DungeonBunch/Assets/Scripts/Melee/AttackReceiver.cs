using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackReceiver : MonoBehaviour
{
    [Header("Overrides")]
    [SerializeField] private DamageMultiplierSO damageIntakeMultipliers;
    private Dictionary<string, float> DamageOverrides => damageIntakeMultipliers.DamageOverrides;

    [Header("References")]
    [SerializeField] Rigidbody rb;
    /* [SerializeField] HealthSystem healthSystem; */

    void OnValidate()
    {
        TryGetComponent(out rb);
        /* TryGetComponent(out healthSystem); */
    }

    public void TakeAttack(Attack attack)
    {
        OverrideDamage(attack);
        /* ApplyAttack(attack); */

        /* OverrideKnockback(attack); */
        ApplyKnockback(attack);

    }

    public uint OverrideDamage(Attack attack)
    {
        float damage = attack.damage;
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

        return Math.Max((uint)damage, 0);
    }

    private void OverrideKnockback(Attack attack)
    {
        throw new NotImplementedException();
    }

    private void ApplyKnockback(Attack attack)
    {
        // Knockback
        if (rb)
        {
            rb.AddForce(attack.knockback, ForceMode.VelocityChange);
        }
    }

}
