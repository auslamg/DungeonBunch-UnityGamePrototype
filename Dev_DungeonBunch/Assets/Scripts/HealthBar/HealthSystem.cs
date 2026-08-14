using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Se crea el evento y los elementos a los que afecta
/// </summary>
public class HealthSystem : MonoBehaviour
{

    [Header("Valores del jugador")]
    private int currentHealth;
    private int maxHealth = 100;
    public event Action<int, int> OnChangeHealth;

    // El jugador inicia su vida al maximo de la vida
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void GetDamage(int damageAmount)
    {
        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnChangeHealth?.Invoke(currentHealth, maxHealth);
        }

    }

    private void Die()
    {
        Debug.Log("El enemigo ha muerto");
    }
}
