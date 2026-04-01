using UnityEngine;
using UnityEngine.SceneManagement; // 用于切换关卡

public class SchoolGate : MonoBehaviour
{
    public float openDistance = 1.5f; // 感应距离
    public string nextSceneName = "WinScene"; // 胜利后的场景名

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < openDistance)
        {
            PlayerInventory inv = player.GetComponent<PlayerInventory>();
            
            if (inv != null && inv.hasCampusCard)
            {
                Debug.Log("<color=green>校园卡验证通过！门已打开。</color>");
                WinGame();
            }
            else
            {
                // 如果没卡，可以这里加个提示，比如“请先寻找校园卡”
                Debug.Log("门锁着，你需要校园卡才能进入。");
            }
        }
    }

    void WinGame()
    {
        // 这里执行胜利逻辑，比如加载下一关或者弹出结算界面
        // SceneManager.LoadScene(nextSceneName); 
        Debug.Log("恭喜通关！");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(openDistance, openDistance, 0));
    }
}