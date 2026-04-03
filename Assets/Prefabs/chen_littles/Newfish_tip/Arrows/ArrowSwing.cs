using UnityEngine;

public class ArrowSwing : MonoBehaviour
{
    public float swingAmount = 10f;
    public float swingSpeed = 2f;

    private Quaternion startLocalRotation; // 使用局部初始旋转

    void Start()
    {
        // 记录相对于父物体的初始旋转
        startLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float swing = Mathf.Sin(Time.time * swingSpeed) * swingAmount;
        
        // 将摆动叠加到初始的局部旋转上
        transform.localRotation = startLocalRotation * Quaternion.Euler(0, 0, swing);
    }
}