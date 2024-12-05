using UnityEngine;

/// <summary>
/// 时间减缓能力类
/// 实现游戏时间的减缓效果，同时保持其他能力（如反重力）的效果
/// </summary>
public class TimeSlowDownAbility : AbilityBase
{
    [SerializeField] private float slowdownFactor = 0.3f;    
    private float originalFixedDeltaTime;                    
    private Player player;                                   
    private Rigidbody2D playerRb;                           
    private AntiGravityAbility antiGravityAbility;          // 添加对反重力能力的引用
    
    private void Awake()
    {
        player = GetComponent<Player>();
        playerRb = GetComponent<Rigidbody2D>();
        antiGravityAbility = GetComponent<AntiGravityAbility>();
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }
    
    public override void Activate()
    {
        if (!CanActivate()) return;
        
        base.Activate();
        
        // 设置时间缩放
        Time.fixedDeltaTime = originalFixedDeltaTime * slowdownFactor;
        Time.timeScale = slowdownFactor;
        
        if (playerRb != null && player != null)
        {
            playerRb.simulated = true;
            UpdateGravityScale();
            player.SetTimeSlowDown(true, slowdownFactor);
        }
    }
    
    public override void Deactivate()
    {
        if (!isActive) return;
        
        if (playerRb != null && player != null)
        {
            UpdateGravityScale();
            player.SetTimeSlowDown(false, 1f);
        }
        
        base.Deactivate();
        Time.fixedDeltaTime = originalFixedDeltaTime;
        Time.timeScale = 1f;
    }

    // 新增：更新重力缩放值的方法
    private void UpdateGravityScale()
    {
        float baseGravityScale = MainControl.Instance.Gravity / Physics2D.gravity.y;
        float direction = (antiGravityAbility != null && antiGravityAbility.IsGravityInverted()) ? -1f : 1f;
        playerRb.gravityScale = baseGravityScale * direction;
    }
}