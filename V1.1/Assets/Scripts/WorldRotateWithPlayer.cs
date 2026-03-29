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
    
    private bool isRotating = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float rotationTimer = 0;
    
    private bool hasRotated = false;
    private float fallTimer = 0;
    private bool playerControlRestored = false;
    
    // 记录当前应该处于哪个方向（0, 90, 180, 270, 360）
    private int currentDirectionIndex = 0;  // 0:0°, 1:90°, 2:180°, 3:270°
    
    void Start()
    {
        // 自动查找组件
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
        
        // 初始化：根据当前实际角度计算应该处于哪个方向
        float currentAngle = worldRoot.eulerAngles.z;
        currentDirectionIndex = Mathf.RoundToInt(currentAngle / 90f) % 4;
        // 标准化到 0-3
        if (currentDirectionIndex < 0) currentDirectionIndex += 4;
        
        Debug.Log($"初始化方向索引: {currentDirectionIndex}, 角度: {currentAngle}");
    }
    
    void Update()
    {
        if (worldRoot == null || player == null || playerRb == null) return;
        
        // 输入检测
        if (!isRotating && !hasRotated)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                StartRotation(-90f);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                StartRotation(90f);
            }
        }
        
        if (isRotating)
        {
            UpdateRotation();
        }
        
        if (hasRotated)
        {
            UpdateFall();
        }
    }
    
    void StartRotation(float angleDelta)
    {
        // 禁用玩家控制
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);
        
        // 冻结玩家
        playerRb.velocity = Vector2.zero;
        playerRb.gravityScale = 0;
        playerRb.bodyType = RigidbodyType2D.Kinematic;
        
        // 更新方向索引（精确整数运算）
        if (angleDelta > 0)  // 顺时针
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % 4;
        }
        else  // 逆时针
        {
            currentDirectionIndex = (currentDirectionIndex - 1 + 4) % 4;
        }
        
        // 计算目标角度（精确的90°倍数）
        float targetAngle = currentDirectionIndex * 90f;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        
        Debug.Log($"当前方向索引: {currentDirectionIndex}, 目标角度: {targetAngle}");
        
        // 记录起始状态
        startRotation = worldRoot.rotation;
        startPosition = worldRoot.position;
        
        // 计算以相机为轴心的新位置
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
        
        rotationTimer = 0;
        isRotating = true;
    }
    
    void UpdateRotation()
    {
        rotationTimer += Time.deltaTime / rotationDuration;
        
        if (rotationTimer >= 1)
        {
            // 旋转完成，直接设置为精确的目标值
            worldRoot.rotation = targetRotation;
            worldRoot.position = targetPosition;
            isRotating = false;
            
            // 进入等待落地阶段
            hasRotated = true;
            fallTimer = 0;
            playerControlRestored = false;
            
            // 恢复物理
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.gravityScale = 1;
            
            Debug.Log($"旋转完成，最终角度: {worldRoot.eulerAngles.z}");
        }
        else
        {
            float t = rotationTimer;
            worldRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            worldRoot.position = Vector3.Lerp(startPosition, targetPosition, t);
        }
    }
    
    void UpdateFall()
    {
        fallTimer += Time.deltaTime;
        
        bool isGrounded = IsPlayerGrounded();
        
        if (isGrounded && !playerControlRestored && fallTimer > 0.1f)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetCanMove(true);
            playerControlRestored = true;
            hasRotated = false;
            Debug.Log("玩家已落地，恢复控制");
        }
        
        if (fallTimer > 3f && !playerControlRestored)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetCanMove(true);
            playerControlRestored = true;
            hasRotated = false;
            Debug.LogWarning("超时，强制恢复控制");
        }
    }
    
    bool IsPlayerGrounded()
    {
        if (player == null) return false;
        
        float rayDistance = 1.2f;
        RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.down, rayDistance, groundLayer);
        Debug.DrawRay(player.position, Vector2.down * rayDistance, hit.collider != null ? Color.green : Color.red);
        
        return hit.collider != null;
    }
}  