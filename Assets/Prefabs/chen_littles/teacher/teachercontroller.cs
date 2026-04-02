using UnityEngine;

public class TeacherController : MonoBehaviour
{
    [Header("引用")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public SpriteRenderer visual_sprite;

    [Header("射程与频率")]
    public float attackRange = 5f;  // 扔粉笔的射程
    public float escapeRange = 8f;  // 观察/坐下的范围
    public float fireRate = 1.5f;

    [Header("动画状态输出")]
    public int teacherState = 0;

    private Transform player1;
    private Transform player2;
    private Transform nearestPlayer;
    private float nextFireTime;
    private bool isTracking = false;

    private Animator anim;
    private int lastState = -1;

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        if (visual_sprite == null)
            visual_sprite = GetComponentInChildren<SpriteRenderer>();

        if (visual_sprite != null)
            anim = visual_sprite.GetComponent<Animator>();
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;

        FindNearestPlayer();
        if (nearestPlayer == null || visual_sprite == null) return;

        // --- 核心修复 1：近身即死判定 (必须使用 Bounds，不能用 attackRange) ---
        Bounds teacherBounds = visual_sprite.bounds;
        Vector3 playerPos = new Vector3(nearestPlayer.position.x, nearestPlayer.position.y, teacherBounds.center.z);

        if (teacherBounds.Contains(playerPos))
        {
            ExecuteDeathLogic(nearestPlayer); // 只有贴身才直接抓死
            return; 
        }

        // --- 核心修复 2：逻辑重组 ---
        float distance = Vector2.Distance(transform.position, nearestPlayer.position);

        if (distance > escapeRange)
        {
            teacherState = 0;
            isTracking = false;
        }
        else if (distance <= escapeRange && distance > attackRange)
        {
            teacherState = 1;
            isTracking = false;
            LookAtPlayer();
        }
        else if (distance <= attackRange) // 玩家在 5 米内，老师扔粉笔，而不是直接抓死
        {
            teacherState = 2;
            isTracking = true;
            LookAtPlayer();
        }

        // 状态更新给 Animator
        if (anim != null && teacherState != lastState)
        {
            anim.SetInteger("Teacher State", teacherState);
            lastState = teacherState;
        }

        // 只有状态 2 才扔粉笔
        if (isTracking && teacherState == 2)
        {
            if (Time.time >= nextFireTime)
            {
                ThrowChalk();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void FindNearestPlayer()
    {
        // 增加存活判定：如果玩家被销毁了就不计入
        float dist1 = (player1 != null) ? Vector2.Distance(transform.position, player1.position) : float.MaxValue;
        float dist2 = (player2 != null) ? Vector2.Distance(transform.position, player2.position) : float.MaxValue;

        nearestPlayer = (dist1 < dist2) ? player1 : player2;
    }

    void LookAtPlayer()
    {
        if (visual_sprite == null || nearestPlayer == null) return;
        visual_sprite.flipX = (nearestPlayer.position.x > transform.position.x);
    }

    void ThrowChalk()
    {
        if (bulletPrefab == null || firePoint == null || nearestPlayer == null) return;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet script = b.GetComponent<Bullet>();

        if (script != null)
        {
            Vector2 dir = (Vector2)nearestPlayer.position - (Vector2)firePoint.position;
            script.Launch(dir);
        }
        if (anim != null)
    {
        anim.SetTrigger("Throw"); 
    }
    }

    private void ExecuteDeathLogic(Transform targetPlayer)
    {
        PlayerShield shield = targetPlayer.GetComponent<PlayerShield>();
        if (shield != null && shield.TryUseShield()) return;

        SpikeTrigger st = Object.FindAnyObjectByType<SpikeTrigger>();
        if (st != null)
        {
<<<<<<< Updated upstream
            st.ExecuteDeath(player.GetComponent<PlayerController>());
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
=======
            st.ExecuteDeath(targetPlayer.GetComponent<PlayerController>());
>>>>>>> Stashed changes
        }
    }
}