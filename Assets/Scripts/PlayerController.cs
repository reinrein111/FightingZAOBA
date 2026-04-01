/**
 * @file PlayerController.cs
 * @brief 彻底解决旋转过程中的碰撞问题！
 * @author ZHY
 * @version 2.1
 * @time 26-3-30 23-52 ~~ 26-3-30 0-
 */

/**
 * 核心理解：人物是一个有体积的方块，墙壁也是有体积的方块。在旋转过程中，我们不应该两者相撞！
 * 解决方案： 
 */
//using System.Diagnostics;
using UnityEngine;

/*
关于场景设计与Player的初始位置：
之后再细化
*/

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("地面检测")]
     // 脚底检测点别名；初始化为null，在start过程中被初始化
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
//--------各个Component的Reference定义如下--------//
    public Animator animator;
    public Transform groundCheck = null;
    private Rigidbody2D rb;
    [SerializeField]
    private BoxCollider2D playerCollider; // 这里直接定义成BoxCollider2D，若之后更换碰撞箱一定记得更改！！！
//--------各个Component的Reference定义如上--------//
    private float faceDirection = 0f;
    private float horizontalInput = 0f; // 这里是为了接入动画设置的新变量！
    private float jumpDelay = 0.2f;
    private float jumpTimer = 0f;

    public enum PlayerState
    {
        Grounding, // 利用地面碰撞修改 currentState 为此
        Jumping, // 利用 W / <Space> 键跳跃修改 currentState 为此
        Rotating, // 利用在背景中的旋转状态来修改 currentState 为此。可以调用 ChangeState 方法。
        Falling // 利用背景中的旋转状态是否结束和地面碰撞修改 currentState 为此。可以调用 ChangeState 方法。
    }
    
    public PlayerState currentState = PlayerState.Grounding;
    
    void Start()
    {
        initializeRigidBody(); // 3项常规初始化，若没有则添加，DEMO中早已手动添加，通常没用
        initializeBoxCollider2D();
        initializeGroundLayer();
        initializeAnimator();
        initializePosition(); // 先将Player和mainCamera的位置摆正.
        initializeGroundCheck(); // 将脚底检测点的位置摆正。
    }
    
    void Update()
    {   
        printCurrentState(); // Debug用，每时每刻报状态
        updateGroundCheckPosition(); // 不断更新脚部监测点的位置
        rb.angularVelocity = 0f; // 在人物行进过程中禁用旋转力矩

        //--->ZHY:何为着陆？脚底踩在地面上即为着陆！脚底探测点浸没在地面之内即为着陆！
        if (isPlayerGrounded()) // jumpTimer每次在用户按下W按键的时候会自动更新为当前时间
        {                                                               // 如果用户正在和Ground相接触，并且此时距离用户按下空格键已经过去0.2S
            changeState(PlayerState.Grounding);                         // 就将状态切换为着陆状态
        } 
        //--->ZHY:这里的实现逻辑有BUG:倘若用户跳起之后和墙壁接触，仍然会被判定成Ground状态！

        ReadInput();

        // 移动
        if(canMove()) // Grounding、Falling、Jumping 状态下都可以移动，Rotating状态不可移动
        {
            Move();
        }

        // 跳跃
        if(canJump())// Grounding状态下可以跳跃，Rotating、Falling、Jumping状态不可跳跃
        {
            Jump();
        }
        
        //失败判断
        FailCheck();
        UpdateAnimation();

    }
    
