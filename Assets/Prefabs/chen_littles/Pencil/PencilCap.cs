
using UnityEngine;

public class PencilCap : MonoBehaviour
{
    public MechanicalPencil rootPencil;
    
    [Header("判定设置")]
    public float pressThreshold = -0.1f;
    private float lastPressTime = 0f;
    public float cooldown = 0.3f;

    private Transform player1;
    private Transform player2;

    private void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsPlayer(collision.transform))
        {
            HandlePress(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsPlayer(collision.transform))
        {
            HandlePress(collision);
        }
    }

    private bool IsPlayer(Transform obj)
    {
        return obj == player1 || obj == player2;
    }

    private void HandlePress(Collider2D collision)
    {
        Debug.Log("检测到玩家进入范围！");
        if (Time.time - lastPressTime < cooldown) return;
        //Debug.Log($"当前时间：{Time.time}，上次按动时间：{lastPressTime}，冷却时间：{cooldown},");
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (rb.velocity.y < pressThreshold)
            {
                rootPencil.OnCapPressed();
                lastPressTime = Time.time;
                rb.velocity = new Vector2(rb.velocity.x, 3f);
                Debug.Log("成功按动笔帽！");
            }
        }
    }
}
