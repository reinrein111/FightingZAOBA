/**
 * @file SharedCameraController.cs
 * @brief 双人游戏共享相机控制器 - 相机位于两玩家中点，size随距离动态调整
 * @author ZHY
 * @version 3.0
 * @time 26-4-1
 */
using UnityEngine;

public class SharedCameraController : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player1;
    public Transform player2;
    
    [Header("相机参数")]
    public float minSize = 10f;           // 最小视野大小
    public float maxSize = 25f;           // 最大视野大小
    public float padding = 2f;            // 边缘留白
    public float smoothSpeed = 5f;        // 平滑速度
    public float verticalOffset = 2f;     // 垂直偏移（让玩家在画面中心偏上）
    
    private Camera cam;
    private Vector3 targetPosition;
    private float targetSize;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // 如果没有指定玩家，尝试自动查找
        if (player1 == null || player2 == null)
        {
            AutoFindPlayers();
        }
    }
    
    void LateUpdate()
    {
        if (player1 == null || player2 == null)
        {
            // 尝试重新查找玩家
            AutoFindPlayers();
            return;
        }
        
        // 计算两个玩家的中点
        Vector3 midPoint = (player1.position + player2.position) / 2f;
        targetPosition = midPoint;
        targetPosition.z = -10f; // 保持相机z轴位置
        targetPosition.y += verticalOffset; // 添加垂直偏移
        
        // 计算两个玩家的距离
        float distance = Vector3.Distance(player1.position, player2.position);
        
        // 根据距离计算目标size（考虑宽高比）
        float aspectRatio = cam.aspect;
        float verticalDistance = Mathf.Abs(player1.position.y - player2.position.y);
        float horizontalDistance = Mathf.Abs(player1.position.x - player2.position.x) / aspectRatio;
        
        // 取较大值作为基础距离，除以2是因为orthographicSize是半高
        float requiredSize = Mathf.Max(verticalDistance, horizontalDistance) / 2f + padding;
        
        // 限制在最小和最大size之间
        targetSize = Mathf.Clamp(requiredSize, minSize, maxSize);
        
        // 平滑移动相机
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        
        // 平滑调整size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, smoothSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 自动查找场景中的玩家
    /// </summary>
    private void AutoFindPlayers()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        
        if (players.Length >= 2)
        {
            // 按playerId排序
            System.Array.Sort(players, (a, b) => a.playerId.CompareTo(b.playerId));
            player1 = players[0].transform;
            player2 = players[1].transform;
        }
        else if (players.Length == 1)
        {
            // 只有一个玩家时，两个引用都指向同一个
            player1 = players[0].transform;
            player2 = players[0].transform;
        }
    }
    
    /// <summary>
    /// 设置玩家引用（供外部调用）
    /// </summary>
    public void SetPlayers(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }
    
    /// <summary>
    /// 立即跳转到目标位置（用于初始化）
    /// </summary>
    public void SnapToTarget()
    {
        if (player1 == null || player2 == null) return;
        
        Vector3 midPoint = (player1.position + player2.position) / 2f;
        targetPosition = midPoint;
        targetPosition.z = -10f;
        targetPosition.y += verticalOffset;
        
        float distance = Vector3.Distance(player1.position, player2.position);
        float aspectRatio = cam.aspect;
        float verticalDistance = Mathf.Abs(player1.position.y - player2.position.y);
        float horizontalDistance = Mathf.Abs(player1.position.x - player2.position.x) / aspectRatio;
        float requiredSize = Mathf.Max(verticalDistance, horizontalDistance) / 2f + padding;
        targetSize = Mathf.Clamp(requiredSize, minSize, maxSize);
        
        transform.position = targetPosition;
        cam.orthographicSize = targetSize;
    }
    
    void OnDrawGizmosSelected()
    {
        if (player1 != null && player2 != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 midPoint = (player1.position + player2.position) / 2f;
            Gizmos.DrawLine(player1.position, player2.position);
            Gizmos.DrawWireSphere(midPoint, 0.5f);
        }
    }
}
