using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;        // 跟随的目标（Player）
    
    [Header("偏移量")]
    public Vector3 offset = new Vector3(0, 0, -10);  // 相机相对于玩家的偏移
    
    [Header("跟随平滑度")]
    public float smoothSpeed = 5f;   // 跟随速度，值越大越灵敏
    
    void Start()
    {
        // 自动查找Player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("相机自动找到Player: " + target.name);
            }
            else
            {
                Debug.LogError("未找到Player！请给Player添加Tag 'Player'");
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 目标位置 = 玩家位置 + 偏移量
        Vector3 targetPosition = target.position + offset;
        
        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}