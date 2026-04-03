using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    private AudioSource audioSource;
    
    void Awake()
    {
        // 单例模式，防止重复实例
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 获取或添加 AudioSource 组件
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 设置音频属性
            audioSource.loop = true; // 循环播放
            audioSource.volume = 0.5f; // 设置音量
            
            // 自动开始播放
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // 如果已经存在实例，销毁当前对象
            Destroy(gameObject);
        }
    }
}