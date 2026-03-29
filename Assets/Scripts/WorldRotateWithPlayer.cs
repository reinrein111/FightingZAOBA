using UnityEngine;

public class WorldRotateWithPlayer : MonoBehaviour
{
    [Header("引用")]
    public Transform worldRoot;
    public Transform player;
    public Rigidbody2D playerRb;
    public Camera mainCamera;
    
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
        hasRotated
    }
    
    private WorldState currentWorldState = WorldState.Common;
    
    // 记录当前应该处于哪个方向（0, 90, 180, 270, 360）
    private int currentDirectionIndex = 0;  // 0:0°, 1:90°, 2:180°, 3:270°
    
    void Start()
    {
        InitializeComponents();
        InitializeDirectionIndex();
    }
    
    void Update()
    {
        if (worldRoot == null || player == null || playerRb == null) return;
        
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
    
    // 初始化组件引用
    private void InitializeComponents()
    {
        if (worldRoot == null)
        {
            GameObject found = GameObject.Find("RootObject");
            if (found != null) worldRoot = found.transform;
        }
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) player = GameObject.Find("Player")?.transform;
        }
        
        if (player != null && playerRb == null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
        
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
    }
    
    // 初始化方向索引
    private void InitializeDirectionIndex()
    {
        float currentAngle = worldRoot.eulerAngles.z;
        currentDirectionIndex = Mathf.RoundToInt(currentAngle / 90f) % 4;
        if (currentDirectionIndex < 0) currentDirectionIndex += 4;
        
        Debug.Log($"初始化方向索引：{currentDirectionIndex}，角度：{currentAngle}");
    }
    
    // 处理 Common 状态
    private void HandleCommonState()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartRotation(-90f);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
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
        
        // 检查是否落地
        if (IsPlayerGrounded() && fallTimer > 0.1f)
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
    
    // 开始旋转
    private void StartRotation(float angleDelta)
    {
        // 禁用玩家控制，设置状态为 Rotating
        DisablePlayerControl();
        
        // 冻结玩家
        FreezePlayer();
        
        // 更新方向索引
        UpdateDirectionIndex(angleDelta);
        
        // 计算目标旋转
        CalculateTargetRotation();
        
        // 计算目标位置
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
        rotationTimer += Time.deltaTime / rotationDuration;
        
        float t = rotationTimer;
        worldRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
        worldRoot.position = Vector3.Lerp(startPosition, targetPosition, t);
    }
    
    // 检查旋转是否完成
    private bool IsRotationComplete()
    {
        return rotationTimer >= 1;
    }
    
    // 完成旋转
    private void FinishRotation()
    {
        // 设置为精确的目标值
        worldRoot.rotation = targetRotation;
        worldRoot.position = targetPosition;
        
        // 恢复物理
        RestorePlayerPhysics();
        
        // 重置计时器
        fallTimer = 0;
        
        // 切换到等待落地状态
        ChangeWorldState(WorldState.hasRotated);
        player.GetComponent<PlayerController>().ChangeState(PlayerController.PlayerState.Falling);
        
        Debug.Log($"旋转完成，最终角度：{worldRoot.eulerAngles.z}");
    }
    
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
        float targetAngle = currentDirectionIndex * 90f;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        startRotation = worldRoot.rotation;
    }
    
    // 计算目标位置
    private void CalculateTargetPosition(float angleDelta)
    {
        startPosition = worldRoot.position;
        
        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            Vector3 offsetFromCamera = startPosition - cameraPos;
            Vector3 rotatedOffset = Quaternion.Euler(0, 0, angleDelta) * offsetFromCamera;
            targetPosition = cameraPos + rotatedOffset;
        }
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
        playerRb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    // 恢复玩家物理
    private void RestorePlayerPhysics()
    {
        playerRb.bodyType = RigidbodyType2D.Dynamic;
        playerRb.gravityScale = 1;
    }
    
    // 禁用玩家控制
    private void DisablePlayerControl()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.ChangeState(PlayerController.PlayerState.Rotating);
    }
    
    // 恢复玩家控制
    private void RestorePlayerControl()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ChangeState(PlayerController.PlayerState.Grounding);
        }
        Debug.Log("玩家已落地，恢复控制");
    }
    
    // 检测玩家是否在地面（使用 Collider 检测）
    private bool IsPlayerGrounded()
    {
        if (player == null) return false;
        
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return false;
        
        return playerCollider.IsTouchingLayers(groundLayer);
    }
    
    // 切换世界状态
    private void ChangeWorldState(WorldState newState)
    {
        currentWorldState = newState;
    }
    
}
