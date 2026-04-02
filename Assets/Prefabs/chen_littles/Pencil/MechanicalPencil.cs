using UnityEngine;

public class MechanicalPencil : MonoBehaviour
{
    [Header("组件引用")]
    public Transform leadTransform;     // 这里的槽位请拖入刚才新建的 Tip_Point 子物体
    public BoxCollider2D bodyCollider;  // 笔杆碰撞体

    [Header("笔芯设置")]
    public float stepDistance = 0.5f;   // 踩一次伸出多少
    public float maxLeadLength = 3f;  // 最大长度
    private float currentLength = 0f;
    private Vector3 initialLeadPos;     // 记录笔芯（整体）的初始位置
    private Transform leadParent;       // 笔芯的父物体引用

    [Header("致死判定设置")]
    public float deathWidth = 0.15f;    // 笔尖危险区的宽度
    public float deathHeight = 0.1f;    // 笔尖危险区的高度
    public LayerMask playerLayer;       // 在 Inspector 里勾选 Player 层

    private void Start()
    {
        // 自动获取笔芯的长条物体（Tip_Point 的父物体）
        if (leadTransform != null)
        {
            leadParent = leadTransform.parent;
            initialLeadPos = leadParent.localPosition;
        }
        
        UpdateLeadVisual();
    }

void Update()
    {
        if (leadTransform == null) return;

        // 逻辑 1：姿态判定
        float zRot = transform.eulerAngles.z;
        bool isVertical = (Mathf.Abs(zRot - 0) < 10 || Mathf.Abs(zRot - 180) < 10);

        // 逻辑 2：横着时，直接禁用笔尖的死亡触发器
        SharpTip tipScript = leadTransform.GetComponent<SharpTip>();
        if (tipScript != null)
        {
            // 只有笔芯伸出了且铅笔是竖着的，才激活危险
            tipScript.enabled = (isVertical && currentLength > 0.1f);
        }
    }

void CheckDeathCollision()
{
    // 增加一个 Debug，让你知道检测确实在运行
    // Debug.Log("正在检测笔尖死亡...");

    // 1. 扩大一点判定范围（宽0.3, 高0.2）
    Collider2D hit = Physics2D.OverlapBox(leadTransform.position, new Vector2(0.3f, 0.2f), 0f, playerLayer);

    if (hit != null)
    {
        // 2. 暂时移除速度判定，只要碰到红框就死，用来测试
        ExecuteDeath(hit.gameObject);
    }
}
    public void OnCapPressed()
    {
        if (currentLength < maxLeadLength)
        {
            currentLength += stepDistance;
            UpdateLeadVisual();
        }
    }

    void UpdateLeadVisual()
    {
        if (leadParent == null) return;

        // 移动笔芯整体，Tip_Point 作为子物体会跟着一起动
        leadParent.localPosition = initialLeadPos + new Vector3(0, -currentLength, 0);
        
        // 笔芯侧面的碰撞体：只有伸出后才激活，允许玩家作为平台踩踏
        BoxCollider2D leadCol = leadParent.GetComponent<BoxCollider2D>();
        if (leadCol != null)
        {
            leadCol.enabled = (currentLength > 0.3f);
        }
    }

    void ExecuteDeath(GameObject playerObj)
    {
        Debug.Log("<color=red>玩家死于笔尖刺杀！</color>");
        // 触发你现有的死亡逻辑
        Object.FindAnyObjectByType<SpikeTrigger>()?.ExecuteDeath(playerObj.GetComponent<PlayerController>());
    }

    // 在 Scene 窗口画出红色的危险方块，方便你调整大小
    private void OnDrawGizmos()
    {
        if (leadTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(leadTransform.position, new Vector3(deathWidth, deathHeight, 0));
        }
    }
}