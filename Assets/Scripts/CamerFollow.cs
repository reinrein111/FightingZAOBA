/**
 * @file CameraFollow.cs
 * @brief 修复人物贴墙也能旋转场景的bug
 * @author ZHY
 * @version 1.3
 * @time 26-3-30 0-0-03 ~~ 26-3-30 0-
 */
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;        // 跟随的目标（Player）
    
    [Header("偏移量")]
    public Vector3 offset = new Vector3(0, 0, -10);  // 相机相对于玩家的偏移
    
    [Header("跟随平滑度")]
    public float smoothSpeed = 5f;   // 跟随速度，值越大越灵敏
    [Header("复位判断阈值")]
    public float resetThreshold = 0.8f; // 允许的误差范围（越小越精确）
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 目标位置 = 玩家位置 + 偏移量
        Vector3 targetPosition = target.position + offset;
        
        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    public bool IsCameraReset()
    {
        if (target == null) return false; // 如果没有目标，直接返回 false

        Vector3 targetPosition = target.position + offset;
        float distance = Vector3.Distance(transform.position, targetPosition);

        return distance <= resetThreshold; // 如果距离小于阈值，认为已复位
    }
}