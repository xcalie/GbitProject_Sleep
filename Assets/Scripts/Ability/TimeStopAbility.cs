using UnityEngine;

public class TimeStopAbility : AbilityBase
{
    private float originalFixedDeltaTime; // 原始的固定时间步长
    private Player player; // 玩家引用
    private Rigidbody2D playerRb; // 玩家的刚体组件
    private const float SMALL_TIME = 0.0001f; // 极小的时间值，用于模拟时间停止
    private Vector2 savedVelocity; // 保存原始速度
    [SerializeField] private LayerMask collisionMask; // 碰撞检测层级
    private PhysicsScene2D physicsScene; // 物理场景
    [SerializeField] private float timeStopScale = 0.01f; // 时间缩放值
    
    private void Awake()
    {
        player = GetComponent<Player>();
        playerRb = GetComponent<Rigidbody2D>();
        physicsScene = Physics2D.defaultPhysicsScene;
    }
    
    public override void Activate()
    {
        if (!CanActivate()) return;
        
        base.Activate();
        
        // 保存当前速度状态
        if (playerRb != null)
        {
            savedVelocity = playerRb.velocity;
            playerRb.simulated = true;
        }
        
        // 使用可配置的时间缩放值
        Time.timeScale = timeStopScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * timeStopScale;
        
        if (player != null)
        {
            player.OnTimeStopStart();
        }
    }
    
    public override void Deactivate()
    {
        if (!isActive) return;
        
        if (playerRb != null)
        {
            playerRb.velocity = savedVelocity;
        }
        
        base.Deactivate();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        
        if (player != null)
        {
            player.OnTimeStopEnd();
        }
    }
}
