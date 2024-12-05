using UnityEngine;
using System.Collections;

/// <summary>
/// 梦影怪物 - 一种在固定区域游荡并能对玩家造成伤害的幽灵
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterAnimationManager))]
public class DreamShadow : MonsterBase
{
    #region 状态机定义
    private enum DreamShadowState
    {
        Patrol,     // 巡逻状态
        Attack,     // 攻击状态
        Disappear,  // 消失状态
        Appear      // 出现状态
    }
    private DreamShadowState currentState = DreamShadowState.Patrol;
    [SerializeField] new private MonsterAnimationManager animator;
    #endregion

    #region 属性定义
    [Header("梦影特殊属性")]
    [SerializeField] private float patrolRadius = 5f;        // 游走半径
    [SerializeField] private Vector2 patrolDirection = Vector2.right; // 巡逻方向
    [SerializeField] private bool isHorizontalPatrol = true; // 是否水平巡逻
    [SerializeField] private float fadeSpeed = 2f;           // 淡入淡出速度
    [SerializeField] private int attackDamage = 1;           // 攻击伤害
    [SerializeField] private float disappearChance = 0.01f;  // 每帧消失的概率
    [SerializeField] private float attackCooldown = 1.5f;    // 攻击冷却时间
    [SerializeField] private float patrolAnimSpeed = 1f;    // 巡逻动画速度
    [SerializeField] private float attackAnimSpeed = 1.5f;  // 攻击动画速度

    private Vector2 startPosition;                           // 初始位置
    private Vector2 targetPosition;                          // 目标位置
    private SpriteRenderer spriteRenderer;                   // 精灵渲染器
    private bool isDisappearing = false;                     // 是否正在消失
    private float lastAttackTime;                            // 上次攻击时间
    private static readonly int StateHash = Animator.StringToHash("State");
    private bool isTransitioning = false;
    [Header("Movement Settings")]
    [SerializeField] new private float moveSpeed = 5f;  // 默认速度为5
    #endregion

    #region 初始化
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<MonsterAnimationManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // 设置动画事件监听
        animator.onAnimationComplete += OnAnimationComplete;
        
        // 确保 patrolDirection 是单位向量
        if (isHorizontalPatrol)
        {
            patrolDirection = Vector2.right;
        }
        else
        {
            patrolDirection = Vector2.up;
        }
        
