using UnityEngine;
using System.Collections;

public class PlayerShield : MonoBehaviour
{
    [Header("UI 引用")]
    public PlayerStatusUI statusUI; // 👈 必须在 Inspector 里拖入该玩家对应的 HUD

    [Header("护盾设置")]
    public int shieldCount = 1; // 初始设为 1，对应 1 颗心
    public float invincibleDuration = 0.5f; 

    [Header("视觉反馈")]
    public SpriteRenderer playerSprite; 

    private float nextDamageTime = 0f; 

public void AddShield(int amount)
{
    // 1. 先打印当前有多少，方便调试
    Debug.Log($"加护盾前：{shieldCount}");

    // 2. 执行加法
    shieldCount += amount; 

    // 3. 强制限制在 6 以内
    shieldCount = Mathf.Clamp(shieldCount, 0, 6);

    Debug.Log($"加护盾后：{shieldCount}");

    // 4. 立即命令 UI 更新
    if (statusUI != null)
    {
        statusUI.SetHeartDisplay(shieldCount);
    }
}

    public bool TryUseShield()
    {
        if (Time.time < nextDamageTime) return true; 

        if (shieldCount > 0)
        {
            shieldCount--;
            nextDamageTime = Time.time + invincibleDuration;

            // 👈 同步更新状态栏
            if (statusUI != null) statusUI.SetHeartDisplay(shieldCount);

            if (playerSprite != null) StartCoroutine(FlashColor(Color.red, invincibleDuration));
            return true; 
        }
        return false; 
    }

    private IEnumerator FlashColor(Color targetColor, float duration)
    {
        if (playerSprite == null) yield break;
        Color originalColor = Color.white;
        playerSprite.color = targetColor;
        yield return new WaitForSeconds(duration);
        playerSprite.color = originalColor;
    }
}