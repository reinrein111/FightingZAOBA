using UnityEngine;

public class MechanicalPencil : MonoBehaviour
{
    [Header("组件引用")]
    public Transform leadTransform;
    public BoxCollider2D bodyCollider;

    [Header("笔芯设置")]
    public float stepDistance = 0.5f;
    public float maxLeadLength = 3f;
    private float currentLength = 0f;
    private Vector3 initialLeadPos;
    private Transform leadParent;

    private void Start()
    {
        if (leadTransform != null)
        {
            leadParent = leadTransform;
            initialLeadPos = leadParent.localPosition;
        }
        
        UpdateLeadVisual();
    }

    void Update()
    {
        if (leadTransform == null) return;

        float zRot = transform.eulerAngles.z;
        bool isVertical = (Mathf.Abs(zRot - 0) < 10 || Mathf.Abs(zRot - 180) < 10);
    }

    public void OnCapPressed()
    {
        if (currentLength < maxLeadLength)
        {
            currentLength += stepDistance;
            UpdateLeadVisual();
        }
    }

    void UpdateLeadVisual()
    {
        Debug.Log($"currentLength: {currentLength}");

        if (leadParent == null) return;

        leadParent.localPosition = initialLeadPos + new Vector3(0, -currentLength, 0);
        
        BoxCollider2D leadCol = leadParent.GetComponent<BoxCollider2D>();
        if (leadCol != null)
        {
            leadCol.enabled = (currentLength > 0.3f);
        }
    }
}
