using UnityEngine;

public class DamageTestTrigger : MonoBehaviour
{
    [SerializeField]
    private Health health;

    void Update()
    {
        // 스페이스바를 누르면 10 데미지 테스트
        if (Input.GetKeyDown(KeyCode.Space))
        {
            health.TakeDamage(10f);
        }
    }
}
