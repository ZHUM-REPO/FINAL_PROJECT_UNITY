using System;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;

    public event Action<float> HealthChanged; // passes current health
    public event Action Died;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;
    public bool Invulnerable { get; set; }

    void Awake() => CurrentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0f) Died?.Invoke();
    }
}