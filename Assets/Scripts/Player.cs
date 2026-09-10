using UnityEngine;
using UnityEngine.WSA;

public class Player : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform launchPoint;
    public float launchForce = 500f;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Launch();
        }
    }

    void Launch()
    {
        // 투사체 생성
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Bullet projectileLogic = projectile.GetComponent<Bullet>();
        
        // 마우스 클릭 위치를 게임 월드 내 좌표로
        // mousePosition은 유니티가 자체적으로 제공하는 변수. 그래서 선언 안해도 됨
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 2D 게임에서 ScreenToWorldPoint를 사용할 때는 z를 반드시 0으로 맞춘다
        mousePos.z = 0f;

        // 방향 게산 (launchPoint --> 마우스 클릭 위치)
        Vector2 dir = (mousePos - launchPoint.position).normalized;

        // Impulse로 초기 속도 부여 --> 포물선
        projectileLogic.FireBullet(dir, launchForce);
        //rb.AddForce(dir * launchForce, ForceMode2D.Impulse);
    }
}
