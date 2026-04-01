using UnityEngine;

public class PencilCap : MonoBehaviour
{
    public MechanicalPencil rootPencil;
    
    [Header("判定设置")]
    public float pressThreshold = -0.5f; // 玩家向下的速度阈值
    private float lastPressTime = 0f;
    public float cooldown = 0.3f; // 两次按压之间的冷却时间

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandlePress(collision);
        }
    }

    // 使用 Stay 判定，防止玩家在上面跳跃但没离开触发区时无法再次触发
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandlePress(collision);
        }
    }

    private void HandlePress(Collider2D collision)
    {
        if (Time.time - lastPressTime < cooldown) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 核心逻辑：只有当玩家垂直速度向下（在掉落或跳跃下落阶段）
            // 且玩家的脚部大概在笔帽上方时触发
            if (rb.velocity.y < pressThreshold)
            {
                rootPencil.OnCapPressed();
                lastPressTime = Time.time;
                
                // 可选：给玩家一个微小的反弹力，让手感更好
                rb.velocity = new Vector2(rb.velocity.x, 3f); 
                
                Debug.Log("成功按动笔帽！");
            }
        }
    }
}