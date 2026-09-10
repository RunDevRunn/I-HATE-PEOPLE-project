using UnityEngine;
using UnityEngine.UI;
public class HealthBarUI : MonoBehaviour
{
    [SerializeField]
    private Health health;  // PlayerHealth → Health 로 타입만 변경

    [SerializeField]
    private Slider immediateSlider;

    [SerializeField]
    private Slider delayedSlider;

    [SerializeField]
    private float delayedSmoothSpeed = 2f;

    private float targetRatio;

    void OnEnable()
    {
        health.OnHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        health.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;

        if (immediateSlider != null)
        {
            immediateSlider.value = ratio;
        }

        targetRatio = ratio;
    }

    void Update()
    {
        if (delayedSlider == null) return;

        delayedSlider.value = Mathf.Lerp(
            delayedSlider.value,
            targetRatio,
            1f - Mathf.Exp(-delayedSmoothSpeed * Time.deltaTime)
        );
    }
}
