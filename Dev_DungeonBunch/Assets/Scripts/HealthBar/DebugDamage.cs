using System;
using UnityEngine;
/// <summary>
/// Debug para testear el daño y como afecta a la barra de vida
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class DebugDamage : MonoBehaviour
{    
    [Header("Debug")]
    [SerializeField] private int debugDamage = 15;
    [SerializeField] private HealthSystem healthSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    // Update is called once per frame
    void Update() 
    {
        if(healthSystem != null && Input.GetKeyDown(KeyCode.B))
        {
            healthSystem.GetDamage(debugDamage);
            Debug.Log(debugDamage);
        }  
    }
}
