using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    [Header("关联目标")]
    public ChangeablePlatform targetPlatform;

    [Header("动画（可选）")]
    public Animator animator;
    public string animatorBoolName = "IsOn";

    [Header("光晕（可选）")]
    public bool enableHalo = true;
    public Color haloColor = new Color(1f, 0.9f, 0.2f, 1f);
    [Range(0f, 1f)] public float haloAlphaOff = 0.15f;
    [Range(0f, 1f)] public float haloAlphaOn = 0.6f;
    [Min(0.01f)] public float haloFadeSpeed = 6f;
    [Range(32, 512)] public int haloTextureSize = 128;
    public int haloSortingOrderOffset = -1;

    [Header("探测设置")]
    public float detectRange = 2f;
    public Color activeColor = Color.red;

    private Transform player1;
    private Transform player2;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool lastIsOn;
    private SpriteRenderer haloRenderer;
    private float haloCurrentAlpha;
    private float lastDetectRange;

    void Start()
    {
        GameObject p1 = GameObject.Find("Player1");
        GameObject p2 = GameObject.Find("Player2");
        if (p1 != null) player1 = p1.transform;
        if (p2 != null) player2 = p2.transform;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        if (targetPlatform == null)
        {
            Debug.LogError($"物体 {gameObject.name} 未关联目标平台！");
        }
        lastIsOn = false;
        if (animator == null) animator = GetComponent<Animator>();

        if (enableHalo)
        {
            EnsureHalo();
            UpdateHaloScale();
        }
    }

    void Update()
    {
        if (player1 == null)
        {
            GameObject p = GameObject.Find("Player1");
            if (p != null) player1 = p.transform;
        }

        if (player2 == null)
        {
            GameObject p = GameObject.Find("Player2");
            if (p != null) player2 = p.transform;
        }

        if (player1 == null && player2 == null) return;

        bool isOn = false;

        if (player1 != null)
        {
            float dist1 = Vector2.Distance(transform.position, player1.position);
            if (dist1 < detectRange) isOn = true;
        }

        if (player2 != null)
        {
            float dist2 = Vector2.Distance(transform.position, player2.position);
            if (dist2 < detectRange) isOn = true;
        }

        if (targetPlatform != null) targetPlatform.SetPlatformState(isOn);

        if (sr != null) sr.color = isOn ? activeColor : originalColor;

        if (animator != null && !string.IsNullOrEmpty(animatorBoolName) && isOn != lastIsOn)
        {
            animator.SetBool(animatorBoolName, isOn);
            lastIsOn = isOn;
        }

        if (enableHalo)
        {
            EnsureHalo();
            if (!Mathf.Approximately(detectRange, lastDetectRange)) UpdateHaloScale();

            float targetAlpha = isOn ? haloAlphaOn : haloAlphaOff;
            haloCurrentAlpha = Mathf.MoveTowards(haloCurrentAlpha, targetAlpha, haloFadeSpeed * Time.deltaTime);
            if (haloRenderer != null)
            {
                Color c = haloColor;
                c.a = haloCurrentAlpha;
                haloRenderer.color = c;
            }
        }
    }

    private void EnsureHalo()
    {
        if (haloRenderer != null) return;

        Transform existing = transform.Find("DetectRangeHalo");
        GameObject haloObject = existing != null ? existing.gameObject : new GameObject("DetectRangeHalo");
        haloObject.transform.SetParent(transform, false);
        haloObject.transform.localPosition = Vector3.zero;
        haloObject.transform.localRotation = Quaternion.identity;

        haloRenderer = haloObject.GetComponent<SpriteRenderer>();
        if (haloRenderer == null) haloRenderer = haloObject.AddComponent<SpriteRenderer>();

        if (sr != null)
        {
            haloRenderer.sortingLayerID = sr.sortingLayerID;
            haloRenderer.sortingOrder = sr.sortingOrder + haloSortingOrderOffset;
        }

        haloRenderer.sprite = CreateRadialGradientSprite(haloTextureSize);
        haloCurrentAlpha = haloAlphaOff;
        Color c = haloColor;
        c.a = haloCurrentAlpha;
        haloRenderer.color = c;
    }

    private void UpdateHaloScale()
    {
        if (haloRenderer == null) return;

        float diameter = Mathf.Max(0.01f, detectRange * 2f);
        haloRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
        lastDetectRange = detectRange;
    }

    private Sprite CreateRadialGradientSprite(int size)
    {
        size = Mathf.Clamp(size, 32, 512);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float maxDist = center;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist01 = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / maxDist);
                float a = 1f - dist01;
                a = a * a;
                pixels[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, false);

        float pixelsPerUnit = size;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
