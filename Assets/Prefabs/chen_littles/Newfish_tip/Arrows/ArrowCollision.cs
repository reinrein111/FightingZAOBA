using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    public Transform player;  // 玩家物体
    private bool playerNearby = false;  // 玩家是否在触发范围内

    void Update()
    {
        // 检测玩家是否靠近，如果靠近则消失
        if (playerNearby)
        {
            // 隐藏箭头
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果玩家进入触发区域
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果玩家离开触发区域
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}