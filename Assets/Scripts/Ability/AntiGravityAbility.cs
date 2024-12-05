using UnityEngine;

// 反重力能力类,继承自AbilityBase基类
public class AntiGravityAbility : AbilityBase
{
    // 玩家的刚体组件引用
    private Rigidbody2D playerRb;
    // 记录原始重力缩放值
    private float originalGravity;
    // 翻转四元数
    private Quaternion flipRotation;
    
    // 添加对Player组件的引用
    private Player player;
    
    // 添加一个标志来跟踪重力方向
    private bool isGravityInverted = false;
    
    // 添加对时间减速能力的引用
    private TimeSlowDownAbility timeSlowDownAbility;
    
    // Awake在对象实例化时调用,用于初始化
    private void Awake()
    {
        // 获取玩家刚体组件
        playerRb = GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            Debug.LogError("AntiGravityAbility requires a Rigidbody2D component!");
            return;
        }
        
        // 保存原始重力缩放值
        originalGravity = playerRb.gravityScale;
        // 生成一个180度的翻转四元数
        flipRotation = Quaternion.AngleAxis(180f, this.transform.right);
        
        // 获取Player组件
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("AntiGravityAbility requires a Player component!");
        }
        
        // 获取时间减速能力的引用
        timeSlowDownAbility = GetComponent<TimeSlowDownAbility>();
    }
    
    // 激活反重力能力
    public override void Activate()
    {
        // 如果能力未解锁或不能激活则直接返回
        if (!CanActivate() || MainControl.Instance.PlayerAddressSt == AddressState.air) 
            return;

        Debug.Log("激活反重力能力");
        
        // 调用基类的激活方法
        base.Activate();
        isGravityInverted = true;
        
        // 立即更新重力
        UpdateGravityScale();
        
        // 翻转玩家的Y轴旋转
        this.transform.rotation = flipRotation;
    }
    
    // 停用反重力能力
    public override void Deactivate()
    {
        // 如果能力未激活或不能停用则直接返回
        if (!isActive || MainControl.Instance.PlayerAddressSt == AddressState.air) 
            return;

        Debug.Log("停用反重力能力");

        // 调用基类的停用方法
        base.Deactivate();
        isGravityInverted = false;
        
        // 立即更新重力
        UpdateGravityScale();
        
        // 恢复玩家的Y轴旋转
        this.transform.rotation = Quaternion.identity;
    }
    
    // 能否激活
    public override bool CanActivate()
    {
        return base.CanActivate() && playerRb != null;
    }
    
    // 每帧更新
    public override void OnUpdate()
    {
        if (isActive != isGravityInverted)
        {
            isGravityInverted = isActive;
            UpdateGravityScale();
        }
    }
    
    // 添加一个公共方法用于获取当前重力状态
    public bool IsGravityInverted()
    {
        return isGravityInverted;
    }
    
    // 新增：更新重力缩放值的方法
    private void UpdateGravityScale()
    {
        float baseGravityScale = MainControl.Instance.Gravity / Physics2D.gravity.y;
        float direction = isGravityInverted ? -1f : 1f;
        playerRb.gravityScale = baseGravityScale * direction;
    }
}
