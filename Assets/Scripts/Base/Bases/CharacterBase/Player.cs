using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家角色控制类
/// 实现了角色的基本移动、跳跃、状态管理等功能
/// </summary>
public class Player : MonoBehaviour, IAction
{
    #region 属性设置

    [Header("组件引用")]
    private Rigidbody2D rb;                     // 刚体组件
    private AbilityManager abilityManager;       // 能力管理器组件
    private GroundTrigger groundTrigger;        // 地面检测组件

    [Header("跳跃设置")]
    [SerializeField] private int maxJumpCount = 2;     // 最大跳跃次数
    private int remainingJumps;                        // 剩余跳跃次数

    // 移动相关属性
    private float currentHorizontalSpeed = 0f;   // 当前水平速度
    private Vector2 velocity;                    // 当前速度向量
    private float moveSpeed = 0f;               // 移动速度

    // 无敌时间相关属性
    public bool IsInvincible { get; private set; }     // 是否处于无敌状态
    public float invincibilityDuration = 1.5f;         // 无敌时间持续时间

    // 控制状态
    private bool canControl = true;              // 是否可以控制角色

    // 视觉效果相关
    private SpriteRenderer spriteRenderer;       // 精灵渲染器
    public float hurtFlashInterval = 0.1f;       // 受伤闪烁间隔
    private Color originalColor;                 // 原始颜色

    [Header("击退参数")]
    [SerializeField] private float knockbackMultiplier = 2.5f;    // 击退力度倍数

    // 时间控制相关
    private bool isTimeStoped = false;           // 是否处于时间停止状态
    private bool isTimeSlowDown = false;         // 是否处于时间减速状态
    private float timeSlowDownFactor = 1f;       // 时间减速因子

    [Header("碰撞检测")]
    [SerializeField] private LayerMask collisionMask;  // 碰撞检测层级