//--------以下为Start部分的初始化函数--------//
    private void initializeAnimator()
    {
        animator = GetComponentInChildren<Animator>();
    }
    private void initializePosition() // 一进入start函数，直接将Player和mainCamera的位置进行初始化
    {
        transform.localPosition = new Vector3(-4.37f, -7.76f, -6.91f); // 仅适用于DEMO演示中的关卡！！！
        Camera.main.transform.position = transform.localPosition;
//        Debug.Log("完成：初始化主人公和摄像机的位置");
    }

    private void initializeGroundCheck()
    {
        GameObject go = new GameObject("GroundCheck"); // 启动时，若Player下没有GroundCheck的Component则自动添加
        go.transform.SetParent(transform); // 并且，将GroundCheck设置为Player下的子Component
        go.transform.localPosition = settleGroundCheckPosition(); // 修正脚步检测点的位置坐标
        groundCheck = go.transform; // 最后修正groundCheck Reference为新建的GroundCheck
//        Debug.Log("完成：初始化脚部检测点");
    }

    private Vector3 settleGroundCheckPosition() // 时刻准备修正脚部检测点的位置坐标
    {
        // 获取 BoxCollider2D 的偏移（相对于角色中心点的局部坐标）
        Vector2 colliderOffset = playerCollider.offset;
        // 获取 BoxCollider2D 的尺寸
        Vector2 colliderSize = playerCollider.size;
        
        // 计算脚步检测点的局部坐标（角色中心点下方半个碰撞箱高度）
        float groundCheckY = colliderOffset.y - (colliderSize.y / 2f);
        
        // 返回局部坐标（X 不变，Y 是计算值，Z 保持默认）
        return new Vector3(0f, groundCheckY, 0f);
    }

    private void initializeRigidBody()
    {
        rb = GetComponent<Rigidbody2D>();
        // 启动时，若Player中没有Rigidbody2D的Component，则在代码中自动添加
        // 启动时将Player设置为Dynamic且有重力的实体
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1;
        }
        rb.freezeRotation = true;
    }

    private void initializeBoxCollider2D()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }

    private void initializeGroundLayer()
    {
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
//        Debug.Log("完成：初始化剩余常规项");
    }
//--------以上为Start部分的初始化函数--------//

//--------以下为Update部分的状态转移与判断函数--------//
    private void printCurrentState() // 查看状态报错用
    {
        if(currentState == PlayerState.Rotating)
            Debug.Log("Rotating");
        else if(currentState == PlayerState.Falling)
            Debug.Log("Falling");
        else if(currentState == PlayerState.Grounding)
            Debug.Log("Grounding");
        else if(currentState == PlayerState.Jumping)
            Debug.Log("Jumping");
    }
    private void ReadInput()
    {
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f; // A键向左
        if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;  // D键向右
    }

    private void updateGroundCheckPosition()
    {
        groundCheck.transform.localPosition = settleGroundCheckPosition();
    }

    public bool isPlayerGrounded() // 判断用户是否着陆，在相机旋转的状态转移中同样要使用！
    {
        float width = playerCollider.size.x; // 取碰撞箱的宽度

        Vector2 centerPos = groundCheck.position; // 左中右设置三个检测点
        Vector2 leftPoint = new Vector2(centerPos.x - width/3, centerPos.y - 0.2f);
        Vector2 rightPoint = new Vector2(centerPos.x + width/3, centerPos.y - 0.2f); 

        // 检测三个点:使用扎针法（小圆形覆盖）
        bool leftHit = Physics2D.OverlapCircle(leftPoint, 0.05f, groundLayer);
        bool centerHit = Physics2D.OverlapCircle(centerPos, 0.05f, groundLayer);
        bool rightHit = Physics2D.OverlapCircle(rightPoint, 0.05f, groundLayer);

        Debug.DrawRay(leftPoint, Vector2.down * 0.1f, leftHit ? Color.green : Color.red);
        Debug.DrawRay(centerPos, Vector2.down * 0.1f, centerHit ? Color.green : Color.red);
        Debug.DrawRay(rightPoint, Vector2.down * 0.1f, rightHit ? Color.green : Color.red);

        return (leftHit || centerHit || rightHit)&&(Time.time >= jumpTimer + jumpDelay); // 检测Player是否于位于groundLayer的任何object相接触
    }
    public void changeState(PlayerState state) // 状态修正函数 + 动画控制！
    {
        currentState = state;
        UpdateAnimationOnStateChange();
    }
    private bool canMove()
    {
        // Grounding、Falling、Jumping 状态下都可以移动
        return currentState != PlayerState.Rotating;
    }
    
    private bool canJump()
    {
        return currentState == PlayerState.Grounding;
    }
