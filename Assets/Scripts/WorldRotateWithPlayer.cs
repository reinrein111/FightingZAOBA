/**
 * @file WorldRotateWithPlayer.cs
 * @brief 修复旋转碰撞问题
 * @author ZHY
 * @version 2.1
 * @time 26-3-31 早
 */
using UnityEngine;
public class WorldRotateWithPlayer : MonoBehaviour
{
    [Header("引用")]
    public Transform worldRoot;
    public Transform player;
    public Rigidbody2D playerRb;
    public Camera mainCamera;
    public CameraFollow cameraFollow;
    public PlayerController playerController;
    public BoxCollider2D playerCol;
    
    [Header("旋转参数")]
    public float rotationDuration = 0.5f;
    [Header("旋转安全检查")]
    public float safetyMargin = 0.05f;    // 安全余量，避免浮点误差
    public int rayCount = 3;              // 检测点数量（上、中、下）

    // 调试用变量
    private Vector2 lastSelectedPivot;    // 记录最后选择的旋转轴
    private bool lastCheckResult;         // 记录最后一次安全检查结果
    
    [Header("地面检测")]
    public LayerMask groundLayer;
    
    private Quaternion startRotation;
    private Quaternion targetRotation; 
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float rotationTimer = 0;
    private float fallTimer = 0;
    private Vector3 pivotPointWorld;      // 旋转轴点（世界坐标）
    private Vector3 startPositionRel;     // 旋转开始时，世界根节点相对于轴点的位置
    private Vector3 targetPositionRel;    // 旋转结束时，世界根节点相对于轴点的位置
    private bool isRotating = false;
    
    private enum WorldState
    {
        Common,
        isRotating,
        hasRotated,
        //illegal
    }
    
    private WorldState currentWorldState = WorldState.Common;
    
    // 记录当前应该处于哪个方向（0, 90, 180, 270, 360）
    private int currentDirectionIndex = 0;  // 0:0°, 1:90°, 2:180°, 3:270°
    private float lastDirectionUpdateTime = -1f;
    private float lastRotationTime = -1f;
    private float rotationCooldown = 1.9f;  // 比旋转动画时长稍长

    
    void Start()
    {
        InitializeComponents(); // 五大基本Component初始化
        InitializeDirectionIndex();
    }
    
    void Update()
    {
        // 不断进行状态判断和状态处理
        switch (currentWorldState)
        {
            case WorldState.Common:
                HandleCommonState();
                break;
            case WorldState.isRotating:
                HandleRotatingState();
                break;
            case WorldState.hasRotated:
                HandleHasRotatedState();
                break;
        }
    }

//--------Start中的初始化函数--------//
    // 初始化组件引用
    private void InitializeComponents() 
    {
        worldRoot = GameObject.Find("RootObject").transform; // Rootobject初始化
        player = GameObject.Find("Player").transform; // player初始化
        playerCol = player.GetComponent<BoxCollider2D>();
        playerRb = player.GetComponent<Rigidbody2D>(); // Rigidbody初始化
        mainCamera = Camera.main; // maincamera初始化
        groundLayer = LayerMask.GetMask("Ground"); // GroundLayer初始化
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        playerController = player.GetComponent<PlayerController>();
    }
    
