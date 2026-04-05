using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("UI 引用")]
    public Image[] heartIcons; // 拖入 6 个心形
    public Image cardIcon;    // 拖入校园卡图片

    // 强制初始化
    void Awake()
    {
        // 1. 隐藏校园卡物体
        if (cardIcon != null) 
        {
            cardIcon.gameObject.SetActive(false); 
        }

        // 2. 初始只显示 1 个爱心
        SetHeartDisplay(1); 
    }

    // 核心显示逻辑
    public void SetHeartDisplay(int count)
    {
        if (heartIcons == null || heartIcons.Length == 0) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // 只有索引小于 count 的心才显示（比如 count 为 1，则只有 i=0 显示）
                heartIcons[i].enabled = (i < count);
            }
        }
    }

public void ShowCard()
{
    if (cardIcon != null) 
    {
        cardIcon.gameObject.SetActive(true); // 1. 激活整个物体
        cardIcon.enabled = true;             // 2. 激活 Image 组件
        Debug.Log("UI：卡片已成功显示！");   // 👈 如果控制台不打印这行，说明逻辑没跑到这
    }
    else
    {
        Debug.LogError("UI：没找到 cardIcon 引用！");
    }
}
}