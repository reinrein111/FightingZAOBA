using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("UI 引用")]
    public Image[] heartIcons;    
    public Image cardIcon;        

    [Header("当前显示数值 (只读)")]
    public int currentHealth = 1; 

    // 重点：改成 Awake，确保它比其他脚本更早准备好
    void Awake()
    {
        if (cardIcon != null) cardIcon.enabled = false;
        UpdateUI(); 
    }

    public void SetHeartDisplay(int count)
    {
        currentHealth = count;
        UpdateUI();
    }

    public void ShowCard()
    {
        if (cardIcon != null) cardIcon.enabled = true;
    }

    private void UpdateUI()
    {
        if (heartIcons == null || heartIcons.Length == 0) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // 核心逻辑：只有索引小于血量的才显示
                heartIcons[i].enabled = (i < currentHealth);
                
                // 调试日志：如果运行了，控制台会打印
                // Debug.Log($"方块 {i} 状态设置为: {heartIcons[i].enabled}");
            }
        }
    }
}