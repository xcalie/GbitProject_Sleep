using UnityEngine;
using System.Collections.Generic;

// 能力管理器类，负责管理和控制所有能力
public class AbilityManager : MonoBehaviour
{
    // 存储所有能力的字典，键为能力类型，值为能力接口
    private Dictionary<AbilityType, IAbility> abilities;
    // 存储当前激活的能力类型
    private HashSet<AbilityType> activeAbilities;
    
    // 音频相关
    private AudioSource abilityAudioSource;
    
    // 时间技能配置
    [System.Serializable]
    public class TimeAbilityConfig
    {
        public AbilityType abilityType;    // 能力类型
        public float duration;              // 持续时间（秒）
        public float cooldownTime;          // 冷却时间（秒）
        public float timeScale;             // 时间缩放值（时间暂停用0.01，时间减缓用0.3）
        [HideInInspector]
        public float remainingDuration;     // 剩余持续时间
        [HideInInspector]
        public float remainingCooldown;     // 剩余冷却时间
    }
    
    // 在Inspector中可配置的时间技能设置
    [Header("时间技能配置")]
    [SerializeField] private TimeAbilityConfig timeStopConfig = new TimeAbilityConfig
    {
        abilityType = AbilityType.TimeStop,
        duration = 3f,
        cooldownTime = 10f,
        timeScale = 0.01f
    };
    
    [SerializeField] private TimeAbilityConfig timeSlowConfig = new TimeAbilityConfig
    {
        abilityType = AbilityType.TimeSlowDown,
        duration = 5f,
        cooldownTime = 8f,
        timeScale = 0.3f
    };
    
    // 用于快速查找的字典
    private Dictionary<AbilityType, TimeAbilityConfig> timeAbilityConfigDict;
    
    private void Awake()
    {
        // 初始化集合
        abilities = new Dictionary<AbilityType, IAbility>();
        activeAbilities = new HashSet<AbilityType>();
        
        // 初始化各种能力组件
        InitializeAbility<AntiGravityAbility>(AbilityType.AntiGravity);
        InitializeAbility<TimeStopAbility>(AbilityType.TimeStop);
        InitializeAbility<TimeSlowDownAbility>(AbilityType.TimeSlowDown);
        
        // 初始化时间技能配置字典
        timeAbilityConfigDict = new Dictionary<AbilityType, TimeAbilityConfig>
        {
            { AbilityType.TimeStop, timeStopConfig },
            { AbilityType.TimeSlowDown, timeSlowConfig }
        };
    }
    
    // 初始化指定类型的能力
    private void InitializeAbility<T>(AbilityType type) where T : MonoBehaviour, IAbility
    {
        T ability = GetComponent<T>();
        if (ability == null)
        {
            ability = gameObject.AddComponent<T>();
        }
        abilities[type] = ability;
    }
    
    private void Update()
    {
        // 更新所有激活的能力
        foreach (var ability in abilities.Values)
        {
            if (ability.IsActive)
            {
                ability.OnUpdate();
            }
        }
        
        // 处理能力输入
        HandleAbilityInputs();
        
        // 更新时间技能的持续时间和冷却时间
        UpdateTimeAbilityTimers();
    }
    
    // 处理所有能力的输入
    private void HandleAbilityInputs()
    {
        // 处理反重力能力输入
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleAbility(AbilityType.AntiGravity);
        }
        
