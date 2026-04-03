using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    [Header("关联目标")]
    public ChangeablePlatform targetPlatform; // 拖入受控的平台物体

    [Header("探测设置")]
    public float detectRange = 3f; // 探测半径，建议设为 3 左右
    public Color activeColor = Color.red; // 激活时的按钮颜色

    private Transform player1;
    private Transform player2;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        if (targetPlatform == null)
        {
            Debug.LogError($"物体 {gameObject.name} 未关联目标平台！");
        }
    }

void Update()
{
    if (targetPlatform == null) return;

    bool player1InRange = player1 != null && Vector2.Distance(transform.position, player1.position) < detectRange;
    bool player2InRange = player2 != null && Vector2.Distance(transform.position, player2.position) < detectRange;

    if (player1InRange || player2InRange)
    {
        targetPlatform.SetPlatformState(true);
        if (sr != null) sr.color = activeColor;
        Debug.Log("玩家进入范围，触发平台！");
    }
    else
    {
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