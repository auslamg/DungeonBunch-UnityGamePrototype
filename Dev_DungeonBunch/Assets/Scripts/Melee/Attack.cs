using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Attack
{
    public Attack(GameObject source, uint damage, Vector3 direction, float knockback, HashSet<DamageTypeSO> damageTypes)
    {
        this.source = source;
        this.damage = damage;
        this.direction = direction;
        this.knockback = direction.normalized * knockback;
        this.damageTypes = damageTypes;
    }

    public Attack(GameObject source, uint damage, Vector3 direction, Vector3 knockback, HashSet<DamageTypeSO> damageTypes)
    {
        this.source = source;
        this.damage = damage;
        this.direction = direction;
        this.knockback = knockback;
        this.damageTypes = damageTypes;
    }

    [SerializeField] public GameObject source;
    [SerializeField] public uint damage;
    [SerializeField] public Vector3 direction;
    [SerializeField] public Vector3 knockback;
    [SerializeField] public HashSet<DamageTypeSO> damageTypes;
}
