using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("护盾设置")]
    [Tooltip("当前剩余的抗伤次数")]
    public int shieldCount = 0; 

    [Tooltip("受击后的无敌时间（秒），防止一瞬间扣光所有护盾")]
    public float invincibleDuration = 0.5f; 

    [Header("视觉反馈（可选）")]
    public SpriteRenderer playerSprite; // 拖入玩家的 SpriteRenderer，用于做闪烁效果

    private float nextDamageTime = 0f; // 下一次可以受到伤害的时间点

    /// <summary>
    /// 增加护盾层数（由包子脚本调用）
    /// </summary>
    public void AddShield(int amount)
    {
        shieldCount += amount;
        Debug.Log($"<color=green>吃了包子！增加 {amount} 层护盾，当前剩余：{shieldCount}</color>");
        
        // 可以在这里触发一个“吃到了”的反馈，比如变绿一下
        if (playerSprite != null) StartCoroutine(FlashColor(Color.green, 0.2f));
    }

    /// <summary>
    /// 尝试使用护盾抵扣伤害（由子弹或老师脚本调用）
    /// 返回 true: 抵扣成功，玩家不死
    /// 返回 false: 无护盾，玩家死亡
    /// </summary>
    public bool TryUseShield()
    {
        // 1. 如果还在无敌时间内，直接判定为“死不了”，且不扣层数
        if (Time.time < nextDamageTime)
        {
            return true; 
        }

        // 2. 如果有护盾，消耗一层
        if (shieldCount > 0)
        {
            shieldCount--;
            
            // 设置下一次可以受伤的时间（进入无敌状态）
            nextDamageTime = Time.time + invincibleDuration;

            Debug.Log($"<color=yellow>护盾抵挡伤害！剩余层数：{shieldCount}</color>");
            
            // 触发受击视觉反馈（比如变红闪烁）
            if (playerSprite != null) StartCoroutine(FlashColor(Color.red, invincibleDuration));
            
            return true; // 抵扣成功
        }

        // 3. 没护盾了
        return false; 
    }

    // 简单的变色反馈协程
    private System.Collections.IEnumerator FlashColor(Color targetColor, float duration)
    {
        if (playerSprite == null) yield break;
        Color originalColor = Color.white;
        playerSprite.color = targetColor;
        yield return new WaitForSeconds(duration);
        playerSprite.color = originalColor;
    }
}