    [Header("墙壁检测")]
    [SerializeField] private float wallCheckDistance = 0.1f;      // 墙壁检测距离
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);  // 墙壁检测区域大小
    private bool isHittingWall;                        // 是否碰到墙壁

    private PlayerAnimationManager animationManager;    // 动画管理器组件

    [Header("动画状态")]
    private AnimationState previousAnimationState = AnimationState.Idle;  // 上一个动画状态
    private bool isFacingLeft = false;                                    // 是否面向左边

    [Header("攻击设置")]
    [SerializeField] private GameObject bulletPrefab;    // 子弹预制体
    [SerializeField] private float bulletSpeed = 15f;    // 子弹速度
    [SerializeField] private float bulletDamage = 1f;    // 子弹伤害
    [SerializeField] private float shootCooldown = 0.2f; // 射击冷却时间
    private float unscaledShootTimer = 0f;              // 使用未缩放时间的射击计时器
    private AudioSource ASForShoot;                      // 射击音效源

    // 添加一个公共属性来检查是否处于时间减缓状态
    public bool IsInTimeSlowDown { get; private set; }

    #endregion

    /// <summary>
    /// 初始化组件引用和设置
    /// </summary>
    private void Awake()
    {
        CreateSonWithGroundTrigger();           // 创建地面检测子物体
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        groundTrigger = GetComponent<GroundTrigger>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        SetupRigidbody();                       // 设置刚体属性
        animationManager = GetComponent<PlayerAnimationManager>();
        if (animationManager == null)
        {
            Debug.LogError("Player缺少PlayerAnimationManager组件！");
        }
    }

    /// <summary>
    /// 初始化游戏数据和状态
    /// </summary>
    private void Start()
    {
        // 确保必要组件存在
        if (abilityManager == null)
        {
            Debug.LogError("Player缺少AbilityManager组件！");
        }

        // 设置重力
        rb.gravityScale = MainControl.Instance.Gravity / Physics2D.gravity.y;
        
        // 初始化跳跃次数
        remainingJumps = maxJumpCount;

        // 初始化时设置 MainControl 中的击退倍数
        MainControl.Instance.KnockbackMultiplier = knockbackMultiplier;
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    private void Update()
    {
        UpdatePlayerState();           // 更新玩家状态
        MoveComponent();              // 处理移动输入
        HandleShooting();             // 处理射击输入
    }

    #region 物理帧更新

    private void FixedUpdate()
    {
        // 物理相关更新
    }

    #endregion

    /// <summary>
    /// 创建带有地面检测的子物体
    /// </summary>
    private void CreateSonWithGroundTrigger()
    {
        GameObject son = new GameObject("GroundCheck");
        son.transform.parent = this.transform;

        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (capsuleCollider == null)
        {
            Debug.LogError("未找到 CapsuleCollider2D!");
            return;
        }

        float colliderHeight = capsuleCollider.size.y;
        float colliderOffset = capsuleCollider.offset.y;
        
        son.transform.localPosition = new Vector3(
            0,
            colliderOffset - (colliderHeight / 2) + 0.05f,
            0
        );

        BoxCollider2D boxCollider = son.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(
            capsuleCollider.size.x * 0.1f,
            0.1f
        );
        boxCollider.offset = Vector2.zero;
        
        var extraCheck = new GameObject("ExtraGroundCheck");
        extraCheck.transform.parent = son.transform;
        extraCheck.transform.localPosition = Vector2.zero;
        var extraCollider = extraCheck.AddComponent<BoxCollider2D>();
        extraCollider.isTrigger = true;
        extraCollider.size = new Vector2(capsuleCollider.size.x * 0.05f, 0.05f);
        extraCollider.offset = new Vector2(0, -0.05f);
        
        var groundTrigger = son.AddComponent<GroundTrigger>();
        groundTrigger.SetGroundCheckBuffer(0.1f);
    }

    /// <summary>
    /// 更新地面检测位置
    /// </summary>
    public void UpdateGroundCheckPosition()
    {
        var groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null) return;

        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (capsuleCollider == null) return;

        float colliderHeight = capsuleCollider.size.y;
        float colliderOffset = capsuleCollider.offset.y;
        
        groundCheck.localPosition = new Vector3(
            0,
            colliderOffset - (colliderHeight / 2) + 0.1f,
            0
        );

        // 更新 BoxCollider2D 大小
        var boxCollider = groundCheck.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(capsuleCollider.size.x * 0.1f, 0.1f);
        }
    }

    /// <summary>
    /// 处理移动和跳跃输入
    /// </summary>
    public void MoveComponent()
    {
        Jump();                      // 处理跳跃
        Move();                      // 处理移动
    }

    /// <summary>
    /// 处理水平移动
    /// </summary>
    private void Move()
    {
        if (!canControl) return;
        
        float moveInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = moveInput * MainControl.Instance.MaxMoveSpeed;
        
        float accel = (MainControl.Instance.PlayerAddressSt == AddressState.ground) 
            ? MainControl.Instance.GroundAcceleration 
            : MainControl.Instance.AirAcceleration;
        
        float deltaTime = Time.unscaledDeltaTime;
        
        float newMoveSpeed = Mathf.MoveTowards(moveSpeed, targetSpeed, accel * deltaTime);
        bool willHitWall = CheckWallCollision(newMoveSpeed);
        moveSpeed = willHitWall ? 0 : newMoveSpeed;
        
        velocity = rb.velocity;
        velocity.x = moveSpeed;

        if (isTimeStoped || isTimeSlowDown)
        {
            float gravity = Physics2D.gravity.y * rb.gravityScale;
            velocity.y += gravity * deltaTime;
            
            Vector2 frameMovement = velocity * deltaTime;
            Vector2 newPosition = rb.position + frameMovement;
            
            rb.MovePosition(newPosition);
            rb.velocity = velocity;
        }
        else
        {
            rb.velocity = velocity;
        }
    }

    private AudioSource ASForJump;    // 跳跃音效源
    
    /// <summary>
    /// 处理跳跃逻辑
    /// </summary>
    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            bool isOnGround = MainControl.Instance.PlayerAddressSt == AddressState.ground;
            
            if (isOnGround)
            {
                remainingJumps = maxJumpCount;
            }

            if (remainingJumps > 0)
            {
                MusicControl.Instance.CreateAndPlay(this.gameObject,ref ASForJump, MusicType.Movement, 1, false, MusicControl.Instance.AUDIOVolume);

                velocity = rb.velocity;
                float jumpForce = MainControl.Instance.JumpForce;
                
                // 设置跳跃速度
                velocity.y = rb.gravityScale > 0 ? jumpForce : -jumpForce;
                
                // 在时间减缓时使用 MovePosition
                if (isTimeSlowDown || isTimeStoped)
                {
                    rb.MovePosition(rb.position + velocity * Time.unscaledDeltaTime);
                }
                rb.velocity = velocity;
                
                MainControl.Instance.PlayerAddressSt = AddressState.air;
                remainingJumps--;
            }
        }
        
        if (Input.GetButtonUp("Jump"))
        {
            float currentVelocityY = rb.velocity.y;
            bool isMovingUp = (rb.gravityScale > 0 && currentVelocityY > 0) || 
                            (rb.gravityScale < 0 && currentVelocityY < 0);
            
            if (isMovingUp)
            {
                velocity = rb.velocity;
                velocity.y *= 0.5f;
                
                // 在时间减缓时使用 MovePosition
                if (isTimeSlowDown || isTimeStoped)
                {
                    rb.MovePosition(rb.position + velocity * Time.unscaledDeltaTime);
                }
                rb.velocity = velocity;
            }
        }
    }

    /// <summary>
    /// 更新玩家状态
    /// </summary>
    private void UpdatePlayerState()
    {
        UpdateAnimation();            // 更新动画状态
    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimation()
    {
        if (!canControl) return;

        const float velocityThreshold = 0.05f;
        AnimationState currentState;
        
        // 始终根据移动速度判断朝向
        bool shouldFlip = moveSpeed < -velocityThreshold;
        
        // 状态判断
        if (MainControl.Instance.PlayerAddressSt == AddressState.ground)
        {
            // 在地面上时，根据水平速度判断是否奔跑
            currentState = Mathf.Abs(moveSpeed) > velocityThreshold ? AnimationState.Run : AnimationState.Idle;
        }
        else
        {
            // 在空中时，根据垂直速度判断跳跃或下落
            currentState = rb.velocity.y > velocityThreshold ? AnimationState.Jump : AnimationState.Fall;
        }

        // 更新动画状态
        if (currentState != previousAnimationState)
        {
            animationManager.PlayAnimationByState(currentState);
            previousAnimationState = currentState;
        }

        // 更新朝向
        if (Mathf.Abs(moveSpeed) > velocityThreshold && shouldFlip != isFacingLeft)
        {
            animationManager.FlipSprite(shouldFlip);
            isFacingLeft = shouldFlip;
        }
    }

    // 处理射击输入
    private void HandleShooting()
    {
        if (!canControl) return;

        // 使用未缩放时间更新射击计时器
        if (unscaledShootTimer > 0)
        {
            unscaledShootTimer -= Time.unscaledDeltaTime;
        }

        // 检测射击输入（J键）
        if (Input.GetKeyDown(KeyCode.J) && unscaledShootTimer <= 0)
        {
            Shoot();
            unscaledShootTimer = shootCooldown;
        }
    }

    // 射击方法
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        // 根据角色朝向决定射击方向
        Vector2 shootDirection = isFacingLeft ? Vector2.left : Vector2.right;

        // 获取角色碰撞体尺寸
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        float characterHeight = capsule != null ? capsule.size.y : 1f;
        
        // 从角色更低位置发射，并增加前方距离
        Vector3 spawnPosition = transform.position + new Vector3(
            shootDirection.x * 1.0f,  // 增加向前偏移距离到1.0
            characterHeight * 0.05f,  // 降低发射高度到5%位置
            0
        );
        
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        // 计算子弹旋转角度
        float angle = isFacingLeft ? 180f : 0f;
        bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 设置子弹属性
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetBulletProperties(
                isPlayer: true,
                newSpeed: bulletSpeed,
                newDamage: bulletDamage,
                newLifeTime: 2f
            );
            bullet.Initialize(shootDirection);
            
            // 添加调试日志
            Debug.Log($"发射子弹 - 方向: {shootDirection}, 伤害: {bulletDamage}, 速度: {bulletSpeed}");
        }

        // 播放射击音效
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForShoot, MusicType.Shoot, 0, false, MusicControl.Instance.AUDIOVolume);
    }

    public void Attack()
    {
        Shoot();  // 现在Attack方法会触发射击
    }

    public void Die()
    {
        MainControl.Instance.isDie = true;
    }

    // 重置跳跃次数的公共法
    public void ResetJumpCount()
    {
        remainingJumps = maxJumpCount;
    }

    // 重置玩家状态
    public void ResetPlayerState()
    {
        // 重置位置状态
        MainControl.Instance.PlayerAddressSt = AddressState.ground;

        // 重置移动状态
        MainControl.Instance.PlayerMotionSt = MotionState.Stand;
    }

    // 用于地面检测的可视化（仅在编辑器中）
    //private void OnDrawGizmosSelected()
    //{
    //    if (groundCheck != null)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    //    }
    //}

    // 重置玩家状态
    public void ResetPlayer()
    {
        MainControl.Instance.PlayerHP = MainControl.Instance.PlayerOrginHP;
        MainControl.Instance.PlayerMoveSpeed = MainControl.Instance.PlayerOrginMove;
        MainControl.Instance.PlayerJumpSpeed = MainControl.Instance.PlayerOrginJump;
        MainControl.Instance.MaxMoveSpeed = MainControl.Instance.OrginMaxMoveSpeed;
        MainControl.Instance.Live = true;
        
        // 重置速度
        moveSpeed = 0f;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // 开始无敌时间
    public void StartInvincibilityPeriod()
    {
        StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        IsInvincible = true;
        
        // 闪烁效果
        float endTime = Time.time + invincibilityDuration;
        bool isVisible = false;
        
        while (Time.time < endTime)
        {
            // 在透明和原始颜色之间切换
            spriteRenderer.color = isVisible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            isVisible = !isVisible;
            yield return new WaitForSeconds(hurtFlashInterval);
        }
        
        // 恢复原始颜色
        spriteRenderer.color = originalColor;
        IsInvincible = false;
    }

    // 添加控制启用/禁用方法
    public void DisableControl()
    {
        canControl = false;
    }

    public void EnableControl()
    {
        canControl = true;
    }

    // 添加一个方法用于更新 MainControl 中的值
    private void OnValidate()
    {
        // 确保在编辑器中修改值同步到 MainControl
        if (MainControl.Instance != null)
        {
            MainControl.Instance.KnockbackMultiplier = knockbackMultiplier;
        }

        // 当在 Unity 编辑器中修改组件时自动更新 GroundCheck
        UpdateGroundCheckPosition();
    }

    // 添加这些方法
    public void OnTimeStopStart()
    {
        isTimeStoped = true;
    }
    
    public void OnTimeStopEnd()
    {
        isTimeStoped = false;
    }

    // 墙壁检测方法
    private bool CheckWallCollision(float velocityX)
    {
        if (velocityX == 0) return false;
        
        // 获取移动方向
        float direction = Mathf.Sign(velocityX);
        
        // 获取碰撞体的尺寸
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null) return false;
        
        // 创建三个检测点：上、中、下
        Vector2 center = (Vector2)transform.position;
        float height = capsule.size.y * 0.8f; // 使用碰撞体高度的80%
        
        Vector2[] origins = new Vector2[]
        {
            center + Vector2.up * (height/2),    // 上部
            center,                              // 中间
            center - Vector2.up * (height/2)     // 下部
        };
        
        float rayDistance = 0.1f; // 射线检测距离
        
        // 对每个点进行射线检测
        foreach (Vector2 origin in origins)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                new Vector2(direction, 0),
                rayDistance,
                LayerMask.GetMask("Ground")
            );
            
            if (hit.collider != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 可视化调试
    private void OnDrawGizmos()
    {
        // 绘制墙壁检测区域
        Gizmos.color = Color.yellow;
        
        // 左右两边的检测框
        Vector2 rightCheck = (Vector2)transform.position + Vector2.right * wallCheckDistance;
        Vector2 leftCheck = (Vector2)transform.position + Vector2.left * wallCheckDistance;
        
        // 绘制检测框
        Gizmos.DrawWireCube(rightCheck, wallCheckSize);
        Gizmos.DrawWireCube(leftCheck, wallCheckSize);
    }

    // 在Start或Awake中设置刚体属性
    private void SetupRigidbody()
    {
        if (rb != null)
        {
            // 移除所有摩擦力
            if (rb.sharedMaterial == null)
            {
                PhysicsMaterial2D material = new PhysicsMaterial2D("NoFriction");
                material.friction = 0f;
                material.bounciness = 0f;
                rb.sharedMaterial = material;
            }
            
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
            rb.drag = 0f;
            rb.angularDrag = 0f;
        }
    }

    /// <summary>
    /// 设置时间减速状态
    /// </summary>
    public void SetTimeSlowDown(bool slowDown, float factor)
    {
        isTimeSlowDown = slowDown;
        timeSlowDownFactor = factor;
    }
}
