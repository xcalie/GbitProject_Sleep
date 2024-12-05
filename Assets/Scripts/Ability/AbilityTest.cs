using UnityEngine;

// 能力测试脚本，用于测试所有能力的功能
public class AbilityTest : MonoBehaviour
{
    // 引用能力管理器
    private AbilityManager abilityManager;
    
    // 测试用的能力道具
    private AbilityPickup abilityPickup;
    
    // 测试按键配置
    [Header("测试按键配置")]
    [SerializeField] private KeyCode unlockAntiGravityKey = KeyCode.Alpha1;    // 解锁反重力能力
    [SerializeField] private KeyCode unlockTimeStopKey = KeyCode.Alpha2;       // 解锁时间停止能力
    [SerializeField] private KeyCode unlockTimeSlowKey = KeyCode.Alpha3;       // 解锁时间减缓能力
    
    [Header("调试信息显示")]
    [SerializeField] private bool showDebugInfo = true;                        // 是否显示调试信息
    
    private void Start()
    {
        // 获取必要组件
        abilityManager = GetComponent<AbilityManager>();
        abilityPickup = FindObjectOfType<AbilityPickup>();
        
        if (abilityManager == null)
        {
            Debug.LogError("未找到AbilityManager组件！");
            enabled = false;
            return;
        }
        
        if (abilityPickup == null)
        {
            Debug.LogWarning("未找到AbilityPickup组件，部分测试功能可能无法使用。");
        }
    }
    
    private void Update()
    {
        // 测试解锁能力
        TestUnlockAbilities();
        
        // 显示调试信息
        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }
    }
    
    // 测试解锁能力
    private void TestUnlockAbilities()
    {
        // 测试解锁反重力能力
        if (Input.GetKeyDown(unlockAntiGravityKey))
        {
            Debug.Log("测试：解锁反重力能力");
            abilityPickup?.GetAbility(AbilityType.AntiGravity, GetComponent<Collider2D>());
        }
        
        // 测试解锁时间停止能力
        if (Input.GetKeyDown(unlockTimeStopKey))
        {
            Debug.Log("测试：解锁时间停止能力");
            abilityPickup?.GetAbility(AbilityType.TimeStop, GetComponent<Collider2D>());
        }
        
        // 测试解锁时间减缓能力
        if (Input.GetKeyDown(unlockTimeSlowKey))
        {
            Debug.Log("测试：解锁时间减缓能力");
            abilityPickup?.GetAbility(AbilityType.TimeSlowDown, GetComponent<Collider2D>());
        }
    }
    
    // 显示调试信息
    private void DisplayDebugInfo()
    {
        // 时间停止能力信息
        DisplayAbilityInfo(AbilityType.TimeStop);
        
        // 时间减缓能力信息
        DisplayAbilityInfo(AbilityType.TimeSlowDown);
    }
    
    // 显示单个能力的信息
    private void DisplayAbilityInfo(AbilityType abilityType)
    {
        float remainingDuration = abilityManager.GetRemainingDuration(abilityType);
        float remainingCooldown = abilityManager.GetRemainingCooldown(abilityType);
        
        if (remainingDuration > 0)
        {
            Debug.Log($"{abilityType} 剩余持续时间: {remainingDuration:F1}秒");
        }
        
        if (remainingCooldown > 0)
        {
            Debug.Log($"{abilityType} 剩余冷却时间: {remainingCooldown:F1}秒");
        }
    }
    
    // 在编辑器中显示调试信息
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        int yPos = 10;
        int lineHeight = 20;
        
        // 显示当前时间缩放
        GUI.Label(new Rect(10, yPos, 300, lineHeight), $"时间缩放: {Time.timeScale:F2}");
        yPos += lineHeight;
        
        // 显示各个能力的状态
        DisplayAbilityGUIInfo(AbilityType.AntiGravity, ref yPos);
        DisplayAbilityGUIInfo(AbilityType.TimeStop, ref yPos);
        DisplayAbilityGUIInfo(AbilityType.TimeSlowDown, ref yPos);
    }
    
    // 在GUI中显示单个能力的信息
    private void DisplayAbilityGUIInfo(AbilityType abilityType, ref int yPos)
    {
        float remainingDuration = abilityManager.GetRemainingDuration(abilityType);
        float remainingCooldown = abilityManager.GetRemainingCooldown(abilityType);
        int lineHeight = 20;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), 
            $"{abilityType}:");
        yPos += lineHeight;
        
        if (remainingDuration > 0)
        {
            GUI.Label(new Rect(30, yPos, 300, lineHeight), 
                $"持续时间: {remainingDuration:F1}秒");
            yPos += lineHeight;
        }
        
        if (remainingCooldown > 0)
        {
            GUI.Label(new Rect(30, yPos, 300, lineHeight), 
                $"冷却时间: {remainingCooldown:F1}秒");
            yPos += lineHeight;
        }
        
        yPos += 5; // 添加一些间距
    }
}
