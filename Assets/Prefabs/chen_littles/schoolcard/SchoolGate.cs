using UnityEngine;
using System.Collections;

public class SchoolGate : MonoBehaviour
{
    public enum GateType { Girl, Boy }
    public GateType gateType = GateType.Girl;

    public float openDistance = 1.5f;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
    [Header("相机过渡设置")]
    public float transitionDuration = 1f;

=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    private Transform player1;
    private Transform player2;
    private bool hasTriggered = false;

    public Camera girlCamera;
    public Camera boyCamera;
<<<<<<< Updated upstream
<<<<<<< Updated upstream

    private float originalBoySize;
    private Rect originalBoyRect;
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null) player1 = p1.transform;
        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null) player2 = p2.transform;

        SetupCameras();
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
    }

    private void SetupCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
<<<<<<< Updated upstream
        {
            PlayerCameraController pcc = cam.GetComponent<PlayerCameraController>();
            if (pcc != null)
            {
                if (pcc.targetPlayerId == 1)
                    girlCamera = cam;
                else if (pcc.targetPlayerId == 2)
                    boyCamera = cam;
            }
        }

        if (boyCamera != null)
        {
            originalBoySize = boyCamera.orthographicSize;
            originalBoyRect = boyCamera.rect;
        }
=======
>>>>>>> Stashed changes
    }

    private void SetupCameras()
    {
<<<<<<< Updated upstream
        if (hasTriggered) return;

        if (gateType == GateType.Girl)
        {
            CheckGateAccess(player1, inv => inv.hasCard_Girl, "女孩卡片");
        }
        else
        {
            CheckGateAccess(player2, inv => inv.hasCard_Boy, "男孩卡片");
        }
    }

    private void CheckGateAccess(Transform targetPlayer, System.Func<PlayerInventory, bool> cardCheck, string cardName)
    {
        if (targetPlayer == null) return;

        float dist = Vector2.Distance(transform.position, targetPlayer.position);
        if (dist < openDistance)
        {
            PlayerInventory inv = targetPlayer.GetComponent<PlayerInventory>();

            if (inv != null && cardCheck(inv))
            {
                Debug.Log($"<color=green>{cardName}验证通过！{targetPlayer.name}可以通过{gameObject.name}。</color>");
                hasTriggered = true;
                targetPlayer.gameObject.SetActive(false);
                StartCoroutine(SmoothCameraTransition());
            }
            else
            {
                Debug.Log($"{cardName}门锁着，{targetPlayer.name}需要{cardName}才能进入。");
=======
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            PlayerCameraController pcc = cam.GetComponent<PlayerCameraController>();
            if (pcc != null)
            {
=======
        {
            PlayerCameraController pcc = cam.GetComponent<PlayerCameraController>();
            if (pcc != null)
            {
>>>>>>> Stashed changes
                if (pcc.targetPlayerId == 1)
                    girlCamera = cam;
                else if (pcc.targetPlayerId == 2)
                    boyCamera = cam;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            }
        }
    }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
    private IEnumerator SmoothCameraTransition()
    {
        if (gateType == GateType.Girl)
        {
            if (girlCamera != null)
            {
                girlCamera.gameObject.SetActive(false);
            }

            if (boyCamera != null)
            {
                float startSize = boyCamera.orthographicSize;
                Rect startRect = boyCamera.rect;
                Rect targetRect = new Rect(0, 0, 1, 1);
                float targetSize = originalBoySize;

                float elapsed = 0f;
                while (elapsed < transitionDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / transitionDuration;
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    boyCamera.rect = LerpRect(startRect, targetRect, smoothT);
                    boyCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
                    yield return null;
                }

                boyCamera.rect = targetRect;
                boyCamera.orthographicSize = targetSize;
            }
        }
        else
        {
            if (boyCamera != null)
            {
                boyCamera.gameObject.SetActive(false);
            }

            if (girlCamera != null)
            {
                float startSize = girlCamera.orthographicSize;
                Rect startRect = girlCamera.rect;
                Rect targetRect = new Rect(0, 0, 1, 1);
                float targetSize = originalBoySize;

                float elapsed = 0f;
                while (elapsed < transitionDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / transitionDuration;
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    girlCamera.rect = LerpRect(startRect, targetRect, smoothT);
                    girlCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
                    yield return null;
                }

                girlCamera.rect = targetRect;
                girlCamera.orthographicSize = targetSize;
            }
        }
    }

    private Rect LerpRect(Rect a, Rect b, float t)
    {
        return new Rect(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.width, b.width, t),
            Mathf.Lerp(a.height, b.height, t)
        );
=======
    void Update()
    {
=======
    void Update()
    {
>>>>>>> Stashed changes
        if (hasTriggered) return;

        if (gateType == GateType.Girl)
        {
            CheckGateAccess(player1, inv => inv.hasCard_Girl, "女孩卡片", 1);
        }
        else
        {
            CheckGateAccess(player2, inv => inv.hasCard_Boy, "男孩卡片", 2);
        }
    }

    private void CheckGateAccess(Transform targetPlayer, System.Func<PlayerInventory, bool> cardCheck, string cardName, int playerId)
    {
        if (targetPlayer == null) return;

        float dist = Vector2.Distance(transform.position, targetPlayer.position);
        if (dist < openDistance)
        {
            PlayerInventory inv = targetPlayer.GetComponent<PlayerInventory>();

            if (inv != null && cardCheck(inv))
            {
                Debug.Log($"<color=green>{cardName}验证通过！{targetPlayer.name}已通过{gameObject.name}。</color>");
                hasTriggered = true;

                targetPlayer.gameObject.SetActive(false);
                Debug.Log($"<color=orange>{targetPlayer.name}已消失，相机停留在当前位置</color>");

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.PlayerPassedGate(playerId);
                }
            }
            else
            {
                Debug.Log($"{cardName}门锁着，{targetPlayer.name}需要{cardName}才能进入。");
            }
        }
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gateType == GateType.Girl ? Color.magenta : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(openDistance, openDistance, 0));
    }
}
