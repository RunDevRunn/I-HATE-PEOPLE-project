using UnityEngine;
using UnityEngine.InputSystem;

/* 코드 흐름
1. 매 프레임 Update() → RotateFirePointToMouse() 실행
   → firePoint가 계속 마우스를 따라 회전 중 (총구가 계속 조준선을 따라감)

2. 마우스 클릭! → OnFire(InputValue value) 호출됨
   → value.isPressed == true

3. Fire() 실행
   → Instantiate(prefab, firePoint.position, firePoint.rotation)

4. 생성된 투사체에 firePoint.right 방향으로 속도 부여
   → 총구가 바라보던 방향 그대로 날아감
*/


public class PlayerAttack_v2 : MonoBehaviour
{
    [Header("발사체 설정")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }  

    // 매 프레임 마우스 방향으로 총구를 회전시킨다
    void Update()
    {
        RotateFirePointToMouse();
    }

    // 마우스 입력이 감지되면 발사한다
    void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            Fire();
        }
    }
    
    // 알고리즘 1. 마우스 방향으로 조준
    void RotateFirePointToMouse()
    {
        if (firePoint == null || mainCam == null)
            return;

        // 마우스 화면 좌표 -> 게임 월드 좌표 변환
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        Vector2 dir = mouseWorldPos - (Vector2)firePoint.position;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 알고리즘 2. 마우스 방향으로 발사
    // 연사를 막고 싶다면 if (Time.time < nextFireTime) return; 같은 쿨다운 체크 조건문 추가
    void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = firePoint.right * projectileSpeed;
        }
    }

}
