using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// 梦魇怪物类 - 继承自MonsterBase
/// </summary>
public class Nightmare : MonsterBase
{
    [Header("子弹设置")]
    [SerializeField] private GameObject bulletPrefab;        // 子弹预制体

    [Header("巡逻设置")]
    [SerializeField] private float patrolRange = 5f;        // 巡逻范围
    [SerializeField] private float detectionRange = 10f;    // 玩家检测范围
    [SerializeField] private float attackRange = 8f;        // 攻击范围
    [SerializeField] private float shootCooldown = 2f;      // 射击冷却时间
    
    private Vector2 startPosition;                          // 初始位置
    private float currentPatrolTarget;                      // 当前巡逻目标点
    private float lastShootTime;                            // 上次射击时间
    private Transform player;                               // 玩家Transform引用
    
    [Header("Gizmos 调试设置")]
    [SerializeField] private Color patrolRangeColor = new Color(0.8f, 0.8f, 0.2f, 0.3f);    // 巡逻范围颜色
    [SerializeField] private Color detectionRangeColor = new Color(0.2f, 0.8f, 0.2f, 0.3f); // 检测范围颜色
    [SerializeField] private Color attackRangeColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);    // 攻击范围颜色
    
    private MonsterAnimationManager animationManager; // 动画管理器引用
    
    // 添加新的字段来追踪动画状态
    private bool isAttacking = false;
    
    protected override void Awake()
    {
        base.Awake();
        // 设置怪物ID为0（梦魇）
        monsterId = 0;
        // 记录初始位置
        startPosition = transform.position;
        // 获取玩家引用
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 初始化动画管理器
        animationManager = GetComponent<MonsterAnimationManager>();
    }

    /// <summary>
    /// 实现怪物AI行为
    /// </summary>
    protected override void MonsterBehavior()
    {
        if (player == null || isAttacking) return;

        // 默认播放巡逻动画
        animationManager?.PlayPatrolAnimation();
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange)
        {
            ChasePlayer();
            if (distanceToPlayer < attackRange)
            {
                TryShoot();
            }
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// 巡逻行为
    /// </summary>
    private void Patrol()
    {
        // 计算巡逻目标点
        float targetX = startPosition.x + Mathf.PingPong(Time.time * moveSpeed, patrolRange * 2) - patrolRange;
        Vector2 targetPosition = new Vector2(targetX, transform.position.y);
        
        // 计算移动方向
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Move(direction);
    }

    /// <summary>
    /// 追击玩家
    /// </summary>
    private void ChasePlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Move(direction);
        }
    }

    /// <summary>
    /// 尝试射击
    /// </summary>
    private void TryShoot()
    {
        if (Time.time - lastShootTime >= shootCooldown && !isAttacking)
        {
            Attack();
            lastShootTime = Time.time;
        }
    }

    /// <summary>
    /// 实现攻击方法
    /// </summary>
    public override void Attack()
    {
        if (player == null || isAttacking) return;

        // 开始攻击协程
        StartCoroutine(AttackSequence());
    }

    /// <summary>
    /// 攻击序列协程
    /// </summary>
    private IEnumerator AttackSequence()
    {
        if (isAttacking) yield break;
        
        isAttacking = true;
        
        // 播放攻击动画并等待完成
        if (animationManager != null)
        {
            yield return StartCoroutine(animationManager.PlayAttackAnimationForced());
            
            // 执行攻击逻辑
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < 1.5f)
            {
                MainControl.Instance.TakeDamage(1, transform.position);
            }
            else
            {
                Vector2 shootDirection = (player.position - transform.position).normalized;
                Shoot(shootDirection);
            }
        }
        
        isAttacking = false;
        
        // 恢复巡逻动画
        animationManager?.PlayPatrolAnimation();
    }

    /// <summary>
    /// 射击方法
    /// </summary>
    private void Shoot(Vector2 direction)
    {
        if (bulletPrefab != null)
        {
            // 在梦魇位置实例化子弹
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            // 设置子弹方向和速度
            bullet.GetComponent<Bullet>().Initialize(direction);
        }
    }

    /// <summary>
    /// 处理碰撞
    /// </summary>
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (collision.gameObject.CompareTag("Player"))
        {
            // 检查时间流速
            if (Time.timeScale < 0.1f)
            {
                // 时间减速状态下，直接击杀怪物
                Die();
                return;
            }

            Vector2 normal = collision.GetContact(0).normal;
            Vector2 contactPoint = collision.GetContact(0).point;
            
            bool isStomped = normal.y < -0.7f && 
                            contactPoint.y > transform.position.y + (GetComponent<Collider2D>().bounds.size.y * 0.3f);
            
            if (isStomped)
            {
                Die();
            }
            else 
            {
                // 如果不是踩踏，对玩家造成伤害
                MainControl.Instance.TakeDamage(1, transform.position);
                
                // 如果怪物不在攻击状态，则进行攻击
                if (!isAttacking)
                {
                    Attack();
                }
            }
        }
    }

    /// <summary>
    /// 在Scene视图中绘制调试范围
    /// </summary>
    private void OnDrawGizmos()
    {
        // 如果在运行时，使用记录的初始位置；否则使用当前位置
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;

        // 绘制巡逻范围
        Gizmos.color = patrolRangeColor;
        // 绘制巡逻区域（矩形）
        Gizmos.DrawCube(new Vector3(center.x, center.y, 0), 
            new Vector3(patrolRange * 2, 1f, 0));

        // 绘制检测范围
        Gizmos.color = detectionRangeColor;
        // 绘制检测圆形范围
        DrawWireDisc(transform.position, detectionRange);

        // 绘制攻击范围
        Gizmos.color = attackRangeColor;
        // 绘制攻击圆形范围
        DrawWireDisc(transform.position, attackRange);

        // 如果在游戏运行时且玩家在检测范围内，绘制到目标的连线
        if (Application.isPlaying && player != null && 
            Vector2.Distance(transform.position, player.position) < detectionRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    /// <summary>
    /// 绘制圆形范围的辅助方法
    /// </summary>
    private void DrawWireDisc(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);

            Gizmos.DrawLine(point1, point2);
        }
    }
}
