using UnityEngine;

public class ArrowMoveUpDown : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startLocalPosition; // 使用局部初始位置

    void Start()
    {
        // 记录相对于父物体的初始位置
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float moveOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        
        // 在局部空间中进行偏移
        transform.localPosition = startLocalPosition + new Vector3(0f, moveOffset, 0f);
    }
}