using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float faceDirection = 0f;
    private float jumpDelay = 0.2f;
    private float jumpTimer = 0f;

    public enum PlayerState
    {
        Grounding, //利用地面碰撞修改 currentState 为此
        Jumping, //利用 W / <Space> 键跳跃修改 currentState 为此
        Rotating, //利用在背景中的旋转状态来修改 currentState 为此。可以调用 ChangeState 方法。
        Falling //利用背景中的旋转状态是否结束和地面碰撞修改 currentState 为此。可以调用 ChangeState 方法。
    }
    
    private PlayerState currentState = PlayerState.Grounding;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1;
        }
        
        if (playerCollider == null)
        {
            playerCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = go.transform;
        }
        
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }
    
    void Update()
    {        
        if(currentState == PlayerState.Rotating)
            Debug.Log("Rotating");
        else if(currentState == PlayerState.Falling)
            Debug.Log("Falling");
        else if(currentState == PlayerState.Grounding)
            Debug.Log("Grounding");
        else if(currentState == PlayerState.Jumping)
            Debug.Log("Jumping");

        if ((Time.time >= jumpTimer + jumpDelay) && IsPlayerGrounded())
        {
            ChangeState(PlayerState.Grounding);
        }

        // 移动
        if(canMove())
        {
            Move();
        }

        // 跳跃
        if(canJump())
        {
            Jump();
        }
        
        //失败判断
        FailCheck();

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

    // 检查是否落地（使用 Collider 检测）
    private bool IsPlayerGrounded()
    {
        if (playerCollider == null) return false;
        return playerCollider.IsTouchingLayers(groundLayer);
    }

    public void ChangeState(PlayerState state)
    {
        currentState = state;
    }

    // 移动
    private void Move()
    {
        float move = 0f;
        
        // A 键向左，D 键向右
        if (Input.GetKey(KeyCode.A))
        {
            move = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move = 1f;
        }
        
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        ChangeDirection(move);
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
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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