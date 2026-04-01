using UnityEngine;

public class TeacherController : MonoBehaviour
{
    [Header("引用")]
    public GameObject bulletPrefab;      // 关联子弹的 Prefab
    public Transform firePoint;         // 关联发射点 Fire_Point
    public SpriteRenderer visual_sprite; // 用于获取图片边界和镜像翻转

    [Header("射程与频率")]
    public float attackRange = 5f;      // 触发攻击（状态2）的距离 r
    public float escapeRange = 8f;      // 停止观察并坐下（状态0）的距离 R
    public float fireRate = 1.5f;       // 发射间隔

    [Header("动画状态输出 (队友使用)")]
    [Tooltip("0: 坐着批改, 1: 站立观察, 2: 扔粉笔攻击")]
    public int teacherState = 0; 

    private Transform player;
    private float nextFireTime;
    private bool isTracking = false;    // 是否处于攻击锁定状态

    void Start()
    {
        // 自动寻找场景中带 Player 标签的玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 安全检查：如果 Inspector 没拖，尝试自动获取
        if (visual_sprite == null)
            visual_sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || visual_sprite == null) return;

        // --- 1. 近身死亡判定 (优先级最高) ---
        Bounds teacherBounds = visual_sprite.bounds;
        Vector3 playerCheckPos = new Vector3(player.position.x, player.position.y, teacherBounds.center.z);

        if (teacherBounds.Contains(playerCheckPos))
        {
            ExecuteDeathLogic();
            return; 
        }

        // --- 2. 距离计算与状态切换 ---
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > escapeRange)
        {
            // 状态 0：玩家跑远了，老师坐下批改作业
            teacherState = 0;
            isTracking = false;
        }
        else if (distance <= escapeRange && distance > attackRange)
        {
            // 状态 1：玩家进入警戒区，老师站起来盯着看
            teacherState = 1;
            isTracking = false;
            LookAtPlayer(); // 盯着玩家看
        }
        else if (distance <= attackRange)
        {
            // 状态 2：玩家进入攻击区，老师开始扔粉笔
            teacherState = 2;
            isTracking = true;
            LookAtPlayer();
        }

        // --- 3. 执行攻击行为 ---
        if (isTracking && teacherState == 2)
        {
            if (Time.time >= nextFireTime)
            {
                ThrowChalk();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void LookAtPlayer()
    {
        if (visual_sprite == null || player == null) return;
        // 根据玩家在左还是在右进行镜像翻转
        visual_sprite.flipX = (player.position.x > transform.position.x);
    }

    void ThrowChalk()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 生成子弹实例
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet script = b.GetComponent<Bullet>();
        
        if (script != null)
        {
            Vector2 dir = (Vector2)player.position - (Vector2)firePoint.position;
            script.Launch(dir);
        }
    }

    private void ExecuteDeathLogic()
    {
        PlayerShield shield = player.GetComponent<PlayerShield>();

        if (shield != null && shield.TryUseShield())
        {
            Debug.Log("老师抓到了玩家，但被包子护盾挡住了！");
            return; 
        }

        SpikeTrigger st = Object.FindAnyObjectByType<SpikeTrigger>();
        if (st != null)
        {
            st.ExecuteDeath();
        }
    }

    // 可视化调试：在 Scene 窗口显示判定区域
    private void OnDrawGizmos()
    {
        if (visual_sprite != null)
        {
            // 白色方框：近身即死区域
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(visual_sprite.bounds.center, visual_sprite.bounds.size);

            // 红色圆圈：攻击范围 (r)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 黄色圆圈：警戒/逃离范围 (R)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, escapeRange);
        }
    }
}