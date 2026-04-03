/**
 * @file PlayerCameraController.cs
 * @brief 单个玩家跟随相机控制器 - 平滑跟随指定玩家
 * @author ZHY
 * @version 1.0
 * @time 26-4-2
 */
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("目标玩家")]
    public Transform targetPlayer;
    public int targetPlayerId = 1; // 1 = 玩家A, 2 = 玩家B
    
    [Header("相机参数")]
    public float cameraSize = 8f;           // 相机视野大小
    public float smoothSpeed = 5f;          // 平滑跟随速度
    public float verticalOffset = 2f;       // 垂直偏移（让玩家在画面中心偏上）
    
    [Header("位置限制")]
    public bool useBounds = false;          // 是否使用边界限制
    public Vector2 minBounds;               // 最小边界
    public Vector2 maxBounds;               // 最大边界
    
    private Camera cam;
    private Vector3 targetPosition;
    
    void Start()
    {
        InitializeCamera();
        
        // 如果没有指定玩家，尝试自动查找
        if (targetPlayer == null)
        {
            AutoFindPlayer();
        }
        
        // 立即跳转到目标位置（避免初始延迟）
        SnapToTarget();
    }
    
    void LateUpdate()
    {
        if (targetPlayer == null)
        {
            // 尝试重新查找玩家
            AutoFindPlayer();
            return;
        }
        
        // 计算目标位置
        targetPosition = targetPlayer.position;
        targetPosition.z = -10f; // 保持相机z轴位置
        targetPosition.y += verticalOffset; // 添加垂直偏移
        
        // 应用边界限制
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
        
        // 平滑移动相机
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 初始化相机组件
    /// </summary>
    private void InitializeCamera()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
        
        // 设置为正交相机
        cam.orthographic = true;
        cam.orthographicSize = cameraSize;
        
        // 设置背景颜色（避免穿帮）
        cam.backgroundColor = Color.black;
        
        // 设置裁剪平面
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
    }
    
    /// <summary>
    /// 自动查找对应playerId的玩家
    /// </summary>
    private void AutoFindPlayer()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        
        foreach (var player in players)
        {
            if (player.playerId == targetPlayerId)
            {
                targetPlayer = player.transform;
                break;
            }
        }
    }
    
    /// <summary>
    /// 设置目标玩家（供外部调用）
    /// </summary>
    public void SetTargetPlayer(Transform player)
    {
        targetPlayer = player;
    }
    
    /// <summary>
    /// 立即跳转到目标位置（用于初始化）
    /// </summary>
    public void SnapToTarget()
    {
        if (targetPlayer == null) return;
        
        targetPosition = targetPlayer.position;
        targetPosition.z = -10f;
        targetPosition.y += verticalOffset;
        
        // 应用边界限制
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
        
        transform.position = targetPosition;
        cam.orthographicSize = cameraSize;
    }
    
    /// <summary>
    /// 设置相机视野大小
    /// </summary>
    public void SetCameraSize(float size)
    {
        cameraSize = size;
        if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }
    
    /// <summary>
    /// 设置Viewport（用于分屏显示）
    /// </summary>
    public void SetViewport(Rect viewport)
    {
        if (cam != null)
        {
            cam.rect = viewport;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (targetPlayer != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
            Gizmos.DrawWireSphere(targetPlayer.position, 0.5f);
        }
        
        // 绘制边界
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
