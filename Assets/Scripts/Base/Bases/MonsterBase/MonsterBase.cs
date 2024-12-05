using UnityEngine;
using System.Collections;

/// <summary>
/// 怪物基类 - 提供基础移动功能，实现IAction接口
/// </summary>
public abstract class MonsterBase : MonoBehaviour, IAction
{
    #region 基础属性
    [Header("怪物ID")]
    [SerializeField] protected int monsterId;                // 怪物ID，对应MainControl中的数组索引
    
    [Header("基础属性")]
    protected float moveSpeed;                               // 从MainControl获取移动速度
    protected float jumpSpeed;                               // 从MainControl获取跳跃速度
    
    protected bool isDead = false;                           // 死亡状态
    protected Rigidbody2D rb;                               // 刚体组件
    protected Animator animator;                             // 动画控制器
    
    protected MotionState motionState = MotionState.Stand;   // 运动状态
    protected AddressState addressState = AddressState.ground; // 位置状态
    #endregion

    #region Unity生命周期
    protected virtual void Awake()
    {
        // 确保在基类中初始化rb
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // 从MainControl获取初始数据
        InitializeFromMainControl();
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            // 实现怪物AI行为
            MonsterBehavior();
        }
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 从MainControl获取初始数据
    /// </summary>
    protected virtual void InitializeFromMainControl()
    {
        if (MainControl.Instance != null)
        {
            moveSpeed = MainControl.Instance.MonsterMoveSpeed[monsterId];
            jumpSpeed = MainControl.Instance.MonsterJumpSpeed[monsterId];
        }
    }
    #endregion

    #region IAction接口实现
    public virtual void MoveComponent()
    {
        // 具体移动逻辑由子类实现
    }

    public virtual void Attack()
    {
        // 具体攻击逻辑由子类实现
    }

    public virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 禁用碰撞体和刚体
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;
        if (rb != null)
            rb.simulated = false;

        // 添加镜头抖动效果
        StartCoroutine(CameraShakeEffect(0.05f, 0.1f)); // 进一步减小相机抖动的持续时间和幅度,持续时间从0.1秒减至0.05秒,幅度从0.15减至0.1

        // 直接开始淡出效果
        StartCoroutine(FadeOutAndDestroy());
    }

    // 添加新的淡出协程
    private IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 初始化透明度
            Color originalColor = spriteRenderer.color;
            float fadeTime = 0.5f; // 缩短淡出时间为0.5秒
            float elapsedTime = 0f;

            // 逐渐降低透明度
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime; // 使用不受时间缩放影响的时间
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // 销毁物体
        Destroy(gameObject);
    }

    // 添加相机抖动协程
    private IEnumerator CameraShakeEffect(float duration, float magnitude)
    {
        // 获取主相机和初始位置
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;
        
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            
            // 计算衰减系数
            float damper = 1.0f - Mathf.Clamp01(elapsed / duration);
            
            // 生成随机偏移
            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;
            
            // 应用偏移
            mainCamera.transform.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            yield return null;
        }

        // 恢复相机位置
        mainCamera.transform.localPosition = originalPos;
    }
    #endregion

    #region 核心方法
    /// <summary>
    /// 怪物AI行为控制,需要子类实现具体逻辑
    /// </summary>
    protected abstract void MonsterBehavior();

    /// <summary>
    /// 基础移动方法
    /// </summary>
    protected virtual void Move(Vector2 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * moveSpeed;
            
            // 更新朝向 - 使用旋转而不是Scale
            if (direction.x != 0)
            {
                // 当向左移动时旋转180度，向右移动时旋转0度
                float yRotation = direction.x < 0 ? 180f : 0f;
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            }
            
            // 更新运动状态
            motionState = direction.magnitude > 0 ? MotionState.Run : MotionState.Stand;
            
            // 更新动画参数
            if (animator != null)
            {
                animator.SetInteger("MotionState", (int)motionState);
                animator.SetInteger("AddressState", (int)addressState);
            }
        }
    }

    /// <summary>
    /// 跳跃方法
    /// </summary>
    protected virtual void Jump(Vector2 direction)
    {
        if (rb != null && addressState == AddressState.ground)
        {
            // 保持当前的水平速度，只添加垂直跳跃力
            Vector2 jumpForce = rb.velocity;  // 保持现有速度
            
            // 设置水平速度（如果需要改变）
            jumpForce.x = direction.x * MainControl.Instance.MaxMoveSpeed * 0.3f;
            
            // 只设置垂直速度，让重力自然影响下落
            jumpForce.y = MainControl.Instance.JumpForce * 0.3f;
            
            // 限制最大速度
            jumpForce.x = Mathf.Clamp(jumpForce.x, -5f, 5f);
            jumpForce.y = Mathf.Clamp(jumpForce.y, 0f, 5f);
            
            Debug.Log($"Applying jump force: {jumpForce}");
            rb.velocity = jumpForce;
            addressState = AddressState.air;
            
            if (animator != null)
            {
                animator.SetInteger("AddressState", (int)addressState);
            }
        }
        else
        {
            Debug.Log($"Jump failed - rb null: {rb == null}, addressState: {addressState}");
        }
    }
    #endregion

    #region 碰撞检测
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测地面碰撞
        if (collision.gameObject.CompareTag("Ground"))
        {
            addressState = AddressState.ground;
            if (animator != null)
            {
                animator.SetInteger("AddressState", (int)addressState);
            }
        }
    }
    #endregion
}
