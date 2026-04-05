// using UnityEngine;

// public class TeacherController : MonoBehaviour
// {
//     [Header("引用")]
//     public GameObject bulletPrefab;
//     public Transform firePoint;
//     public SpriteRenderer visual_sprite;

//     [Header("射程与频率")]
//     public float attackRange = 5f;  // 扔粉笔的射程
//     public float escapeRange = 8f;  // 观察/坐下的范围
//     // 注意：有了动画事件，fireRate 将由动画播放速度和间隔决定

//     [Header("动画状态输出")]
//     public int teacherState = 0;

//     private Transform player1;
//     private Transform player2;
//     private Transform nearestPlayer;
//     private bool isTracking = false;

//     private Animator anim;
//     private int lastState = -1;

//     void Start()
//     {
//         GameObject p1 = GameObject.Find("Player1");
//         if (p1 != null) player1 = p1.transform;
//         GameObject p2 = GameObject.Find("Player2");
//         if (p2 != null) player2 = p2.transform;

//         if (visual_sprite == null)
//             visual_sprite = GetComponentInChildren<SpriteRenderer>();

//         if (visual_sprite != null)
//             anim = visual_sprite.GetComponent<Animator>();
//     }

//     void Update()
//     {
//         if (player1 == null && player2 == null) return;

//         FindNearestPlayer();
//         if (nearestPlayer == null || visual_sprite == null) return;

//         // --- 1. 近身即死判定 ---
//         Bounds teacherBounds = visual_sprite.bounds;
//         Vector3 playerPos = new Vector3(nearestPlayer.position.x, nearestPlayer.position.y, teacherBounds.center.z);

//         if (teacherBounds.Contains(playerPos))
//         {
//             ExecuteDeathLogic(nearestPlayer);
//             return; 
//         }

//         // --- 2. 距离计算与状态切换 ---
//         float distance = Vector2.Distance(transform.position, nearestPlayer.position);

//         if (distance > escapeRange)
//         {
//             teacherState = 0;
//             isTracking = false;
//         }
//         else if (distance <= escapeRange && distance > attackRange)
//         {
//             teacherState = 1;
//             isTracking = false;
//             LookAtPlayer();
//         }
//         else if (distance <= attackRange)
//         {
//             teacherState = 2;
//             isTracking = true;
//             LookAtPlayer();
//         }

//         // --- 3. 动画处理 ---
//         if (anim != null)
//         {
//             // 更新基础状态 (0:坐, 1:站)
//             if (teacherState != lastState)
//             {
//                 anim.SetInteger("Teacher State", teacherState);
//                 lastState = teacherState;
//             }

//             // 如果处于攻击范围，且当前处于状态2，通过 Trigger 触发攻击动画
//             // 动画播放时，我们在动画中设置的 Event 会自动调用 ThrowChalk
//             if (isTracking && teacherState == 2)
//             {
//                 // 这里可以加一个简单的计时器，或者让动画播完自动回切
//                 // 如果你的动画是循环的，建议使用 Trigger
//                 anim.SetTrigger("Throw"); 
//             }
//         }
//     }

//     private void FindNearestPlayer()
//     {
//         float dist1 = (player1 != null) ? Vector2.Distance(transform.position, player1.position) : float.MaxValue;
//         float dist2 = (player2 != null) ? Vector2.Distance(transform.position, player2.position) : float.MaxValue;
//         nearestPlayer = (dist1 < dist2) ? player1 : player2;
//     }

//     void LookAtPlayer()
//     {
//         if (visual_sprite == null || nearestPlayer == null) return;
//         visual_sprite.flipX = (nearestPlayer.position.x > transform.position.x);
//     }

//     // --- 核心变化：这个函数现在由 Animation Event 调用 ---
//     // 必须保持为 public 才能被动画事件检测到
//     public void ThrowChalk()
//     {
//         if (bulletPrefab == null || firePoint == null || nearestPlayer == null) return;

