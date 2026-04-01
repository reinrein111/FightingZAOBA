using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    [Header("关联目标")]
    public ChangeablePlatform targetPlatform; // 拖入受控的平台物体

    [Header("探测设置")]
    public float detectRange = 3f; // 探测半径，建议设为 3 左右
    public Color activeColor = Color.red; // 激活时的按钮颜色
    
    private Transform player;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        // 自动寻找玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // 初始化检查
        if (targetPlatform == null)
        {
            Debug.LogError($"物体 {gameObject.name} 未关联目标平台！");
        }
    }

void Update()
{
    if (player == null || targetPlatform == null) return;

    float dist = Vector2.Distance(transform.position, player.position);

    if (dist < detectRange)
    {
        // 只有进入范围才会触发
        targetPlatform.SetPlatformState(true);
        if (sr != null) sr.color = activeColor;
        // Debug.Log("玩家进入范围，触发平台！"); // 调试用
    }
    else
    {
        // 离开范围恢复虚体
        targetPlatform.SetPlatformState(false);
        if (sr != null) sr.color = originalColor;
    }
}

    // 在编辑器窗口画出红色圆圈，方便调试距离
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}