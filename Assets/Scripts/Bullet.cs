using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void FireBullet(Vector2 direction, float force)
    {
        Vector2 dir = direction.normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

}
