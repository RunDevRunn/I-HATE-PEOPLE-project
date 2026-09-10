using UnityEngine;
using System;
// 플레이어든 적이든, "체력을 가진 모든 것"이 공통으로 쓰는 클래스
public class Health : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;

    private float currentHealth;

    // 체력 변경 신호. 플레이어 것이든 적 것이든 구조는 동일함
    public event Action<float, float> OnHealthChanged;

    // 사망 신호
    public event Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

}
