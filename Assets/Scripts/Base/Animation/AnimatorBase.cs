using UnityEngine;
using System.Collections.Generic;

// 要求SpriteRenderer组件
[RequireComponent(typeof(SpriteRenderer))]
public class AnimatorBase : MonoBehaviour
{
    [SerializeField] protected AnimationDataBase animationData;  // 动画数据
    
    protected SpriteRenderer spriteRenderer;  // 精灵渲染器
    protected Dictionary<string, AnimationDataBase.AnimationClipData> animationDict;  // 动画片段字典
    protected AnimationDataBase.AnimationClipData currentClip;  // 当前播放的动画片段
    protected float frameTimer;  // 帧计时器
    protected int currentFrameIndex;  // 当前帧索引
    protected bool isPlaying;  // 是否正在播放
    protected System.Action onComplete;  // 动画完成回调
    protected AudioSource audioSource;  // 音频源
    protected string lastPlayedAnimation;  // 上一个播放的动画名称

    // 初始化
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        InitializeAnimations();
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

    // 每帧更新
    protected virtual void Update()
    {
        if (!isPlaying || currentClip == null) return;

        float deltaTime = currentClip.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        frameTimer += deltaTime;
        
        if (frameTimer >= 1f / currentClip.frameRate)
        {
            frameTimer = 0;
            NextFrame();
        }
    }

    // 切换到下一帧
    protected virtual void NextFrame()
    {
        currentFrameIndex++;
        
        if (currentFrameIndex >= currentClip.sprites.Length)
        {
            if (currentClip.isLoop)
            {
                currentFrameIndex = 0;
            }
            else
            {
                isPlaying = false;
                onComplete?.Invoke();
                return;
            }
        }

        spriteRenderer.sprite = currentClip.sprites[currentFrameIndex];
    }

    // 播放指定动画
    public virtual void Play(string stateName, System.Action onCompleteCallback = null)
    {
        if (string.IsNullOrEmpty(stateName))
        {
            Debug.LogError("Animation state name cannot be null or empty");
            return;
        }

        if (!animationDict.TryGetValue(stateName, out var clip))
        {
            Debug.LogError($"找不到动画: {stateName}");
            return;
        }

        if (stateName == lastPlayedAnimation && isPlaying) return;

        currentClip = clip;
        currentFrameIndex = -1;
        frameTimer = 0;
        isPlaying = true;
        onComplete = onCompleteCallback;
        lastPlayedAnimation = stateName;

        // 播放音效
        if (clip.soundEffect != null)
        {
            audioSource.PlayOneShot(clip.soundEffect, clip.volume);
        }
        
        NextFrame();
    }

    // 停止当前动画
    public virtual void Stop()
    {
        isPlaying = false;
        onComplete = null;
    }

    // 翻转精灵
    public virtual void FlipSprite(bool flip)
    {
        spriteRenderer.flipX = flip;
    }

    // 获取当前动画名称
    public virtual string GetCurrentAnimationName()
    {
        return currentClip?.stateName;
    }

    // 检查是否存在指定动画
    public virtual bool HasAnimation(string stateName)
    {
        return animationDict.ContainsKey(stateName);
    }

    // 组件禁用时停止动画
    protected virtual void OnDisable()
    {
        Stop();
    }
}
