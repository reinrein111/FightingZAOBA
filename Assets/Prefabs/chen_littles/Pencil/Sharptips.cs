using UnityEngine;

public class SharpTip : MonoBehaviour
{
    // 1. 当任何带有 Collider 的物体进入这个 Trigger 时触发
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 2. 判定对方是不是玩家（通过 Tag 或 Layer）
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=red>玩家碰到笔尖，瞬间暴毙！</color>");
            
            // 3. 执行死亡逻辑（调用你现有的尖刺致死脚本）
            Object.FindAnyObjectByType<SpikeTrigger>()?.ExecuteDeath();
            
            // 可选：让玩家物理静止，防止穿过去
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.simulated = false; // 禁用物理仿真
            }
        }
    }
}