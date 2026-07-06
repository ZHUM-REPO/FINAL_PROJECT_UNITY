using System;
using UnityEngine;

public class Damage : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth = 100f; // visible in the inspector; drains live at runtime

    public event Action<float> HealthChanged; // passes current health
    public event Action Died;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool Invulnerable { get; set; }

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f) Died?.Invoke();
    }
}