    // 初始化方向索引
    private void InitializeDirectionIndex()
    {
        float currentAngle = worldRoot.eulerAngles.z; // 获取在z轴上的旋转角度（单位：度，范围：0~360度）
        currentDirectionIndex = Mathf.RoundToInt(currentAngle / 90f) % 4; // 角度参数初始化
        // 0：旋转角度[0,90)；1：旋转角度[90,180)...
        if (currentDirectionIndex < 0) currentDirectionIndex += 4; // 防止角度是负数
    }
//--------Start中的初始化函数--------//

//--------状态处理专用函数--------//
    // 处理 Common 状态
    // 处理 Common 状态
    private void HandleCommonState()
    {
        if (currentWorldState != WorldState.Common) return;
        if (Time.time - lastRotationTime < rotationCooldown) return;
        if (Input.GetKeyDown(KeyCode.E)) // E代表顺时针旋转（-90度）
        {
            lastRotationTime = Time.time;
//            //Debug.Log($"E按下，当前currentDirectionIndex: {currentDirectionIndex}, currentWorldState: {currentWorldState}");
            float angleDelta = -90f;
            if (CanRotateSafely(angleDelta, out Vector2 pivotPoint))
            {
//                //Debug.Log($"in 129 line, now pivotpoint:{pivotPoint}");
                StartRotation(angleDelta, pivotPoint);
            }
            else
            {
 //               //Debug.Log("旋转被阻止：安全距离不足");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // Q代表逆时针旋转（+90度）
        {
//            //Debug.Log($"Q按下，当前currentDirectionIndex: {currentDirectionIndex}, currentWorldState: {currentWorldState}");
            float angleDelta = 90f;
            if (CanRotateSafely(angleDelta, out Vector2 pivotPoint))
            {
//                //Debug.Log($"in 129 line, now pivotpoint:{pivotPoint}");
                StartRotation(angleDelta, pivotPoint);// 此处pivot已经无定义了
            }
            else
            {
//                //Debug.Log("旋转被阻止：安全距离不足");
            }
        }
    }
    
    // 处理 isRotating 状态
    private void HandleRotatingState()
    {
        //Debug.Log("进入UpdateRotation");
        UpdateRotation();
        
        // 检查是否旋转完成
        if (IsRotationComplete())
        {
//            //Debug.Log("进入FinishRotation");
            FinishRotation();
        }
    }
    
    // 处理 hasRotated 状态
    private void HandleHasRotatedState()
    {
        fallTimer += Time.deltaTime;
        
        // 检查是否落地
        if (fallTimer > 0.1f && playerController.isPlayerGrounded())
        {
            RestorePlayerControl();
            ChangeWorldState(WorldState.Common);
            fallTimer = 0;
        }
        
        // 超时保护
        if (fallTimer > 3f)
        {
            RestorePlayerControl();
            ChangeWorldState(WorldState.Common);
//            //Debug.LogWarning("超时，强制恢复控制");
        }
    }
//--------状态处理专用函数--------//

    // 修复：加入旋转轴，动态调整转轴
    private void StartRotation(float angleDelta, Vector2 pivotPoint)
    {
        if (isRotating)
        {
            //Debug.LogWarning("旋转正在进行中，忽略新请求");
            return;
        }
    
        isRotating = true;
        
    
        //Debug.Log($"StartRotation调用前 currentDirectionIndex: {currentDirectionIndex}");
        // 禁用玩家控制，设置状态为 Rotating；玩家在此状态下不可以移动不可以跳跃
        DisablePlayerControl();
        
        // 冻结玩家：player的Rigidbody不再有重力，不再有速度，但是仍然要计算碰撞（Dynamic）
        FreezePlayer();
        
        // 更新方向索引：顺时针index+1；逆时针index-1
        UpdateDirectionIndex(angleDelta);
        
        // 计算目标旋转-->目的：更新Z轴的目的角度（90度为单位）
        CalculateTargetRotation();
        
        // 计算目标位置-->计算相对相机旋转90度的位置（存在隐患）
        CalculateTargetTransform(angleDelta, pivotPoint);
        
        // 初始化旋转计时器
        rotationTimer = 0;
        
        // 切换到旋转状态
        ChangeWorldState(WorldState.isRotating);
        
        //Debug.Log($"开始旋转，方向索引：{currentDirectionIndex}，目标角度：{currentDirectionIndex * 90f}，旋转轴：{pivotPoint}");
    }
    
    // 更新旋转
    private void UpdateRotation()
    {
        rotationTimer += Time.deltaTime / rotationDuration;
        float t = rotationTimer;
        
        // 旋转角度插值
        worldRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
        
        // 位置插值：对相对位置进行球面插值，实现圆弧轨迹
        Vector3 currentRel = Vector3.Slerp(startPositionRel, targetPositionRel, t);
//        //Debug.Log($"In line 214, now pivot is{pivotPointWorld}"); // 出问题！
        worldRoot.position = pivotPointWorld + currentRel;
    }
    
    // 检查旋转是否完成
    private bool IsRotationComplete()
    {
        return rotationTimer >= 1; 
        //--->ZHY:可能存在隐患？万一rotationTimer在时间上计算出来已经到1但是角度由于平滑旋转，尚未归位？
        //        若未遇到问题，暂不修复
    }
    
    // 完成旋转
    private void FinishRotation()
    {
        //Debug.Log($"FinishRotation调用前 currentDirectionIndex: {currentDirectionIndex}");
        // 设置为精确的目标值（之前出现过误差累积，旋转角度不再是严格90度）
        worldRoot.rotation = targetRotation;
    //    //Debug.Log($"in line 232, {targetPosition}");
        worldRoot.position = targetPosition; // 调用出错(NaH)
        
        // 恢复物理
        RestorePlayerPhysics(); // 恢复Dynamic和重力（再此版中旋转仍然保持Dynamic）
        
        // 重置计时器
        fallTimer = 0;
        
        // 切换到等待落地状态
        ChangeWorldState(WorldState.hasRotated);
        player.GetComponent<PlayerController>().changeState(PlayerController.PlayerState.Falling);
        
        //Debug.Log($"旋转完成，最终角度：{worldRoot.eulerAngles.z}");
    }
//--------新增安全距离检查专用函数接口--------//
// 获取角色碰撞箱的四个角（世界坐标）
    private (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) GetPlayerBounds()
    {   
        Bounds bounds = playerCol.bounds;
        
        return (
            new Vector2(bounds.min.x, bounds.max.y),  // 左上
            new Vector2(bounds.max.x, bounds.max.y),  // 右上
            new Vector2(bounds.min.x, bounds.min.y),  // 左下
            new Vector2(bounds.max.x, bounds.min.y)   // 右下
        );
    }

    // 获取角色当前的实际高度（世界单位）[存疑]
    private float GetPlayerHeight()
    {
        if (playerCol == null) return 1f;
        return playerCol.bounds.size.y;
    }
    //--------安全检查判断主接口--------//
    // 检查是否可以安全旋转
    // 返回：是否安全，以及旋转轴点（out参数）
    private bool CanRotateSafely(float angleDelta, out Vector2 pivotPoint)
    {
        pivotPoint = Vector2.negativeInfinity;
        
        // 前置条件：必须在Grounding状态
        if (playerController.currentState != PlayerController.PlayerState.Grounding)
        {
//            //Debug.Log("旋转失败：不在Grounding状态");  
            return false;
        }
        
        // 获取左右墙壁距离
        float leftDist = GetDistanceToLeftWall();
        float rightDist = GetDistanceToRightWall();
        
        // 调试输出
//        //Debug.Log($"左墙距离: {leftDist:F2}, 右墙距离: {rightDist:F2}, 角色高度: {GetPlayerHeight():F2}");
        
        // 选择旋转轴
        pivotPoint = SelectRotationAxis(angleDelta, leftDist, rightDist); // Nah
        
        // 记录调试信息
        lastSelectedPivot = pivotPoint;
        lastCheckResult = (pivotPoint != Vector2.negativeInfinity);
        return !(pivotPoint.x < -10000000 || pivotPoint.y < -10000000);
    }
    //--------安全检查判断主接口--------//

    //--------墙壁安全距离计算函数--------//
    // 获取与左侧墙壁的最小距离（多点检测）
    private float GetDistanceToLeftWall()
    {   
        Bounds bounds = playerCol.bounds;
        float startY = bounds.min.y;
        float endY = bounds.max.y;
        float minDistance = Mathf.Infinity;
        // 射线检测
        for (int i = 0; i < rayCount; i++)
        {
            float t = i / (float)(rayCount - 1);
            float y = Mathf.Lerp(startY, endY, t);
            
            Vector2 origin = new Vector2(bounds.min.x, y);
            
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.left, Mathf.Infinity, groundLayer);
            
            if (hit.collider != null)
            {
                float distance = origin.x - hit.point.x;
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }
        return minDistance;
    }

    // 获取与右侧墙壁的最小距离（多点检测）
    private float GetDistanceToRightWall()
    { 
        Bounds bounds = playerCol.bounds;
        float startY = bounds.min.y;
        float endY = bounds.max.y;
        
        float minDistance = Mathf.Infinity;
        
        for (int i = 0; i < rayCount; i++)
        {
            float t = i / (float)(rayCount - 1);
            float y = Mathf.Lerp(startY, endY, t);
            
            Vector2 origin = new Vector2(bounds.max.x, y);
            
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, Mathf.Infinity, groundLayer);
            
            if (hit.collider != null)
            {
                float distance = hit.point.x - origin.x;
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }
        
        return minDistance;
    }

    // 判断距离是否安全（距离 >= 角色高度）
    private bool IsSafeDistance(float distance)
    {
        float playerHeight = GetPlayerHeight();
        return distance >= playerHeight - safetyMargin;
    }
    //--------墙壁安全距离计算函数--------//

    //--------安全距离判断核心函数：旋转轴选择函数--------//
    // 根据左右安全状态和旋转方向选择旋转轴
    // 返回值：旋转轴的世界坐标，如果不可旋转则返回Vector2.negativeInfinity[存疑]
    private Vector2 SelectRotationAxis(float angleDelta, float leftDist, float rightDist)
    {
//        //Debug.Log("进入旋转轴判断函数！");
        float playerHeight = GetPlayerHeight();
        bool leftSafe = IsSafeDistance(leftDist);
        bool rightSafe = IsSafeDistance(rightDist);
        
        // 获取角色的四个角
        var bounds = GetPlayerBounds();//？
        
        // 两侧都不安全，不允许旋转
        if (!leftSafe && !rightSafe)
        {
//            //Debug.Log("旋转失败：两侧空间都不足");
            return Vector2.negativeInfinity; // ->
        }
        
        // 顺时针旋转（angleDelta < 0，因为顺时针是负角度）
        if (angleDelta < 0)  // E键，顺时针
        {
            if (!leftSafe && rightSafe)
            {
                // 左侧不安全，右侧安全：以左上角为轴
                //Debug.Log("选择旋转轴：左上角");
                return bounds.topLeft;
            }
            else if (leftSafe && !rightSafe)
            {
                // 左侧安全，右侧不安全：以左下角为轴
                //Debug.Log("选择旋转轴：左下角");
                return bounds.bottomLeft;
            }
            else // 两侧都安全
            {
                // 默认以左下角为轴
                //Debug.Log("选择旋转轴：左下角（默认）");
                return bounds.bottomLeft;
            }
        }
        else // 逆时针旋转（angleDelta > 0），Q键
        {
            if (!leftSafe && rightSafe)
            {
                // 左侧不安全，右侧安全：以右下角为轴
                //Debug.Log("选择旋转轴：右下角");
                return bounds.bottomRight;
            }
            else if (leftSafe && !rightSafe)
            {
                // 左侧安全，右侧不安全：以右上角为轴
                //Debug.Log("选择旋转轴：右上角");
                return bounds.topRight;
            }
            else // 两侧都安全
            {
                // 默认以右下角为轴
//                //Debug.Log("选择旋转轴：右下角（默认）");
                return bounds.bottomRight;
            }
        }
    }
    //--------安全距离判断核心函数：旋转轴选择函数--------//
//--------新增安全距离检查专用函数接口--------//
//--------小功能函数组--------//
    
    // 更新方向索引
    private void UpdateDirectionIndex(float angleDelta)
    {
        if (Time.time - lastDirectionUpdateTime < 0.1f)
        {
//            //Debug.LogWarning($"UpdateDirectionIndex 被频繁调用，间隔: {Time.time - lastDirectionUpdateTime}");
            return;
        }
        lastDirectionUpdateTime = Time.time;
        int oldIndex = currentDirectionIndex;
        if (angleDelta > 0)  // 顺时针
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % 4;
        }
        else  // 逆时针
        {
            currentDirectionIndex = (currentDirectionIndex - 1 + 4) % 4;
        }
//        //Debug.Log($"UpdateDirectionIndex: {oldIndex} -> {currentDirectionIndex}, angleDelta: {angleDelta}, 调用堆栈: {StackTraceUtility.ExtractStackTrace()}");
    }
    
    // 计算目标旋转
    private void CalculateTargetRotation()
    {
        // 检测到按下按键，更新index，自动计算新的z轴角度
        float targetAngle = currentDirectionIndex * 90f;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        startRotation = worldRoot.rotation;
    }
    
    // 计算目标位置：加入旋转轴，动态调整旋转轴
    // 计算旋转相关的目标位置和相对位置
    private void CalculateTargetTransform(float angleDelta, Vector2 pivotPoint)
    {
        // 保存旋转轴点
        pivotPointWorld = pivotPoint;
        
        // 记录起始世界位置
        startPosition = worldRoot.position;
        
        // 计算起始相对位置（相对于旋转轴）
        startPositionRel = startPosition - pivotPointWorld;
        
        // 计算旋转后的相对位置
        Quaternion rotationDelta = Quaternion.Euler(0, 0, angleDelta);
        targetPositionRel = rotationDelta * startPositionRel;
        
        // 计算目标世界位置
        targetPosition = pivotPointWorld + targetPositionRel;
    }
    
    // 冻结玩家
    private void FreezePlayer()
    {
        playerRb.velocity = Vector2.zero;
        playerRb.gravityScale = 0;
        // playerRb.bodyType = RigidbodyType2D.Kinematic;
        //--->ZHY：在旋转过程中将玩家视作Dynamic
        //         好处：不再有穿模的问题；坏处：有的时候旋转会比较“敏感”
    }
    
    // 恢复玩家物理
    private void RestorePlayerPhysics()
    {
        playerRb.bodyType = RigidbodyType2D.Dynamic;
        playerRb.gravityScale = 1;
        isRotating = false;
    }
    
    // 禁用玩家控制：切换PlayerController中的状态变量到Rotating状态
    private void DisablePlayerControl()
    {
        // Rotating状态下玩家不可以移动也不可以跳跃！即操作冻结
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.changeState(PlayerController.PlayerState.Rotating);
    }
    
    // 恢复玩家控制
    private void RestorePlayerControl()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.changeState(PlayerController.PlayerState.Grounding);
        }
//        //Debug.Log("玩家已落地，恢复控制");
    }
    
    // 切换世界状态
    private void ChangeWorldState(WorldState newState)
    {
        currentWorldState = newState;
    }
    
}
//--------小功能函数组--------//