/**
 * @file WorldRotateWithPlayer.cs
 * @brief 修复人物贴墙也能旋转场景的bug
 * @author ZHY
 * @version 1.3
 * @time 26-3-30 0-0-03 ~~ 26-3-30 0-
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
    
    [Header("旋转参数")]
    public float rotationDuration = 0.5f;
    
    [Header("地面检测")]
    public LayerMask groundLayer;
    
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float rotationTimer = 0;
    private float fallTimer = 0;
    
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
    private void HandleCommonState()
    {
        if (Input.GetKeyDown(KeyCode.E)) // E代表顺时针旋转
        {
            StartRotation(-90f);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // Q代表逆时针旋转
        {
            StartRotation(90f);
        }
    }
    
    // 处理 isRotating 状态
    private void HandleRotatingState()
    {
        UpdateRotation();
        
        // 检查是否旋转完成
        if (IsRotationComplete())
        {
            FinishRotation();
        }
    }
    
    // 处理 hasRotated 状态
    private void HandleHasRotatedState()
    {
        fallTimer += Time.deltaTime;
        
        // 检查是否落地(增加相机复位逻辑)
        if (fallTimer > 0.1f && cameraFollow.IsCameraReset() && playerController.isPlayerGrounded())
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
            Debug.LogWarning("超时，强制恢复控制");
        }
    }
//--------状态处理专用函数--------//

    // 开始旋转
    private void StartRotation(float angleDelta)
    {
        // 禁用玩家控制，设置状态为 Rotating；玩家在此状态下不可以移动不可以跳跃
        DisablePlayerControl();
        
        // 冻结玩家：player的Rigidbody不再有重力，不再有速度，但是仍然要计算碰撞（Dynamic）
        FreezePlayer();
        
        // 更新方向索引：顺时针index+1；逆时针index-1
        UpdateDirectionIndex(angleDelta);
        
        // 计算目标旋转-->目的：更新Z轴的目的角度（90度为单位）
        CalculateTargetRotation();
        
        // 计算目标位置-->计算相对相机旋转90度的位置（存在隐患）
        CalculateTargetPosition(angleDelta);
        
        // 初始化旋转计时器
        rotationTimer = 0;
        
        // 切换到旋转状态
        ChangeWorldState(WorldState.isRotating);
        
        Debug.Log($"开始旋转，方向索引：{currentDirectionIndex}，目标角度：{currentDirectionIndex * 90f}");
    }
    
    // 更新旋转
    private void UpdateRotation()
    {
        // 每次更新计算两帧时间间隔在0.5s的占比,注意rotationTimer是全局变量，会一直累加
        rotationTimer += Time.deltaTime / rotationDuration; // 规定0.5s完成旋转动画

        float t = rotationTimer;
        // 缓慢更新旋转中角度和位置，平滑旋转（对于函数的调用任然是黑箱，不过目前看来没问题）
        worldRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
        worldRoot.position = Vector3.Lerp(startPosition, targetPosition, t);
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
        // 设置为精确的目标值（之前出现过误差累积，旋转角度不再是严格90度）
        worldRoot.rotation = targetRotation;
        worldRoot.position = targetPosition;
        
        // 恢复物理
        RestorePlayerPhysics(); // 恢复Dynamic和重力（再此版中旋转仍然保持Dynamic）
        
        // 重置计时器
        fallTimer = 0;
        
        // 切换到等待落地状态
        ChangeWorldState(WorldState.hasRotated);
        player.GetComponent<PlayerController>().changeState(PlayerController.PlayerState.Falling);
        
        Debug.Log($"旋转完成，最终角度：{worldRoot.eulerAngles.z}");
    }

//--------小功能函数组--------//
    
    // 更新方向索引
    private void UpdateDirectionIndex(float angleDelta)
    {
        if (angleDelta > 0)  // 顺时针
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % 4;
        }
        else  // 逆时针
        {
            currentDirectionIndex = (currentDirectionIndex - 1 + 4) % 4;
        }
    }
    
    // 计算目标旋转
    private void CalculateTargetRotation()
    {
        // 检测到按下按键，更新index，自动计算新的z轴角度
        float targetAngle = currentDirectionIndex * 90f;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        startRotation = worldRoot.rotation;
    }
    
    // 计算目标位置
    private void CalculateTargetPosition(float angleDelta)
    {
        startPosition = worldRoot.position;
        
        if (mainCamera != null) // 每当按下QE键，都将以相机为轴进行旋转
        {
            Vector3 cameraPos = mainCamera.transform.position;
            Vector3 offsetFromCamera = startPosition - cameraPos;
            Vector3 rotatedOffset = Quaternion.Euler(0, 0, angleDelta) * offsetFromCamera;
            targetPosition = cameraPos + rotatedOffset;
        }
        //--->ZHY:存在隐患！人物落地，但由于延迟，相机尚未复位，此时旋转轴将出现问题
        //        解决方案：仅当相机复位并且人物脚部着陆，才设置为Grounding,允许旋转
        else
        {
            targetPosition = startPosition;
        }
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
        Debug.Log("玩家已落地，恢复控制");
    }
    
    // 切换世界状态
    private void ChangeWorldState(WorldState newState)
    {
        currentWorldState = newState;
    }
    
}
//--------小功能函数组--------//