//         // 实例化粉笔
//         GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
//         Bullet script = b.GetComponent<Bullet>();

//         if (script != null)
//         {
//             Vector2 dir = (Vector2)nearestPlayer.position - (Vector2)firePoint.position;
//             script.Launch(dir);
//         }
        
//         Debug.Log("动画触发了 ThrowChalk！粉笔已发射。");
//     }

//     private void ExecuteDeathLogic(Transform targetPlayer)
//     {
//         PlayerShield shield = targetPlayer.GetComponent<PlayerShield>();
//         if (shield != null && shield.TryUseShield()) return;

//         SpikeTrigger st = Object.FindAnyObjectByType<SpikeTrigger>();
//         if (st != null)
//         {
//             st.ExecuteDeath(targetPlayer.GetComponent<PlayerController>());
//         }
//     }
// }

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
    // 注意：有了动画事件，fireRate 将由动画播放速度和间隔决定

    [Header("动画状态输出")]
    public int teacherState = 0;

    private Transform player1;
    private Transform player2;
    private Transform nearestPlayer;
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

        // --- 1. 近身即死判定 ---
        Bounds teacherBounds = visual_sprite.bounds;
        Vector3 playerPos = new Vector3(nearestPlayer.position.x, nearestPlayer.position.y, teacherBounds.center.z);

        if (teacherBounds.Contains(playerPos))
        {
            ExecuteDeathLogic(nearestPlayer);
            return; 
        }

        // --- 2. 距离计算与状态切换 ---
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
        else if (distance <= attackRange)
        {
            teacherState = 2;
            isTracking = true;
            LookAtPlayer();
        }

        // --- 3. 动画处理 ---
        if (anim != null)
        {
            // 更新基础状态 (0:坐, 1:站)
            if (teacherState != lastState)
            {
                anim.SetInteger("Teacher State", teacherState);
                lastState = teacherState;
            }

            // 如果处于攻击范围，且当前处于状态2，通过 Trigger 触发攻击动画
            // 动画播放时，我们在动画中设置的 Event 会自动调用 ThrowChalk
            if (isTracking && teacherState == 2)
            {
                // 这里可以加一个简单的计时器，或者让动画播完自动回切
                // 如果你的动画是循环的，建议使用 Trigger
                anim.SetTrigger("Throw"); 
            }
        }
    }

    private void FindNearestPlayer()
    {
        float dist1 = (player1 != null) ? Vector2.Distance(transform.position, player1.position) : float.MaxValue;
        float dist2 = (player2 != null) ? Vector2.Distance(transform.position, player2.position) : float.MaxValue;
        nearestPlayer = (dist1 < dist2) ? player1 : player2;
    }

    void LookAtPlayer()
    {
        if (visual_sprite == null || nearestPlayer == null) return;
        visual_sprite.flipX = (nearestPlayer.position.x > transform.position.x);
    }

    // --- 核心变化：这个函数现在由 Animation Event 调用 ---
    // 必须保持为 public 才能被动画事件检测到
    public void ThrowChalk()
    {
        if (bulletPrefab == null || firePoint == null || nearestPlayer == null) return;

        // 实例化粉笔
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet script = b.GetComponent<Bullet>();

        if (script != null)
        {
            Vector2 dir = (Vector2)nearestPlayer.position - (Vector2)firePoint.position;
            script.Launch(dir);
        }
        
        Debug.Log("动画触发了 ThrowChalk！粉笔已发射。");
    }

    private void ExecuteDeathLogic(Transform targetPlayer)
    {
        PlayerShield shield = targetPlayer.GetComponent<PlayerShield>();
        if (shield != null && shield.TryUseShield()) return;

        SpikeTrigger st = Object.FindAnyObjectByType<SpikeTrigger>();
        if (st != null)
        {
            st.ExecuteDeath(targetPlayer.GetComponent<PlayerController>());
        }
    }
}