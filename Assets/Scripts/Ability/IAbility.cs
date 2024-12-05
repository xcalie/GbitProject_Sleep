using UnityEngine;

// 定义能力接口
public interface IAbility
{
    bool IsUnlocked { get; }  // 能力是否已解锁
    bool IsActive { get; }    // 能力是否处于激活状态
    bool CanActivate();       // 检查能力是否可以被激活
    void Activate();          // 激活能力
    void Deactivate();        // 停用能力
    void OnUpdate();          // 能力的更新逻辑
    void Unlock();            // 解锁能力
}

// 能力基类，实现IAbility接口
public abstract class AbilityBase : MonoBehaviour, IAbility
{
    protected bool isUnlocked = false;  // 能力解锁状态
    protected bool isActive = false;    // 能力激活状态
    
    public bool IsUnlocked => isUnlocked;  // 获取能力解锁状态
    public bool IsActive => isActive;      // 获取能力激活状态
    
    // 检查能力是否可以被激活
    public virtual bool CanActivate()
    {
        return isUnlocked && !isActive;  // 能力已解锁且当前未激活
    }
    
    // 激活能力
    public virtual void Activate()
    {
        if (!CanActivate()) return;  // 如果不能激活，直接返回
        isActive = true;             // 设置激活状态为true
    }
    
    // 停用能力
    public virtual void Deactivate()
    {
        isActive = false;  // 设置激活状态为false
    }
    
    // 能力的更新逻辑，默认为空
    public virtual void OnUpdate() { }
    
    // 解锁能力
    public virtual void Unlock()
    {
        isUnlocked = true;  // 设置解锁状态为true
        Debug.Log($"{GetType().Name} has been unlocked!");  // 输出解锁日志
    }
}
