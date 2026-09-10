using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [Header("이펙트 설정")]
    public GameObject explosionEffect;

    [SerializeField]
    private float damage = 3f;
    private Rigidbody2D rb;

    // 자기 자신이 어느 풀 소속인지 알아야 스스로 반납 요청 가능
    private IObjectPool<Projectile> myPool;

    // 같은 프레임에 충돌 + 화면이탈이 겹쳐서 Release가 두 번 호출되는 문제 방지용
    private bool isReleased;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<Projectile> pool)
    {
        myPool = pool;
    }

    // public 붙이는 이유: PlayerAttack.cs에서 접근 가능하게 하려고
    public void SetupProjectile(Vector2 shootDirection, float launchForce)
    {
        // 풀에서 재사용되는 오브젝트이므로 이전 발사의 속도, 회전이 남아있으면 안 되기 때문에 초기화 해준다
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 재사용 시점에 Release 상태 초기화. 반납 플래그 초기화
        isReleased = false;

        if (rb != null)
        {
            rb.AddForce(shootDirection * launchForce, ForceMode2D.Impulse);
        }
    }

private void OnCollisionEnter2D(Collision2D collision)
    {     
        Health targetHealth = collision.gameObject.GetComponent<Health>();

        // 테스트용: Health를 찾았는지 확인
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        // 충돌 시 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 충돌 시 포탄을 풀로 반납
        ReleaseToPool();
    }

    void OnBecameInvisible()
    {
        ReleaseToPool();
    }

    private void ReleaseToPool()
    {
        // 이미 반납된 상태라면 중복 호출 방지
        if (isReleased) return;

        isReleased = true;
        // 풀에 반납. null 조건부 연산자 사용. myPool이 null이면 Release 호출 안 함
        myPool?.Release(this);

    }

}
