using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpikeTrigger : MonoBehaviour 
{
    [Header("References")]
    public Camera mainCamera;
    public CanvasGroup redOverlay; 

    [Header("Settings")]
    public float shakeDuration = 1f;
    public float shakeStrength = 0.4f;
    public float fadeDuration = 1f;
    public float resetDelay = 0.5f;

    private PlayerController player1;
    private PlayerController player2;
    private PlayerController triggeredPlayer;
    private bool hasTriggered = false;

    void Start() 
    {
        // 自动获取相机
        if (mainCamera == null) mainCamera = Camera.main;

        // 自动获取红色遮罩（需确保 UI 图片有 Overlay 标签）
        if (redOverlay == null)
        {
            GameObject overlayObj = GameObject.FindWithTag("Overlay");
            if (overlayObj != null) redOverlay = overlayObj.GetComponent<CanvasGroup>();
        }
        
        if (redOverlay != null) redOverlay.alpha = 0f;

        // 自动获取玩家
        GameObject playerObj1 = GameObject.Find("Player1");
        GameObject playerObj2 = GameObject.Find("Player2");
        
        if (playerObj1 != null) player1 = playerObj1.GetComponent<PlayerController>();
        if (playerObj2 != null) player2 = playerObj2.GetComponent<PlayerController>();
    }

    public void ExecuteDeath(PlayerController player)
    {
        if (hasTriggered) return;

        if (player != player1 && player != player2) return;

        triggeredPlayer = player;
        hasTriggered = true;
        StartCoroutine(TriggerSequence());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && (player == player1 || player == player2))
        {
            ExecuteDeath(player);
        }
    }

    IEnumerator TriggerSequence()
    {
        if (triggeredPlayer != null) triggeredPlayer.gameObject.SetActive(false);

        // 启动效果
        StartCoroutine(ShakeCamera());
        yield return StartCoroutine(FadeRedOverlay());

        yield return new WaitForSeconds(resetDelay);
        ResetGame();
    }

    IEnumerator ShakeCamera() 
    {
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration) 
        {
            float x = Random.Range(-1f, 1f) * shakeStrength;
            float y = Random.Range(-1f, 1f) * shakeStrength;
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    IEnumerator FadeRedOverlay() 
    {
        if (redOverlay != null) 
        {
            redOverlay.alpha = 1f;
            float fadeOutElapsed = 0f;
            while (fadeOutElapsed < fadeDuration) 
            {
                redOverlay.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
                fadeOutElapsed += Time.deltaTime;
                yield return null;
            }
            redOverlay.alpha = 0f;
        }
    }

    void ResetGame() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}