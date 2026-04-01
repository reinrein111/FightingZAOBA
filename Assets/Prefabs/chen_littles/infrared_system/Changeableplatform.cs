using UnityEngine;

public class ChangeablePlatform : MonoBehaviour
{
    [Header("视觉设置")]
    public float idleAlpha = 0.2f;   // 虚化时的透明度（建议 0.2）
    public float activeAlpha = 1.0f; // 实体时的透明度（1.0）

    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        // 强制初始化为“虚体”状态
        SetPlatformState(false);
    }

    public void SetPlatformState(bool isSolid)
    {
        // 关键逻辑：控制碰撞体的启用/禁用
        if (col != null) 
        {
            col.enabled = isSolid; 
        }

        // 改变视觉透明度
        if (sr != null)
        {
            Color c = sr.color;
            c.a = isSolid ? activeAlpha : idleAlpha;
            sr.color = c;
        }
    }
}