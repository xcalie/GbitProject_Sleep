using UnityEngine;
using System.Collections.Generic;

// AnimationManagerBase 类：动画管理器的基类，实现 IAnimatable 接口
public abstract class AnimationManagerBase : MonoBehaviour, IAnimatable
{
    [Header("基础组件")]
    [SerializeField] protected SpriteRenderer spriteRenderer;  // 精灵渲染器
    [SerializeField] protected AudioSource audioSource;        // 音频源
    
    [Header("动画设置")]
    [SerializeField] protected AnimationDataBase animationData;  // 动画数据

    protected Dictionary<string, AnimationDataBase.AnimationClipData> animationDict;  // 动画片段字典
    protected string currentState;        // 当前动画状态
    protected float currentTime;          // 当前动画时间
    protected AnimationDataBase.AnimationClipData currentClip;  // 当前动画片段
    protected bool isPaused;              // 是否暂停
    protected int currentFrameIndex;      // 当前帧索引
    
    // 事件系统
    public System.Action<string> onAnimationStart;             // 动画开始事件
    public System.Action<string> onAnimationComplete;          // 动画完成事件
    public System.Action<string, int> onFrameChanged;          // 帧变化事件
    public System.Action<int> onEventFrame;                    // 特定帧事件
    
    // Awake 方法：初始化组件和动画
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeAnimations();
    }
    
    // 初始化组件
    protected virtual void InitializeComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    // 初始化动画
    protected virtual void InitializeAnimations()
    {
        if (animationData == null)
        {
            Debug.LogError($"Animation Data is missing on {gameObject.name}!");
            return;
        }

        animationDict = new Dictionary<string, AnimationDataBase.AnimationClipData>();
        foreach (var clip in animationData.clips)
        {
            animationDict[clip.stateName] = clip;
        }
    }
    
    // Update 方法：每帧更新动画
    protected virtual void Update()
    {
        if (!isPaused)
        {
            UpdateAnimation();
        }
    }
    
    // 更新动画
    protected virtual void UpdateAnimation()
    {
        if (currentClip == null) return;
        
        float deltaTime = currentClip.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        currentTime += deltaTime;
        
        int lastFrameIndex = currentFrameIndex;
        currentFrameIndex = Mathf.FloorToInt(currentTime * currentClip.frameRate);
        
        // 更新精灵
        if (currentFrameIndex < currentClip.sprites.Length)
        {
            spriteRenderer.sprite = currentClip.sprites[currentFrameIndex];
            
            // 帧变化事件
            if (currentFrameIndex != lastFrameIndex)
            {
                onFrameChanged?.Invoke(currentState, currentFrameIndex);
                HandleEventFrame(currentFrameIndex);
            }
        }
        
        // 检查动画完成
        if (currentFrameIndex >= currentClip.sprites.Length)
        {
            if (currentClip.isLoop)
            {
                currentTime = 0;
                currentFrameIndex = 0;
            }
            else
            {
                onAnimationComplete?.Invoke(currentState);
                isPaused = true;
            }
        }
    }
    
    // 处理特定帧事件
    protected virtual void HandleEventFrame(int frameIndex)
    {
        // 处理特效
        if (currentClip.effectPrefab != null)
        {
            Vector3 position = transform.position + (Vector3)currentClip.effectOffset;
            Instantiate(currentClip.effectPrefab, position, Quaternion.identity);
        }
        
        // 处理音效
        if (currentClip.soundEffect != null)
        {
            audioSource.PlayOneShot(currentClip.soundEffect, currentClip.volume);
        }
    }
    
    #region IAnimatable Implementation
    
    // 播放动画
    public virtual void PlayAnimation(string stateName, bool forceRestart = false)
    {
        if (currentState == stateName && !forceRestart) return;
        
        if (animationDict.TryGetValue(stateName, out AnimationDataBase.AnimationClipData newClip))
        {
            if (currentState != stateName)
            {
                onAnimationStart?.Invoke(stateName);
            }
            
            currentState = stateName;
            currentClip = newClip;
            
            if (forceRestart)
            {
                currentTime = 0;
                currentFrameIndex = 0;
            }
            
            isPaused = false;
        }
        else
        {
            Debug.LogWarning($"Animation not found: {stateName}");
        }
    }
    
    // 停止动画
    public virtual void StopAnimation()
    {
        currentClip = null;
        currentState = null;
        currentTime = 0;
        currentFrameIndex = 0;
        isPaused = true;
    }
    
    // 暂停动画
    public virtual void PauseAnimation()
    {
        isPaused = true;
    }
    
    // 恢复动画
    public virtual void ResumeAnimation()
    {
        isPaused = false;
    }
    
    // 检查是否正在播放指定动画
    public virtual bool IsPlaying(string stateName)
    {
        return currentState == stateName && !isPaused;
    }
    
    // 获取当前动画进度
    public virtual float GetAnimationProgress()
    {
        if (currentClip == null) return 0f;
        return Mathf.Clamp01(currentTime / (currentClip.sprites.Length / currentClip.frameRate));
    }
    
    // 翻转精灵
    public virtual void FlipSprite(bool flip)
    {
        spriteRenderer.flipX = flip;
    }
    
    #endregion
}
