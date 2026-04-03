/**
 * @file RotationSystem.cs
 * @brief 双人游戏旋转系统总控 - 管理世界旋转和玩家姿态保持
 * @author ZHY
 * @version 3.0
 * @time 26-4-1
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSystem : MonoBehaviour
{
    public static RotationSystem Instance { get; private set; }
    
    [Header("引用")]
    public Transform worldRoot;
    public List<PlayerController> allPlayers = new List<PlayerController>();
    
    [Header("旋转参数")]
    public float rotationDuration = 0.5f;
    public float safetyMargin = 0.0f;
    public int rayCount = 3;
    public LayerMask groundLayer;
    
    [Header("地面检测")]
    public float wallCheckDistance = 10f;
    
    private bool isRotating = false;
    public float playerHeight;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        
        
        // 自动收集场景中的所有玩家
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        allPlayers.AddRange(players);
        
        // 获取玩家高度（假设所有玩家高度相同）
        playerHeight = allPlayers[0].GetComponent<BoxCollider2D>().bounds.size.y;
        
    }
    
    /// <summary>
    /// 注册玩家到旋转系统
    /// </summary>
    public void RegisterPlayer(PlayerController player)
    {
        if (!allPlayers.Contains(player))
        {
            allPlayers.Add(player);
        }
    }
    
    /// <summary>
    /// 发起旋转请求（由玩家调用）
    /// </summary>
    public bool RequestRotation(float angleDelta, PlayerController initiator)
    {
        if (isRotating) 
        {
            return false;
        }
        
        if (worldRoot == null)
        {
            return false;
        }
        
        // 1. 计算发起者的旋转轴
        Vector2 initiatorPivot = CalculatePivotForPlayer(initiator, angleDelta);
        
        // 检查发起者自身是否安全（x > 1000000 表示不安全）
        if (initiatorPivot.x > 1000000f)
        {
            return false;
        }
        
        // 2. 检查所有其他玩家的安全性
        var participantData = new List<RotationParticipantData>();
        
        foreach (var player in allPlayers)
        {
            if (player == initiator) continue;
            
            // 检查安全距离
            if (!IsPlayerSafeForRotation(player, initiatorPivot, angleDelta))
            {
                return false;
            }
            
            // 计算该玩家的安全旋转轴（与发起者使用相同的逻辑）
            // 这样玩家B的那个角会始终与移动的安全旋转轴重合
            float playerLeftDist = GetDistanceToLeftWall(player);
            float playerRightDist = GetDistanceToRightWall(player);
            Vector2 selfPivot = SelectRotationAxis(angleDelta, playerLeftDist, playerRightDist, player);
            
            participantData.Add(new RotationParticipantData
            {
                player = player,
                selfPivot = selfPivot,
                startPosition = player.transform.position,
                startRotation = player.transform.rotation
            });
        }
        
        // 3. 所有检查通过，开始旋转
        StartCoroutine(ExecuteRotation(angleDelta, initiatorPivot, initiator, participantData));
        return true;
    }
    
    /// <summary>
    /// 执行旋转（协程确保同步）
    /// </summary>
    private IEnumerator ExecuteRotation(float angleDelta, Vector2 worldPivot, 
        PlayerController initiator, List<RotationParticipantData> participants)
    {
        isRotating = true;
        
        // 通知所有玩家进入旋转状态，并将Rigidbody2D设为Kinematic
        foreach (var player in allPlayers)
        {
            player.EnterRotationState();
            // 设置为Kinematic，避免物理干扰
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
        
        // 准备阶段 - 计算目标值
        Quaternion worldStartRot = worldRoot.rotation;
        Quaternion worldTargetRot = worldStartRot * Quaternion.Euler(0, 0, angleDelta);
        Vector3 worldStartPos = worldRoot.position;
        
        // 计算世界根节点相对于旋转轴的偏移
        Vector3 worldPivot3D = new Vector3(worldPivot.x, worldPivot.y, worldRoot.position.z);
        Vector3 worldOffset = worldStartPos - worldPivot3D;
        
        // 计算各玩家的目标值
        foreach (var p in participants)
        {
            p.CalculateTargets(worldPivot, angleDelta);
        }
        
        // 执行阶段
        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            
            // 世界旋转（使用Slerp）
            worldRoot.rotation = Quaternion.Slerp(worldStartRot, worldTargetRot, t);
            
            // 世界位置：使用球面插值实现圆弧轨迹
            // 对相对于旋转轴的偏移进行球面插值
            Quaternion currentRot = Quaternion.Euler(0, 0, angleDelta * t);
            Vector3 currentOffset = currentRot * worldOffset;
            worldRoot.position = worldPivot3D + currentOffset;
            
            // 玩家跟随世界旋转
            foreach (var p in participants)
            {
                p.UpdateTransform(t, worldPivot, angleDelta);
            }
            
            yield return null;
        }
        
        // 完成阶段 - 精确设置最终值
        worldRoot.rotation = worldTargetRot;
        // 计算最终位置（使用球面插值的方式）
        Quaternion finalRot = Quaternion.Euler(0, 0, angleDelta);
        Vector3 finalOffset = finalRot * worldOffset;
        worldRoot.position = worldPivot3D + finalOffset;
        
        foreach (var p in participants)
        {
            p.Finalize();
        }
        
        // 通知所有玩家退出旋转状态，并恢复Rigidbody2D为Dynamic
        foreach (var player in allPlayers)
        {
            player.ExitRotationState();
            // 恢复为Dynamic，重新启用物理
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        
        isRotating = false;
    }
    
    #region 旋转轴计算
    
    /// <summary>
    /// 计算玩家的旋转轴（基于安全距离判断）
    /// </summary>
    private Vector2 CalculatePivotForPlayer(PlayerController player, float angleDelta)
    {
        float leftDist = GetDistanceToLeftWall(player);
        float rightDist = GetDistanceToRightWall(player);
        return SelectRotationAxis(angleDelta, leftDist, rightDist, player);
    }
    
    /// <summary>
    /// 计算玩家自转轴（保持姿态）
    /// 自转轴位于玩家底部，用于保持姿态的自转
    /// </summary>
    private Vector2 CalculateSelfPivot(PlayerController player, float angleDelta)
    {
        Bounds bounds = player.GetColliderBounds();
        
        // 自转轴位于玩家底部中心
        // 根据公转方向选择底部左侧或右侧作为自转轴
        if (angleDelta < 0) // 顺时针公转 -> 自转轴在左下角
        {
            return new Vector2(bounds.min.x, bounds.min.y);
        }
        else // 逆时针公转 -> 自转轴在右下角
        {
            return new Vector2(bounds.max.x, bounds.min.y);
        }
    }
    
    /// <summary>
    /// 选择旋转轴（基于左右安全距离）
    /// 根据左右安全状态和旋转方向选择旋转轴
    /// 返回值：旋转轴的世界坐标，如果不可旋转则返回 (9999999, 9999999)
    /// </summary>
    private Vector2 SelectRotationAxis(float angleDelta, float leftDist, float rightDist, PlayerController player)
    {
        bool leftSafe = IsSafeDistance(leftDist);
        bool rightSafe = IsSafeDistance(rightDist);
        // 获取角色的四个角
        var bounds = GetPlayerBounds(player);
        
        // 两侧都不安全，不允许旋转
        if (!leftSafe && !rightSafe)
        {
            return new Vector2(9999999f, 9999999f);
        }
        
        // 顺时针旋转（angleDelta < 0，因为顺时针是负角度）
        if (angleDelta < 0)  // E键，顺时针
        {
            if (!leftSafe && rightSafe)
            {
                // 左侧不安全，右侧安全：以左上角为轴
                return bounds.topLeft;
            }
            else if (leftSafe && !rightSafe)
            {
                // 左侧安全，右侧不安全：以左下角为轴
                return bounds.bottomLeft;
            }
            else // 两侧都安全
            {
                // 默认以左下角为轴
                return bounds.bottomLeft;
            }
        }
        else // 逆时针旋转（angleDelta > 0），Q键
        {
            if (!leftSafe && rightSafe)
            {
                // 左侧不安全，右侧安全：以右下角为轴
                return bounds.bottomRight;
            }
            else if (leftSafe && !rightSafe)
            {
                // 左侧安全，右侧不安全：以右上角为轴
                return bounds.topRight;
            }
            else // 两侧都安全
            {
                // 默认以右下角为轴
                return bounds.bottomRight;
            }
        }
    }
    
    /// <summary>
    /// 获取玩家碰撞箱的四个角
    /// </summary>
    private (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) GetPlayerBounds(PlayerController player)
    {
        Bounds bounds = player.GetColliderBounds();
        
        return (
            new Vector2(bounds.min.x, bounds.max.y),  // 左上
            new Vector2(bounds.max.x, bounds.max.y),  // 右上
            new Vector2(bounds.min.x, bounds.min.y),  // 左下
            new Vector2(bounds.max.x, bounds.min.y)   // 右下
        );
    }
    
    #endregion
    
    #region 安全检查
    
    /// <summary>
    /// 检查玩家是否处于安全状态（基于左右安全距离）
    /// </summary>
    private bool IsPlayerSafeForRotation(PlayerController player, Vector2 worldPivot, float angleDelta)
    {
        // 获取玩家到左右墙壁的距离
        float leftDist = GetDistanceToLeftWall(player);
        float rightDist = GetDistanceToRightWall(player);
        
        // 判断两侧是否安全
        bool leftSafe = IsSafeDistance(leftDist);
        bool rightSafe = IsSafeDistance(rightDist);
        
        // 只要有一侧安全，就可以旋转
        return leftSafe || rightSafe;
    }
    
    /// <summary>
    /// 判断距离是否安全
    /// </summary>
    private bool IsSafeDistance(float distance)
    {
        return distance >= playerHeight - safetyMargin;
    }
    
    /// <summary>
    /// 获取与左侧墙壁的距离
    /// </summary>
    private float GetDistanceToLeftWall(PlayerController player)
    {
        Bounds bounds = player.GetColliderBounds();
        float startY = bounds.min.y;
        float endY = bounds.max.y;
        float minDistance = Mathf.Infinity;
        
        for (int i = 0; i < rayCount; i++)
        {
            float t = i / (float)(rayCount - 1);
            float y = Mathf.Lerp(startY, endY, t);
            Vector2 origin = new Vector2(bounds.min.x, y);
            
            // 使用RaycastAll获取所有击中结果，排除玩家自己
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.left, wallCheckDistance, groundLayer);
            
            foreach (var hit in hits)
            {
                // 排除玩家自己的碰撞器
                if (hit.collider != null && hit.collider.gameObject != player.gameObject)
                {
                    float distance = origin.x - hit.point.x;
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                    break; // 找到第一个有效击中就退出（最近的）
                }
            }
        }
        
        return minDistance;
    }
    
    /// <summary>
    /// 获取与右侧墙壁的距离
    /// </summary>
    private float GetDistanceToRightWall(PlayerController player)
    {
        Bounds bounds = player.GetColliderBounds();
        float startY = bounds.min.y;
        float endY = bounds.max.y;
        float minDistance = Mathf.Infinity;
        
        for (int i = 0; i < rayCount; i++)
        {
            float t = i / (float)(rayCount - 1);
            float y = Mathf.Lerp(startY, endY, t);
            Vector2 origin = new Vector2(bounds.max.x, y);
            
            // 使用RaycastAll获取所有击中结果，排除玩家自己
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.right, wallCheckDistance, groundLayer);
            
            foreach (var hit in hits)
            {
                // 排除玩家自己的碰撞器
                if (hit.collider != null && hit.collider.gameObject != player.gameObject)
                {
                    float distance = hit.point.x - origin.x;
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                    break; // 找到第一个有效击中就退出（最近的）
                }
            }
        }
        
        return minDistance;
    }
    
    #endregion
}

/// <summary>
/// 旋转参与者数据
/// </summary>
public class RotationParticipantData
{
    public PlayerController player;
    public Vector2 selfPivot;  // 玩家自身的旋转轴（用于自转）
    public Vector3 startPosition;
    public Quaternion startRotation;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 revolutionStartRelPos;  // 相对于世界旋转轴的偏移
    private Vector3 selfPivotStartRelPos;   // 玩家中心相对于自转轴的偏移
    
    public void CalculateTargets(Vector2 worldPivot, float angleDelta)
    {
        // 记录玩家中心相对于安全旋转轴的偏移
        // 安全旋转轴 = selfPivot (CalculatePivotForPlayer计算出的旋转轴)
        selfPivotStartRelPos = startPosition - new Vector3(selfPivot.x, selfPivot.y, startPosition.z);
    }
    
    public void UpdateTransform(float t, Vector2 worldPivot, float angleDelta)
    {
        // 计算安全旋转轴随世界旋转后的位置
        // 安全旋转轴的初始位置
        Vector3 pivotStartPos = new Vector3(selfPivot.x, selfPivot.y, startPosition.z);
        // 安全旋转轴相对于世界旋转轴的偏移
        Vector3 pivotRelToWorld = pivotStartPos - new Vector3(worldPivot.x, worldPivot.y, startPosition.z);
        
        // 世界旋转
        Quaternion currentWorldRot = Quaternion.Euler(0, 0, angleDelta * t);
        // 安全旋转轴公转后的位置
        Vector3 currentPivotPos = new Vector3(worldPivot.x, worldPivot.y, startPosition.z) 
                                  + currentWorldRot * pivotRelToWorld;
        
        // 玩家位置 = 旋转后的安全旋转轴位置 + 原始偏移
        // 保持rotation不变，只改变位置
        Vector3 currentPos = currentPivotPos + selfPivotStartRelPos;
        
        player.transform.position = currentPos;
        // rotation保持不变，不自转
    }
    
    public void Finalize()
    {
        // 最终位置使用最后一次计算的位置
        // rotation保持不变
    }
}
