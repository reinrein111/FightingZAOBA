using UnityEngine;

/// <summary>
/// 控制游戏对象持续旋转的脚本 (主要用于 compass 等静态图)
/// </summary>
public class CompassRotate : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("每秒旋转的角度")]
    public float rotateSpeed = 90f; 
    
    [Tooltip("是否顺时针旋转")]
    public bool clockwise = true;

    void Update()
    {
        // 确保游戏没有暂停时才旋转
        if (Time.timeScale > 0)
        {
            // 计算旋转方向
            float direction = clockwise ? -1f : 1f;
            // 执行旋转 (绕Z轴)
            transform.Rotate(0, 0, direction * rotateSpeed * Time.deltaTime);
        }
    }
}
