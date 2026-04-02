/**
 * @file CameraManager.cs
 * @brief 相机管理器 - 管理三个相机的配置和Viewport设置
 * @author ZHY
 * @version 1.0
 * @time 26-4-2
 */
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("相机引用")]
    public Camera mainCamera;           // 主相机（共享视图）
    public Camera cameraA;              // 玩家A跟随相机
    public Camera cameraB;              // 玩家B跟随相机
    
    [Header("分屏配置")]
    public bool useSplitScreen = true;  // 是否使用分屏显示
    public SplitScreenMode splitMode = SplitScreenMode.ThreeWay;
    
    [Header("相机参数")]
    public float mainCameraSize = 12f;  // 主相机视野大小
    public float playerCameraSize = 8f; // 玩家相机视野大小
    
    public enum SplitScreenMode
    {
        ThreeWay,       // 三分屏：主相机在上，两个玩家相机在下
        PictureInPicture // 画中画：主相机全屏，玩家相机小窗口
    }
    
    void Start()
    {
        InitializeCameras();
        SetupViewports();
    }
    
    /// <summary>
    /// 初始化所有相机
    /// </summary>
    private void InitializeCameras()
    {
        // 查找或创建相机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 确保所有相机都有对应的控制器
        SetupMainCamera();
        SetupPlayerCameras();
    }
    
    /// <summary>
    /// 配置主相机
    /// </summary>
    private void SetupMainCamera()
    {
        if (mainCamera == null) return;
        
        // 确保有SharedCameraController
        SharedCameraController sharedController = mainCamera.GetComponent<SharedCameraController>();
        if (sharedController == null)
        {
            sharedController = mainCamera.gameObject.AddComponent<SharedCameraController>();
        }
        
        // 设置深度（确保渲染顺序）
        mainCamera.depth = 0;
    }
    
    /// <summary>
    /// 配置玩家相机
    /// </summary>
    private void SetupPlayerCameras()
    {
        // 配置相机A
        if (cameraA != null)
        {
            PlayerCameraController controllerA = cameraA.GetComponent<PlayerCameraController>();
            if (controllerA == null)
            {
                controllerA = cameraA.gameObject.AddComponent<PlayerCameraController>();
            }
            controllerA.targetPlayerId = 1;
            controllerA.cameraSize = playerCameraSize;
            cameraA.depth = 1;
        }
        
        // 配置相机B
        if (cameraB != null)
        {
            PlayerCameraController controllerB = cameraB.GetComponent<PlayerCameraController>();
            if (controllerB == null)
            {
                controllerB = cameraB.gameObject.AddComponent<PlayerCameraController>();
            }
            controllerB.targetPlayerId = 2;
            controllerB.cameraSize = playerCameraSize;
            cameraB.depth = 1;
        }
    }
    
    /// <summary>
    /// 设置相机Viewport（分屏显示）
    /// </summary>
<<<<<<< Updated upstream
    private void SetupViewports()
=======
    public void SetupViewports()
>>>>>>> Stashed changes
    {
        if (!useSplitScreen) return;
        
        switch (splitMode)
        {
            case SplitScreenMode.ThreeWay:
                SetupThreeWaySplit();
                break;
            case SplitScreenMode.PictureInPicture:
                SetupPictureInPicture();
                break;
        }
    }
    
    /// <summary>
    /// 三分屏布局
    /// 主相机：上半部分
    /// 相机A：左下角
    /// 相机B：右下角
    /// </summary>
    private void SetupThreeWaySplit()
    {   
        if (cameraA != null)
        {
            cameraA.rect = new Rect(0, 0, 0.5f, 1f);
        }
        
        if (cameraB != null)
        {
            cameraB.rect = new Rect(0.5f, 0, 0.5f, 1f);
        }
    }
    
    /// <summary>
    /// 画中画布局
    /// 主相机：全屏
    /// 相机A和B：小窗口在角落
    /// </summary>
    private void SetupPictureInPicture()
    {
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        
        if (cameraA != null)
        {
            cameraA.rect = new Rect(0.02f, 0.02f, 0.2f, 0.2f);
        }
        
        if (cameraB != null)
        {
            cameraB.rect = new Rect(0.78f, 0.02f, 0.2f, 0.2f);
        }
    }
    
    /// <summary>
    /// 切换分屏模式
    /// </summary>
    public void SetSplitScreenMode(SplitScreenMode mode)
    {
        splitMode = mode;
        SetupViewports();
    }
    
    /// <summary>
    /// 设置相机启用状态
    /// </summary>
    public void SetCameraEnabled(CameraType cameraType, bool enabled)
    {
        switch (cameraType)
        {
            case CameraType.Main:
                if (mainCamera != null) mainCamera.enabled = enabled;
                break;
            case CameraType.PlayerA:
                if (cameraA != null) cameraA.enabled = enabled;
                break;
            case CameraType.PlayerB:
                if (cameraB != null) cameraB.enabled = enabled;
                break;
        }
    }
    
    public enum CameraType
    {
        Main,
        PlayerA,
        PlayerB
    }
    
    /// <summary>
    /// 设置所有相机视野大小
    /// </summary>
    public void SetAllCameraSizes(float mainSize, float playerSize)
    {
        mainCameraSize = mainSize;
        playerCameraSize = playerSize;
        
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = mainSize;
        }
        
        if (cameraA != null)
        {
            cameraA.orthographicSize = playerSize;
        }
        
        if (cameraB != null)
        {
            cameraB.orthographicSize = playerSize;
        }
    }
    
    void OnValidate()
    {
        // 在编辑器中修改参数时自动更新
        if (Application.isPlaying)
        {
            SetupViewports();
        }
    }
}
