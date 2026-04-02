using UnityEngine;

public class Shake : MonoBehaviour
{
    [Header("晃动设置")]
    [Tooltip("逆时针旋转角度")]
    public float counterClockwiseAngle = 5f;
    
    [Tooltip("顺时针旋转角度")]
    public float clockwiseAngle = 10f;
    
    [Tooltip("每次旋转持续时间（秒）")]
    public float intervalTime = 0.5f;
    
    private Quaternion originalRotation;
    private float elapsed;
    private bool isShaking = true;
    private bool isCounterClockwise = true;
    
    void Start()
    {
        originalRotation = transform.rotation;
        transform.Rotate(Vector3.forward, counterClockwiseAngle);
    }
    
    void Update()
    {
        if (!isShaking)
            return;
        
        elapsed += Time.deltaTime;
        
        if (elapsed >= intervalTime)
        {
            elapsed = 0f;
            
            if (isCounterClockwise)
            {
                transform.Rotate(Vector3.forward, -clockwiseAngle);
            }
            else
            {
                transform.Rotate(Vector3.forward, clockwiseAngle);
            }
            
            isCounterClockwise = !isCounterClockwise;
        }
    }
    
    public void StartShake()
    {
        isShaking = true;
        elapsed = 0f;
    }
    
    public void StopShake()
    {
        isShaking = false;
        transform.rotation = originalRotation;
    }
}
