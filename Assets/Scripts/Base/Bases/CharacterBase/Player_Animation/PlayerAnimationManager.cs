using UnityEngine;

/// <summary>
/// 玩家动画管理器
/// 负责处理玩家角色的动画状态切换
/// </summary>
public class PlayerAnimationManager : AnimationManagerBase
{
    // 预定义的动画状态名称，移除了 ATTACK, HURT 和 DEATH
    public static class AnimationNames
    {
        public const string IDLE = "Idle";
        public const string RUN = "Run";
        public const string JUMP = "Jump";
        public const string FALL = "Fall";
    }

    protected override void Awake()
    {
        base.Awake();
        // 玩家特定的初始化
    }

    // 简化后的动画播放方法，移除了 Attack, Hurt 和 Death
    public void PlayIdle() => PlayAnimation(AnimationNames.IDLE);
    public void PlayRun() => PlayAnimation(AnimationNames.RUN);
    public void PlayJump() => PlayAnimation(AnimationNames.JUMP);
    public void PlayFall() => PlayAnimation(AnimationNames.FALL);

    // 简化后的状态切换方法
    public void PlayAnimationByState(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Idle:
                PlayIdle();
                break;
            case AnimationState.Run:
                PlayRun();
                break;
            case AnimationState.Jump:
                PlayJump();
                break;
            case AnimationState.Fall:
                PlayFall();
                break;
        }
    }
} 