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
            Debug.Log("El enemigo tiene " + currentHealth + " de vida");
            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnChangeHealth?.Invoke(currentHealth, maxHealth);
        }

    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnChangeHealth?.Invoke(currentHealth, maxHealth);
        Debug.Log("El jugador se ha curado " + healAmount + " y tiene " + currentHealth + " de vida");
    }

    public void Die()
    {
        Debug.Log("El enemigo ha muerto");
    }
}