//--------以上为Update部分的状态转移与判断函数--------//
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 1. 更新移动速度（用于跑步/待机动画切换）
        float speed = Mathf.Abs(horizontalInput);
        animator.SetFloat("speed", speed);
        
        // 2. 更新朝向（用于左右动画切换）
        bool isRight = faceDirection > 0;
        animator.SetBool("isright", isRight);
        
        // 3. 确保状态标志正确（防止状态不同步）
        // 根据currentState强制设置标志
        /*switch (currentState)
        {
            case PlayerState.Grounding:
                if (!animator.GetBool("isGrounded"))
                {
                    animator.SetBool("isGrounded", true);
                    animator.SetBool("isJumping", false);
                    animator.SetBool("isFalling", false);
                }
                break;
                
            case PlayerState.Jumping:
                if (!animator.GetBool("isJumping"))
                {
                    animator.SetBool("isGrounded", false);
                    animator.SetBool("isJumping", true);
                    animator.SetBool("isFalling", false);
                }
                break;
                
            case PlayerState.Falling:
                if (!animator.GetBool("isFalling"))
                {
                    animator.SetBool("isGrounded", false);
                    animator.SetBool("isJumping", false);
                    animator.SetBool("isFalling", true);
                }
                break;
                
            case PlayerState.Rotating:
                animator.SetBool("isRotating", true);
                break;
        }*/
        
        // 4. 清理不需要的动画标志
        if (currentState != PlayerState.Rotating && animator.GetBool("isRotating"))
        {
            animator.SetBool("isRotating", false);
        }
    }
    private void UpdateAnimationOnStateChange()
    {
        if (animator == null) return;
        
        switch (currentState)
        {
            case PlayerState.Grounding:
                // 着陆：停止跳跃/下落动画
                animator.SetBool("isGrounded", true);
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
                break;
                
            case PlayerState.Jumping:
                // 跳跃：播放跳跃动画
                animator.SetBool("isGrounded", false);
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
                break;
                
            case PlayerState.Falling:
                // 下落：播放下落动画
                animator.SetBool("isGrounded", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
                break;
                
            case PlayerState.Rotating:
                // 旋转：播放旋转动画
                animator.SetBool("isRotating", true);
                break;
        }
    }

    // 移动
    // 添加墙壁检测方法
    private bool IsTouchingWall(int direction) // direction: -1左, 1右
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 origin = direction == -1 ? 
            new Vector2(bounds.min.x, bounds.center.y) : 
            new Vector2(bounds.max.x, bounds.center.y);
        
        RaycastHit2D hit = Physics2D.Raycast(origin, direction == -1 ? Vector2.left : Vector2.right, 0.15f, groundLayer);
//        Debug.Log($"{hit.collider != null}");
        Debug.DrawRay(origin, (direction == -1 ? Vector2.left : Vector2.right) * 0.15f, hit.collider != null ? Color.red : Color.green);
        return hit.collider != null;
    }

    private void Move()
    {
        float move = horizontalInput;
        
        // 空中移动时，如果贴墙则禁止继续向墙移动
        if (currentState != PlayerState.Grounding)
        {
            if (move < 0 && IsTouchingWall(-1))
            {
                move = 0;
            }
            else if (move > 0 && IsTouchingWall(1))
            {
                move = 0;
            }
        }
        
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        
        if (horizontalInput != 0f)
        {
            ChangeDirection(horizontalInput);
        }
    }

    private void ChangeDirection(float move)
    {
        if (move != 0f)
        {
            faceDirection = move;
            
            // 翻转角色方向
          Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(faceDirection);
            transform.localScale = scale;
        }
    }

    // 跳跃
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) | Input.GetKeyDown(KeyCode.W))
        {
            rb.AddForce(Vector2.up * jumpForce / 2f, ForceMode2D.Impulse);
            currentState = PlayerState.Jumping;
            jumpTimer = Time.time;
        }
    }

    private void JumpStateUntilLand() //修改人物动画为跳跃状态
    {
        // 切换到跳跃状态
        // 例如，播放跳跃动画
        // ...
    }

    private void FallStateUntilLand() //修改人物动画为落地状态
    {
        // 切换到落地状态
        // 例如，播放落地动画
        // ...
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // 失败判断
    private void FailCheck()
    {

    }

}