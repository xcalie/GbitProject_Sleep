using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Boss类：控制Boss的行为、攻击模式和移动
public class Boss : MonoBehaviour
{
    [Header("组件引用")]
    private MonsterAnimationManager animationManager;  // 动画管理器引用
    
    // 基础属性
    [SerializeField] public float maxHealth = 50f;    // 增加boss最大血量到50
    public float currentHealth;                       // 当前血量
    [SerializeField] public float moveSpeed = 6f;     // 降低移动速度 (从10改为6)
    
    [Header("攻击参数")]
    [SerializeField] public float attackInterval = 1.5f;    // 缩短普通攻击间隔
    [SerializeField] public float attackRange = 12f;    // 减小攻击范围 (从30改为12)
    [SerializeField] public float preferredDistance = 5f;  // 将理想距离缩小到5
    private float attackTimer;                             // 攻击计时器
    
    // 计时器和触发条件
    [Header("特殊攻击设置")]
    [SerializeField] public float rotationAttackInterval = 15f;  // 旋转发射间隔时间（秒）
    private float rotationAttackTimer;                           // 旋转发射时器
    [SerializeField] public float healthThreshold = 4f;         // 回旋散射触发血量阈值
    private float lastHealthThreshold;                           // 上次触发回旋散射的血量
    
    // 移动相关属性
    private bool isMovingToPlayer;         // 是否正在向玩家移动的标志
    private bool isReturningToCenter;      // 是否正在返回中心的标志
    private Vector3 targetPosition;         // 目标移动位置
    private Vector3 centerPosition;         // 中心位置，Boss活动的参考点
    private float moveTimer;               // 移动计时器
    private bool wasMoving = false;        // 记录上一帧的移动状态
    
    [Header("移动设置")]
    [SerializeField] public float arenaWidth = 15f;     // 缩小活动区域宽度 (从40改为15)
    [SerializeField] public float arenaHeight = 12f;    // 缩小活动区域高度 (从30改为12)
    [SerializeField] public float repositionInterval = 4f;    
    [SerializeField] public float chasePlayerChance = 0.7f;  // 追踪玩家的概率(70%)
    private float repositionTimer;
    
    // 攻击状态枚举
    public enum AttackState
    {
        Idle,           
        Normal,         
        RotationAttack, 
        SpreadAttack,
        RageMode,       // 新增: 狂暴模式
        CrossAttack     // 新增: 十字攻击
    }
    [SerializeField] private AttackState currentState = AttackState.Normal;
    
    [Header("伤害参数")]
    [SerializeField] private float contactDamage = 1f;    // 改为float类型
    [SerializeField] private float knockbackForce = 10f;
    
    [Header("子弹参数")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;      // 增加子弹速度
    [SerializeField] private float bulletLifetime = 4f;    // 增加子弹存活时间
    [SerializeField] private float bulletDamage = 2f;      // 增加子弹伤害（改为float类型）
    
    private Player player;
    private bool isDead = false;
    
    [Header("瞬移设置")]
    [SerializeField] private float teleportCooldown = 8f;     // 瞬移冷却时
    [SerializeField] private float teleportRange = 8f;    // 减小瞬移范围 (从15改为8)
    [SerializeField] private float teleportChance = 0.3f;     // 瞬移触发概率
    private float teleportTimer;                              // 瞬移计时器
    [SerializeField] private GameObject teleportEffectPrefab;  // 瞬移特效预制体
    
    [Header("逃跑设置")]
    [SerializeField] private float escapeSpeed = 10f;        // 跑速度
    [SerializeField] private float escapeDuration = 1.5f;    // 逃跑持续时间
    [SerializeField] private float escapeChance = 0.4f;      // 逃跑触发概率
    private bool isEscaping = false;                         // 是否正在逃跑
    private Vector3 escapeDirection;                         // 逃跑方向


    [Header("空气墙对象")]
    [SerializeField] private GameObject airWallPrefab;

    [Header("激活设置")]
    [SerializeField] private float activationRange = 15f;  // 激活范围
    [SerializeField] public bool isActivated = false;     // 是否已激活
    [SerializeField] private float initialDelay = 1f;      // 激活后的初始延迟


    private void Start()
    {
        InitializeComponents();
        InitializeTimers();
        // 初始状态设为休息
        currentState = AttackState.Idle;
    }
    
    private void InitializeComponents()
    {
        animationManager = GetComponent<MonsterAnimationManager>();
        if (animationManager == null)
            Debug.LogError("找不到MonsterAnimationManager组件！");
            
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player == null)
            Debug.LogError("找不到玩家对象！");
            
        currentHealth = maxHealth;
        centerPosition = transform.position;
        lastHealthThreshold = maxHealth;
    }
    
