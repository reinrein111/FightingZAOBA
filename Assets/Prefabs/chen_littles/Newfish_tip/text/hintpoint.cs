using UnityEngine;
using TMPro;
using System.Collections;

public class HintPoint_Distance : MonoBehaviour
{
    [Header("引用")]
    private Transform player1; // 改为私有，Start里自动找
    private Transform player2; // 新增玩家2
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;
    public GameObject continueHint;

    [Header("触发设置")]
    public float triggerDistance = 5f;

    [Header("提示内容")]
    public string[] hintMessages;

    private bool hasTriggered = false;

    void Start()
    {
        // 自动寻找场景中的两个玩家
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;

        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        if (hintPanel != null) hintPanel.SetActive(false);
    }

    void Update()
    {
        if (hasTriggered) return;

        // 检查两个玩家中是否有任意一个进入范围
        bool p1InRange = player1 != null && Vector2.Distance(player1.position, transform.position) < triggerDistance;
        bool p2InRange = player2 != null && Vector2.Distance(player2.position, transform.position) < triggerDistance;

        if (p1InRange || p2InRange)
        {
            TriggerHint();
        }
    }

    void TriggerHint()
    {
        hasTriggered = true;
        hintPanel.SetActive(true);
        
        // 暂停游戏逻辑
        Time.timeScale = 0f;

        StartCoroutine(ShowHint());
    }

    IEnumerator ShowHint()
    {
        foreach (var msg in hintMessages)
        {
            hintText.text = msg;
            continueHint.SetActive(true);

            // 在 Time.timeScale = 0 时，必须使用 WaitUntil 配合 Input
            // 注意：这里需要跳过当前帧的按键，防止触发瞬间就跳过第一句
            yield return null; 

            // 等待玩家按下任意键
            // 使用 Input.anyKeyDown 在暂停状态下依然有效
            yield return new WaitUntil(() => Input.anyKeyDown);

            // 防止按得太快瞬间跳过所有对话
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // 结束提示，恢复游戏
        hintText.text = "";
        if (continueHint != null) continueHint.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        
        Time.timeScale = 1f;
        
        // 既然已经触发过了，这个脚本的任务就完成了，可以直接销毁或禁用
        // gameObject.SetActive(false); 
    }

    // 在编辑器里画出触发范围，方便你调试（黄色圈）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}