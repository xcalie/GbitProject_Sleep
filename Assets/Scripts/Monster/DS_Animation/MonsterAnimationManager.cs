using UnityEngine;
using System.Collections;

public class MonsterAnimationManager : AnimationManagerBase
{
    [Header("Monster特有组件")]
    [SerializeField] protected TrailRenderer trailRenderer;
    
    // Monster动画状态
    public static class MonsterStates
    {
        public const string ATTACK = "Attack";        // 攻击状态
        public const string PATROL = "Patrol";        // 巡逻状态
        public const string APPEAR = "Appear";        // 出现状态
        public const string DISAPPEAR = "Disappear";  // 消失状态
        public const string HURT = "Hurt";           // 受伤状态
        public const string DEATH = "Death";         // 死亡状态
        public const string MOVE = "Move";           // 移动状态
        public const string IDLE = "Idle";           // 待机状态
    }
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        if (trailRenderer == null)
            trailRenderer = GetComponentInChildren<TrailRenderer>();
    }
    
    // 简单的动画播放方法
    public virtual void PlayAttackAnimation()
    {
        PlayAnimation(MonsterStates.ATTACK, true);
    }
    
    public virtual void PlayPatrolAnimation()
    {
        PlayAnimation(MonsterStates.PATROL);
    }
    
    public virtual void PlayAppearAnimation()
    {
        PlayAnimation(MonsterStates.APPEAR, true);
    }
    
    public virtual void PlayDisappearAnimation()
    {
        PlayAnimation(MonsterStates.DISAPPEAR, true);
    }
    
    public virtual void PlayHurtAnimation()
    {
        PlayAnimation(MonsterStates.HURT, true);
    }
    
    public virtual void PlayDeathAnimation()
    {
        PlayAnimation(MonsterStates.DEATH, true);
    }
    
    public virtual void PlayMoveAnimation()
    {
        PlayAnimation(MonsterStates.MOVE);
    }
    
    public virtual void PlayIdleAnimation()
    {
        PlayAnimation(MonsterStates.IDLE);
    }
    
    /// <summary>
    /// 强制播放攻击动画并等待完成
    /// </summary>
    public IEnumerator PlayAttackAnimationForced()
    {
        // 播放攻击动画
        PlayAnimation(MonsterStates.ATTACK, true);
        
        // 等待动画完成
        float animationDuration = currentClip.sprites.Length / currentClip.frameRate;
        yield return new WaitForSeconds(animationDuration);
    }

    /// <summary>
    /// 强制播放死亡动画并等待完成
    /// </summary>
    public IEnumerator PlayDeathAnimationForced()
    {
        // 播放死亡动画
        PlayAnimation(MonsterStates.DEATH, true);
        
        // 等待动画完成
        float animationDuration = currentClip.sprites.Length / currentClip.frameRate;
        yield return new WaitForSeconds(animationDuration);
    }
    
    // 重写基类的PlayAnimation方法
    public override void PlayAnimation(string stateName, bool oneShot = false)
    {
        base.PlayAnimation(stateName, oneShot);
    }
} 