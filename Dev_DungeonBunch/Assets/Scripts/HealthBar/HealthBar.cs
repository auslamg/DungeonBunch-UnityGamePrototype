using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el evento, cuando se suscribe y se desuscribe y que hace cuando esta suscrito
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Componentes HealthBar")]
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    [Header("Health System")]
    [SerializeField] private HealthSystem HealthSystem;
    void OnEnable()
    {
        if (HealthSystem != null) // Suscribimos el evento
        {
            HealthSystem.OnChangeHealth += HealthSystem_OnHealthChanged;
        }
    }
    void OnDisable()
    {
        if(HealthSystem != null) //Desuscribimos el evento
        {
            HealthSystem.OnChangeHealth -= HealthSystem_OnHealthChanged;
        }  
    }

    void HealthSystem_OnHealthChanged(int currentHealth, int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