        // 处理时间相关能力输入
        HandleTimeAbilities();
    }
    
    // 处理时间相关能力的输入逻辑
    private void HandleTimeAbilities()
    {
        // 处理时间暂停能力 (F键)
        if (Input.GetKeyDown(KeyCode.F) && !IsAnyTimeAbilityActive())
        {
            if (ActivateAbility(AbilityType.TimeStop))
            {
                MusicControl.Instance.CreateAndPlay(gameObject, ref abilityAudioSource, MusicType.Ability, 0, false, MusicControl.Instance.AUDIOVolume);
            }
        }
        
        // 处理时间减缓能力 (Shift键)
        if (Input.GetKeyDown(KeyCode.LeftShift) && !IsAnyTimeAbilityActive())
        {
            if (ActivateAbility(AbilityType.TimeSlowDown))
            {
                MusicControl.Instance.CreateAndPlay(gameObject, ref abilityAudioSource, MusicType.Ability, 0, false, MusicControl.Instance.AUDIOVolume);
            }
        }
    }
    
    // 检查是否有任何时间相关能力处于激活状态
    private bool IsAnyTimeAbilityActive()
    {
        return abilities[AbilityType.TimeStop].IsActive || 
               abilities[AbilityType.TimeSlowDown].IsActive;
    }
    
    // 停用所有时间相关能力
    private void DeactivateTimeAbilities()
    {
        abilities[AbilityType.TimeStop].Deactivate();
        abilities[AbilityType.TimeSlowDown].Deactivate();
        activeAbilities.Remove(AbilityType.TimeStop);
        activeAbilities.Remove(AbilityType.TimeSlowDown);
    }
    
    // 切换指定能力的激活状态
    private void ToggleAbility(AbilityType type)
    {
        if (!abilities.ContainsKey(type))
        {
            Debug.LogError($"Ability {type} not found!");
            return;
        }
        
        if (abilities[type].IsActive)
        {
            DeactivateAbility(type);
        }
        else
        {
            ActivateAbility(type);
        }
    }
    
    // 激活指定能力
    private bool ActivateAbility(AbilityType type)
    {
        if (!abilities.ContainsKey(type))
        {
            Debug.LogError($"Ability {type} not found!");
            return false;
        }
        
        // 检查冷却时间
        if (timeAbilityConfigDict.ContainsKey(type))
        {
            var config = timeAbilityConfigDict[type];
            if (config.remainingCooldown > 0)
            {
                Debug.Log($"Ability {type} is still on cooldown: {config.remainingCooldown:F1}s");
                return false;
            }
        }
        
        var ability = abilities[type];
        if (!ability.CanActivate())
        {
            return false;
        }
        
        // 检查时间能力互斥
        if ((type == AbilityType.TimeStop || type == AbilityType.TimeSlowDown) && 
            IsAnyTimeAbilityActive())
        {
            return false;
        }
        
        try
        {
            ability.Activate();
            activeAbilities.Add(type);
            
            // 设置持续时间和开始冷却
            if (timeAbilityConfigDict.ContainsKey(type))
            {
                var config = timeAbilityConfigDict[type];
                config.remainingDuration = config.duration;
                config.remainingCooldown = config.cooldownTime;
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to activate ability {type}: {e.Message}");
            return false;
        }
    }
    
    // 停用指定能力
    private void DeactivateAbility(AbilityType type)
    {
        if (!abilities.ContainsKey(type))
        {
            Debug.LogError($"Ability {type} not found!");
            return;
        }
        
        try
        {
            abilities[type].Deactivate();
            activeAbilities.Remove(type);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to deactivate ability {type}: {e.Message}");
        }
    }
    
    // 解锁指定能力
    public void UnlockAbility(AbilityType type)
    {
        if (!abilities.ContainsKey(type))
        {
            Debug.LogError($"Ability {type} not found!");
            return;
        }
        
        try
        {
            abilities[type].Unlock();
            Debug.Log($"Ability {type} unlocked successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to unlock ability {type}: {e.Message}");
        }
    }
    
    // 播放能力音效
    private void PlayAbilitySound(AbilityType type)
    {
        // 检查能力是否存在且处于激活状态
        if (!abilities.ContainsKey(type) || !abilities[type].IsActive)
        {
            return;
        }

        // 播放音效
        if (abilityAudioSource == null)
        {
            MusicControl.Instance.CreateAndPlay(
                this.gameObject, 
                ref abilityAudioSource, 
                MusicType.Ability, 
                0, 
                false, 
                0.5f
            );
        }
    }
    
    // 更新时间技能的计时器
    private void UpdateTimeAbilityTimers()
    {
        foreach (var config in timeAbilityConfigDict.Values)
        {
            // 更新冷却时间
            if (config.remainingCooldown > 0)
            {
                config.remainingCooldown -= Time.unscaledDeltaTime;
            }

            // 更新持续时间
            if (config.remainingDuration > 0)
            {
                config.remainingDuration -= Time.unscaledDeltaTime;
                if (config.remainingDuration <= 0)
                {
                    // 持续时间结束，自动停用能力
                    DeactivateAbility(config.abilityType);
                }
            }
        }
    }
    
    // 获取能力剩余持续时间
    public float GetRemainingDuration(AbilityType type)
    {
        if (timeAbilityConfigDict.ContainsKey(type))
        {
            return Mathf.Max(0, timeAbilityConfigDict[type].remainingDuration);
        }
        return 0f;
    }
    
    // 获取能力总持续时间
    public float GetTotalDuration(AbilityType type)
    {
        if (timeAbilityConfigDict.ContainsKey(type))
        {
            return timeAbilityConfigDict[type].duration;
        }
        return 0f;
    }
    
    // 获取能力剩余冷却时间
    public float GetRemainingCooldown(AbilityType type)
    {
        if (timeAbilityConfigDict.ContainsKey(type))
        {
            return Mathf.Max(0, timeAbilityConfigDict[type].remainingCooldown);
        }
        return 0f;
    }
    
    // 获取能力总冷却时间
    public float GetTotalCooldown(AbilityType type)
    {
        if (timeAbilityConfigDict.ContainsKey(type))
        {
            return timeAbilityConfigDict[type].cooldownTime;
        }
        return 0f;
    }
}