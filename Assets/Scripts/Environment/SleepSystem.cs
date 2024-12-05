using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// 睡眠系统类，管理玩家的疲劳度和相关视觉效果
/// 此类负责模拟玩家的疲劳状态，并通过后处理效果来可视化疲劳程度
/// </summary>
public class SleepSystem : MonoBehaviour
{
    private static SleepSystem instance;  // 睡眠系统的单例实例
    public static SleepSystem Instance => instance;  // 获取单例实例的属性

    [Header("后处理组件")]
    [SerializeField] private Volume postProcessVolume;  // 后处理音量控制器
    private DepthOfField depthOfField;    // URP的景深效果，用于模拟视觉模糊
    private Vignette vignette;            // URP的暗角效果，用于模拟视野收缩
    private ChromaticAberration chromatic; // URP的色差效果，用于模拟视觉扭曲

    [Header("睡眠参数")]
    [SerializeField] private float maxSleepiness = 100f;      // 最大疲劳值
    [SerializeField] private float sleepIncreaseRate = 5f;    // 基础疲劳增加速率
    [SerializeField] private float currentSleepiness = 0f;    // 当前疲劳值
    [SerializeField] private float decelerationThreshold = 0.3f;  // 30%时开始减速
    [SerializeField] private float minSpeedMultiplier = 0.1f;    // 降至十分之一速度
    
    [Header("效果参数")]
    [SerializeField] private float maxBlurFocus = 10f;        // 最大模糊焦距，用于景深效果
    [SerializeField] private float maxVignetteIntensity = 0.35f; // 最大暗角强度
    [SerializeField] private float maxChromaticIntensity = 3f;  // 最大色差强度
    
    private bool isSystemActive = false;  // 系统是否激活，用于控制整个系统的开关
    private float _sleepRatio;            // 当前疲劳比例，用于计算视觉效果强度

    /// <summary>
    /// 初始化后处理效果组件
    /// 在游戏开始时调用，设置初始状态
    /// </summary>
    private void Awake()
    {
        instance = this;  // 设置单例实例为当前实例

        // 检查后处理音量和配置文件是否存在
        if (postProcessVolume == null || postProcessVolume.profile == null) return;

        var profile = postProcessVolume.profile;
        // 尝试获取各种后处理效果组件
        profile.TryGet(out depthOfField);
        profile.TryGet(out vignette);
        profile.TryGet(out chromatic);
        
        // 初始时禁用景深效果，启用暗角和色差效果
        if (depthOfField) depthOfField.active = false;
        if (vignette) vignette.active = true;
        if (chromatic) chromatic.active = true;
        
        // 重置所有视觉效果到初始状态
        ResetEffects();
    }

    /// <summary>
    /// 初始化系统参数和设置
    /// 在Awake之后调用，进行额外的初始化
    /// </summary>
    private void Start()
    {
        // 计算初始疲劳比例
        _sleepRatio = currentSleepiness / maxSleepiness;
        
        // 设置目标帧率为60，如果启用了垂直同步
        if (QualitySettings.vSyncCount > 0) {
            Application.targetFrameRate = 60;
        }

        // 设置后处理音量权重为最大
        postProcessVolume.weight = 1f;
        // 激活睡眠系统
        ActivateSystem();
    }

    /// <summary>
    /// 每帧更新疲劳度和视觉效果
    /// 这是系统的主要运行逻辑
    /// </summary>
    private void Update()
    {
        if (!isSystemActive) return;

        // 计算当前疲劳比例
        float currentRatio = currentSleepiness / maxSleepiness;
        
        // 计算速度倍率：30%之前保持原速，之后降至十分之一
        float speedMultiplier = 1f;
        if (currentRatio > decelerationThreshold)
        {
            speedMultiplier = minSpeedMultiplier; // 直接降至0.1倍速度
        }
        
        // 应用速度倍率更新疲劳值
        currentSleepiness = Mathf.Min(
            currentSleepiness + (sleepIncreaseRate * speedMultiplier) * Time.deltaTime,
            maxSleepiness
        );

        UpdateVisualEffects();
    }

    /// <summary>
    /// 更新视觉效果
    /// 根据当前疲劳度调整各种后处理效果的参数
    /// </summary>
    private void UpdateVisualEffects()
    {
        // 计算新的疲劳比例
        float newRatio = currentSleepiness / maxSleepiness;
        // 只有当疲劳比例变化超过阈值时才更新效果，以优化性能
        if (Mathf.Abs(newRatio - _sleepRatio) > 0.01f) 
        {
            _sleepRatio = newRatio;
            
            // 更新景深效果
            if (depthOfField)
            {
                // 当疲劳度开始增加时才启用景深效果
                if (_sleepRatio > 0.01f && !depthOfField.active)
                {
                    depthOfField.active = true;
                }
                else if (_sleepRatio <= 0.01f && depthOfField.active)
                {
                    depthOfField.active = false;
                }

                // 根据疲劳度调整景深参数
                depthOfField.focusDistance.value = Mathf.Lerp(10f, maxBlurFocus, _sleepRatio);
                depthOfField.focalLength.value = Mathf.Lerp(1f, 300f, _sleepRatio);
                depthOfField.aperture.value = Mathf.Lerp(1f, 32f, _sleepRatio);
            }
            
            // 更新暗角效果
            if (vignette)
                vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, _sleepRatio);
            
            // 更新色差效果
            if (chromatic)
                chromatic.intensity.value = Mathf.Lerp(0f, maxChromaticIntensity, _sleepRatio);
        }
    }

    /// <summary>
    /// 重置所有视觉效果
    /// 将所有后处理效果恢复到初始状态
    /// </summary>
    private void ResetEffects()
    {
        // 重置景深效果
        if (depthOfField) 
        {
            depthOfField.focusDistance.value = 10f;
            depthOfField.focalLength.value = 1f;
            depthOfField.aperture.value = 1f;
        }
        // 重置暗角效果
        if (vignette) vignette.intensity.value = 0f;
        // 重置色差效果
        if (chromatic) chromatic.intensity.value = 0f;
    }

    /// <summary>
    /// 刷新清醒度
    /// 减少当前疲劳值，模拟玩家变得更清醒
    /// </summary>
    /// <param name="amount">清醒度增加量，即疲劳值减少量</param>
    public void RefreshAwakeness(float amount)
    {
        currentSleepiness = Mathf.Max(0f, currentSleepiness - amount);
        UpdateVisualEffects();
    }

    /// <summary>
    /// 完全清醒
    /// 将疲劳值重置为0，并重置所有视觉效果
    /// </summary>
    public void FullyAwake()
    {
        currentSleepiness = 0f;
        ResetEffects();
    }

    /// <summary>
    /// 激活睡眠系统
    /// 开启系统并重置疲劳值
    /// </summary>
    public void ActivateSystem()
    {
        isSystemActive = true;
        currentSleepiness = 0f;
    }

    /// <summary>
    /// 停用睡眠系统
    /// 关闭系统并使玩家完全清醒
    /// </summary>
    public void DeactivateSystem()
    {
        isSystemActive = false;
        FullyAwake();
    }

    /// <summary>
    /// 获取当前疲劳度比例
    /// 用于外部系统查询当前疲劳状态
    /// </summary>
    /// <returns>疲劳度比例，范围0到1</returns>
    public float GetSleepinessRatio()
    {
        return currentSleepiness / maxSleepiness;
    }
}