    private void InitializeTimers()
    {
        attackTimer = attackInterval;
        rotationAttackTimer = rotationAttackInterval;
        repositionTimer = repositionInterval;
        teleportTimer = teleportCooldown;
    }
    
    private void Update()
    {
        if (isDead || player == null) return;
        
        // 检查是否需要激活
        if (!isActivated)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= activationRange)
            {
                StartCoroutine(ActivateBoss());
            }
            return; // 未激活时不执行其他逻辑
        }
        
        UpdateTimers();
        HandleMovement();
        HandleAttacks();
    }
    
    private void UpdateTimers()
    {
        if (currentState == AttackState.Idle)
        {
            HandleIdleState();
            return;
        }

        attackTimer -= Time.deltaTime;
        rotationAttackTimer -= Time.deltaTime;
        repositionTimer -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;
        
        // 检查是否触发特殊攻击
        if (rotationAttackTimer <= 0)
        {
            currentState = AttackState.RotationAttack;
            rotationAttackTimer = rotationAttackInterval;
        }
    }
    
    private void HandleIdleState()
    {
        if (currentState != AttackState.Idle) return;
        
        animationManager.PlayIdleAnimation();
        if (repositionTimer <= 0)
        {
            currentState = AttackState.Normal;
            repositionTimer = repositionInterval;
        }
    }
    
    private void HandleMovement()
    {
        if (player == null || currentState == AttackState.Idle || isEscaping || !isActivated) return;
        
        Vector3 oldPosition = transform.position;
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // 尝试触发瞬移
        if (teleportTimer <= 0 && Random.value < teleportChance)
        {
            PerformTeleport();
            teleportTimer = teleportCooldown;
            return;
        }
        
        // 更新瞬移计时器
        teleportTimer -= Time.deltaTime;
        
        // 在特殊击状态下回到中心点
        if (currentState == AttackState.SpreadAttack || currentState == AttackState.RotationAttack)
        {
            targetPosition = centerPosition;
        }
        // 正常移动逻辑
        else if (repositionTimer <= 0)
        {
            // 移除始终保持理想距离的逻辑，改为更随机的移动
            if (Random.value < chasePlayerChance)
            {
                // 70%概率在玩家周围随机位置移动
                Vector2 randomOffset = Random.insideUnitCircle * attackRange;
                targetPosition = player.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            }
            else
            {
                // 30%概率随机移动到竞技场内的任意位置
                targetPosition = new Vector3(
                    Random.Range(centerPosition.x - arenaWidth/2, centerPosition.x + arenaWidth/2),
                    Random.Range(centerPosition.y - arenaHeight/2, centerPosition.y + arenaHeight/2),
                    0
                );
            }
            repositionTimer = repositionInterval;
        }
        
        // 确保目标位置在矩形范围内
        targetPosition = ClampPositionToArena(targetPosition);
        
        // 使用MoveTowards平滑移动到目标位置
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // 更新动画状态
        bool isMoving = Vector3.Distance(transform.position, targetPosition) > 0.001f;
        if (isMoving != wasMoving)
        {
            if (isMoving) animationManager.PlayMoveAnimation();
            else animationManager.PlayIdleAnimation();
            wasMoving = isMoving;
        }
    }
    
    private Vector3 ClampPositionToArena(Vector3 position)
    {
        float halfWidth = arenaWidth / 2;
        float halfHeight = arenaHeight / 2;
        
        position.x = Mathf.Clamp(position.x, 
            centerPosition.x - halfWidth, 
            centerPosition.x + halfWidth);
        position.y = Mathf.Clamp(position.y, 
            centerPosition.y - halfHeight, 
            centerPosition.y + halfHeight);
        
        return position;
    }
    
    private void HandleAttacks()
    {
        if (player == null || currentState == AttackState.Idle || !isActivated) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // 血量低于30%时进入狂暴模式
        if (currentHealth / maxHealth < 0.3f && currentState != AttackState.RageMode)
        {
            currentState = AttackState.RageMode;
        }
        
        if (distanceToPlayer <= attackRange)
        {
            switch (currentState)
            {
                case AttackState.Normal:
                    PerformTrackingShot();
                    break;
                case AttackState.RotationAttack:
                    StartCoroutine(PerformRotationAttack());
                    break;
                case AttackState.SpreadAttack:
                    StartCoroutine(PerformSpreadAttack());
                    break;
                case AttackState.RageMode:
                    StartCoroutine(PerformRageMode());
                    break;
                case AttackState.CrossAttack:
                    StartCoroutine(PerformCrossAttack());
                    break;
            }
        }
    }
    
    // 追踪射击（普通攻击）
    private void PerformTrackingShot()
    {
        if (attackTimer <= 0)
        {
            // 计算朝向玩家的方向
            Vector2 direction = (player.transform.position - transform.position).normalized;
            animationManager.PlayAttackAnimation();
            ShootBullet(direction);
            attackTimer = attackInterval;
        }
    }
    
    // 旋转发射攻击
    private IEnumerator PerformRotationAttack()
    {
        currentState = AttackState.Idle;
        repositionTimer = repositionInterval;
        
        float rotationSpeed = 180f;  // 每秒旋转180度
        float duration = 4f;         // 持续4秒
        float shotInterval = 0.2f;   // 发射间隔
        float timer = 0f;
        float shotTimer = 0f;
        bool clockwise = Random.value > 0.5f;
        
        // 播放攻击动画
        animationManager.PlayAttackAnimation();
        
        while (timer < duration)
        {
            // 旋转Boss
            float rotation = (clockwise ? 1 : -1) * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotation);
            
            // 更新发射计时器
            shotTimer += Time.deltaTime;
            
            // 到达发射间隔时发射子弹
            if (shotTimer >= shotInterval)
            {
                // 向左两个方向发射子弹
                Vector2 rightDirection = transform.right;
                Vector2 leftDirection = -transform.right;
                
                ShootBullet(rightDirection);
                ShootBullet(leftDirection);
                
                // 重置发射计时器
                shotTimer = 0f;
                
                // 播放攻击动画
                animationManager.PlayAttackAnimation();
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 重置旋转
        transform.rotation = Quaternion.identity;
        currentState = AttackState.Normal;
    }
    
    // 散射攻击
    private IEnumerator PerformSpreadAttack()
    {
        currentState = AttackState.Idle;
        yield return StartCoroutine(animationManager.PlayAttackAnimationForced());
        
        int bulletCount = 12;  // 增加子弹数量到12
        float angleStep = 360f / bulletCount;
        
        // 发射两轮子弹，第二轮的角度错开
        for (int round = 0; round < 2; round++)
        {
            float startAngle = round * (angleStep / 2); // 第二轮错开一半角度
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + (i * angleStep);
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
                ShootBullet(direction);
            }
            yield return new WaitForSeconds(0.3f);
        }
        
        yield return new WaitForSeconds(0.5f);
        currentState = AttackState.Normal;
    }
    
    // 优化狂暴模式攻击
    private IEnumerator PerformRageMode()
    {
        // 在狂暴模式下适度提升属性
        float originalMoveSpeed = moveSpeed;
        float originalAttackInterval = attackInterval;
        moveSpeed *= 1.2f;         // 降低速度提升
        attackInterval *= 0.7f;    // 降低攻击频率提升
        
        // 减少攻击轮数，从3轮改为2轮
        for (int i = 0; i < 2; i++)
        {
            // 简化的散射攻击
            yield return StartCoroutine(PerformRageSpreadAttack());
            yield return new WaitForSeconds(0.8f); // 增加间隔时间
            
            // 简化的十字攻击
            yield return StartCoroutine(PerformRageCrossAttack());
            yield return new WaitForSeconds(0.8f);
        }
        
        // 恢复原始属性
        moveSpeed = originalMoveSpeed;
        attackInterval = originalAttackInterval;
        currentState = AttackState.Normal;
    }
    
    // 狂暴模式下的简化散射攻击
    private IEnumerator PerformRageSpreadAttack()
    {
        currentState = AttackState.Idle;
        yield return StartCoroutine(animationManager.PlayAttackAnimationForced());
        
        int bulletCount = 8;  // 减少子弹数量
        float angleStep = 360f / bulletCount;
        
        // 只发射一轮子弹
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            ShootBullet(direction);
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    // 狂暴模式下的简化十字攻击
    private IEnumerator PerformRageCrossAttack()
    {
        currentState = AttackState.Idle;
        animationManager.PlayAttackAnimation();
        
        // 只发射主要方向的子弹
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };
        
        // 只发射两轮
        for (int round = 0; round < 2; round++)
        {
            foreach (Vector2 dir in directions)
            {
                ShootBullet(dir);
                // 第二轮才添加斜向子弹
                if (round == 1)
                {
                    ShootBullet(Quaternion.Euler(0, 0, 45) * dir);
                }
            }
            yield return new WaitForSeconds(0.4f);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    // 添加新的十字攻击模式
    private IEnumerator PerformCrossAttack()
    {
        currentState = AttackState.Idle;
        animationManager.PlayAttackAnimation();
        
        // 发射十字形状的子弹
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };
        
        // 发射三轮十字形子弹
        for (int round = 0; round < 3; round++)
        {
            foreach (Vector2 dir in directions)
            {
                ShootBullet(dir);
                // 同时发射斜向子弹
                if (round > 0)
                {
                    ShootBullet(Quaternion.Euler(0, 0, 45) * dir);
                    ShootBullet(Quaternion.Euler(0, 0, -45) * dir);
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
        
        yield return new WaitForSeconds(0.5f);
        currentState = AttackState.Normal;
    }
    
    // 受到伤害的方法
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // 播放受伤动画
        if (animationManager != null)
        {
            animationManager.PlayHurtAnimation();
        }
        
        // 触发逃跑行为
        if (!isEscaping && Random.value < escapeChance)
        {
            StartCoroutine(Escape());
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }
    
    private void Die()
    {
        // 播放死亡动画
        if (animationManager != null)
        {
            animationManager.PlayDeathAnimation();
        }

        Destroy(airWallPrefab);

        // 延迟销毁对象
        Destroy(gameObject, 1f);
    }
    
    private void ShootBullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;
        
        // 创建子弹实例
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        
        // 计算子弹旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        
        // 获取并初始化子弹组件
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            // 设置子弹属性
            bullet.SetBulletProperties(
                isPlayer: false,
                newSpeed: bulletSpeed,
                newDamage: bulletDamage,
                newLifeTime: bulletLifetime
            );
            
            // 初始化子弹方
            bullet.Initialize(direction.normalized);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞对象是否是玩家，并且Boss没有死亡
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            // 计算击退方向
            Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
            
            // 对玩家造成接触伤害，并施加击退力
            MainControl.Instance.TakeDamage(contactDamage, transform.position, knockbackForce);
        }
    }
    
    // 修改验证参数的方法
    private void OnValidate()
    {
        // 验证攻击范围
        if (attackRange < preferredDistance)
        {
            attackRange = preferredDistance + 5f;
        }
        
        // 验证理想距离
        if (preferredDistance < 5f)
        {
            preferredDistance = 5f;
        }
    }
    
    // 添加新的瞬移方法
    private void PerformTeleport()
    {
        // 在瞬移范围内随机选择一个位置
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 teleportPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * teleportRange;
        
        // 确保瞬移位置在竞技场范围内
        teleportPosition = ClampPositionToArena(teleportPosition);
        
        // 播放瞬移特效（如果有的话）
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Instantiate(teleportEffectPrefab, teleportPosition, Quaternion.identity);
        }
        
        // 执行瞬移
        transform.position = teleportPosition;
        
        // 瞬移后立即执行攻击
        StartCoroutine(PerformTeleportAttack());
    }
    
    // 添加瞬移后的攻击方法
    private IEnumerator PerformTeleportAttack()
    {
        // 等待很短的时间让玩家反应
        yield return new WaitForSeconds(0.2f);
        
        // 朝玩家方向发射多个子弹
        if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
            
            // 发射3发子弹，呈扇形分布
            float spreadAngle = 30f; // 扇形角度
            for (int i = -1; i <= 1; i++)
            {
                Vector2 bulletDirection = Quaternion.Euler(0, 0, i * spreadAngle) * directionToPlayer;
                ShootBullet(bulletDirection);
            }
        }
    }
    
    // 添加逃跑协程
    private IEnumerator Escape()
    {
        isEscaping = true;
        float timer = 0f;
        
        // 计算逃跑方向（远离玩家）
        if (player != null)
        {
            escapeDirection = (transform.position - player.transform.position).normalized;
        }
        else
        {
            escapeDirection = Random.insideUnitCircle.normalized;
        }
        
        // 暂时提高移动速度
        float originalSpeed = moveSpeed;
        moveSpeed = escapeSpeed;
        
        while (timer < escapeDuration)
        {
            if (!isDead)
            {
                // 计算目标位置
                Vector3 targetPos = transform.position + escapeDirection * escapeSpeed * Time.deltaTime;
                // 确保不会逃出竞技场范围
                targetPos = ClampPositionToArena(targetPos);
                // 移动
                transform.position = targetPos;
                
                // 播放移动动画
                animationManager.PlayMoveAnimation();
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 恢复原始速度
        moveSpeed = originalSpeed;
        isEscaping = false;
    }
    
    // 新增激活协程
    private IEnumerator ActivateBoss()
    {
        isActivated = true;
        
        // 播放激活动画
        if (animationManager != null)
        {
            animationManager.PlayIdleAnimation();
        }
        
        // 等待初始延迟
        yield return new WaitForSeconds(initialDelay);
        
        // 切换到正常状态
        currentState = AttackState.Normal;
    }
}
