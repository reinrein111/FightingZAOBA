using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float rotateSpeed = 720f;
    public float lifeTime = 5f;

    private Vector2 moveDirection;

    public void Launch(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

   private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        PlayerShield shield = collision.GetComponent<PlayerShield>();

        // 核心逻辑：先尝试用护盾抵扣
        if (shield != null && shield.TryUseShield())
        {
            // 抵扣成功：子弹消失，但不调用死亡
            Destroy(gameObject);
            return; 
        }

        // 抵扣失败或没护盾：执行原有的死亡
        SpikeTrigger st = Object.FindAnyObjectByType<SpikeTrigger>();
        if (st != null) st.ExecuteDeath();
        
        Destroy(gameObject);
    }
}
}