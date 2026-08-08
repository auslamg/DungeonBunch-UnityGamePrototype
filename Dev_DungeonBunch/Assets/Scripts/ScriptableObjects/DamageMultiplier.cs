using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageMultiplierSO", menuName = "Scriptable Objects/DamageMultiplier")]
public class DamageMultiplierSO : ScriptableObject
{
    [Header("Values")]
    [SerializeField] private List<DamageMultiplierEntry> damageTypeMultipliers = new();
    public Dictionary<string, float> DamageOverrides { get; private set; }

    void OnValidate()
    {
        DamageOverrides = new();
        foreach (var entry in damageTypeMultipliers)
        {
            DamageOverrides.Add(entry.damageType.Key, entry.multiplier);
        }
    }

    [Serializable]
    public struct DamageMultiplierEntry
    {
        [SerializeField] public DamageTypeSO damageType;
        [SerializeField] public float multiplier;
    }
}