        // 设置怪物ID为0（梦影）
        monsterId = 0;
        // 记录初始位置
        startPosition = transform.position;
        // 初始化目标位置
        targetPosition = startPosition;
    }

    private void OnEnable()
    {
        // 从 MainControl 获取初始速度
        if (MainControl.Instance != null)
        {
            moveSpeed = MainControl.Instance.MonsterMoveSpeed[1];
        }
        // 设置初始目标位置
        SetNewRandomTarget();
    }

    private void OnDisable()
    {
        // 清理状态
        isDisappearing = false;
        StopAllCoroutines();
    }

    private void Start()
    {
        // 原有的开始逻辑
        UpdateAnimationState(DreamShadowState.Patrol);
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (animator != null)
        {
            animator.onAnimationComplete -= OnAnimationComplete;
        }
    }
    #endregion

    #region 核心行为
    protected override void MonsterBehavior()
    {
        switch (currentState)
        {
            case DreamShadowState.Patrol:
                HandlePatrolState();
                break;
            case DreamShadowState.Attack:
                HandleAttackState();
                break;
            case DreamShadowState.Disappear:
            case DreamShadowState.Appear:
                // 这些状态由协程处理
                break;
        }
    }

    private void HandlePatrolState()
    {
        // 向目标位置移动
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 更新朝向
        UpdateFacing(direction);

        // 检查是否到达目标点
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewRandomTarget();
        }

        // 随机消失检查
        if (!isDisappearing && Random.value < disappearChance)
        {
            StartCoroutine(DisappearAndReappear());
        }
    }

    private void HandleAttackState()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            UpdateAnimationState(DreamShadowState.Patrol);
        }
    }

    private void UpdateAnimationState(DreamShadowState newState)
    {
        // 如果正在过渡中，不允许新的状态切换
        if (isTransitioning && newState != DreamShadowState.Patrol)
            return;
        
        currentState = newState;
        isTransitioning = true;
        
        switch (newState)
        {
            case DreamShadowState.Patrol:
                isTransitioning = false;
                animator.PlayAnimation(MonsterAnimationManager.MonsterStates.PATROL);
                break;
                
            case DreamShadowState.Attack:
                animator.PlayAnimation(MonsterAnimationManager.MonsterStates.ATTACK, true);
                break;
                
            case DreamShadowState.Disappear:
                animator.PlayAnimation("Disappear");
                break;
                
            case DreamShadowState.Appear:
                animator.PlayAnimation("Appear");
                break;
        }
    }

    private void OnAnimationComplete(string stateName)
    {
        isTransitioning = false;
        
        switch (stateName)
        {
            case "Attack":
                StartCoroutine(AttackAndDisappear());
                break;
            case "Disappear":
                UpdateAnimationState(DreamShadowState.Appear);
                break;
            case "Appear":
                UpdateAnimationState(DreamShadowState.Patrol);
                break;
        }
    }

    public override void Attack()
    {
        // 检查攻击冷却
        if (Time.time - lastAttackTime < attackCooldown || isDisappearing)
            return;

        // 播放攻击动画，并在动画结束后立即消失
        UpdateAnimationState(DreamShadowState.Attack);
        
        // 造成伤害并触发击退效果
        MainControl.Instance.TakeDamage(attackDamage, transform.position);
        lastAttackTime = Time.time;

        // 修改动画状态切换逻辑
        StartCoroutine(AttackAndDisappear());
    }

    // 新增一个协程来处理攻击后立即消失的逻辑
    private IEnumerator AttackAndDisappear()
    {
        // 等待一小段时间让攻击动画播放（可以调整这个时间）
        yield return new WaitForSeconds(0.2f);
        
        // 直接开始消失效果
        StartCoroutine(DisappearAndReappear());
    }
    #endregion

    #region 碰撞检测
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Attack();
        }
    }
    #endregion

    #region 特效相关
    private IEnumerator DisappearAndReappear()
    {
        isDisappearing = true;
        UpdateAnimationState(DreamShadowState.Disappear);

        // 淡出
        yield return StartCoroutine(FadeOut());

        // 重新定位
        SetNewRandomTarget();
        transform.position = targetPosition;

        UpdateAnimationState(DreamShadowState.Appear);
        // 淡入
        yield return StartCoroutine(FadeIn());

        isDisappearing = false;
        UpdateAnimationState(DreamShadowState.Patrol);
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float alpha = 0f;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 设置新的随机目标位置
    /// </summary>
    private void SetNewRandomTarget()
    {
        if (isHorizontalPatrol)
        {
            // 计算当前位置相对于起始点的偏移
            float currentOffset = transform.position.x - startPosition.x;
            
            // 如果接近或超出巡逻范围，则改变方向
            if (Mathf.Abs(currentOffset) >= patrolRadius * 0.9f)
            {
                patrolDirection = new Vector2(-Mathf.Sign(currentOffset), 0);
            }
            
            // 在当前方向上设置新的目标点
            float newX = transform.position.x + patrolDirection.x * Random.Range(2f, 4f);
            // 确保不会超出巡逻范围
            newX = Mathf.Clamp(newX, startPosition.x - patrolRadius, startPosition.x + patrolRadius);
            targetPosition = new Vector2(newX, startPosition.y);
        }
        else
        {
            // 垂直巡逻的逻辑
            float currentOffset = transform.position.y - startPosition.y;
            
            if (Mathf.Abs(currentOffset) >= patrolRadius * 0.9f)
            {
                patrolDirection = new Vector2(0, -Mathf.Sign(currentOffset));
            }
            
            float newY = transform.position.y + patrolDirection.y * Random.Range(2f, 4f);
            newY = Mathf.Clamp(newY, startPosition.y - patrolRadius, startPosition.y + patrolRadius);
            targetPosition = new Vector2(startPosition.x, newY);
        }
    }
    #endregion

    #region Gizmos绘制
    private void OnDrawGizmosSelected()
    {
        // 修改Gizmos显示为直线巡逻路径
        Gizmos.color = Color.cyan;
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(startPos, patrolRadius);
        
        // 绘制巡逻路线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos - (Vector3)(patrolDirection * patrolRadius), 
                       startPos + (Vector3)(patrolDirection * patrolRadius));
    }
    #endregion

    private void OnAttackAnimationHit()
    {
        // 在攻击动画的特定帧触发伤害
        MainControl.Instance.TakeDamage(attackDamage, transform.position);
    }

    // 修改UpdateFacing方法，确保在改变朝向时保持缩放
    private void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            // 根据移动方向设置旋转
            // 向右移动时角度为0，向左移动时角度为180
            float angle = direction.x > 0 ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
