using UnityEngine;

/// <summary>
/// 子弹类，用于处理子弹的行为和碰撞
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 2;  // 子弹速度
    [SerializeField]
    private float lifeTime = 3;  // 子弹生存时间
    [SerializeField]
    private float damage = 1f;  // 子弹造成的伤害
    [SerializeField]
    private bool isPlayerBullet = true;  // 是否为玩家发射的子弹
    [SerializeField]
    private float bulletKnockbackForce = 5f;  // 子弹击退力度
    private Vector2 direction;  // 子弹方向
    private AudioSource ASForDestroy;  // 子弹销毁音效
    private bool ignoreTimeScale = false;  // 是否忽略时间缩放

    // 更新属性设置方法
    public void SetBulletProperties(bool isPlayer, float newSpeed = 2f, float newDamage = 1f, float newLifeTime = 3f)
    {
        isPlayerBullet = isPlayer;
        speed = newSpeed;
        damage = newDamage;
        lifeTime = newLifeTime;
        // 玩家子弹忽略时间缩放
        ignoreTimeScale = isPlayer;
    }

    private void Start()
    {
        // 获取或添加刚体组件
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 设置刚体属性
        rb.gravityScale = 0f;         // 无重力
        rb.mass = 0.0001f;           // 极小的质量
        rb.drag = 0f;                // 无阻力
        rb.angularDrag = 0f;         // 无角阻力
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 连续碰撞检测
        
        // 确保子弹使用触发器而不是普通碰撞器
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;  // 设置为触发器
        }

        // 如果是玩家子弹，设置销毁时使用未缩放时间
        if (ignoreTimeScale)
        {
            Destroy(gameObject, lifeTime * Time.timeScale);  // 补偿时间缩放
        }
        else
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public void Initialize(Vector2 dir)
    {
        direction = dir;
    }

    private void Update()
    {
        // 根据是否忽略时间缩放使用不同的 deltaTime
        float deltaTime = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.position += (Vector3)(direction * speed * deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"子弹碰撞到: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, isPlayerBullet: {isPlayerBullet}");
        
        // 如果碰到地面，销毁子弹
        if (collision.CompareTag("Ground"))
        {
            Debug.Log("子弹碰到地面，销毁");
            DestroyBullet();
            return;
        }

        // 如果是Boss的子弹击中玩家
        if (!isPlayerBullet && collision.CompareTag("Player"))
        {
            Debug.Log("Boss子弹击中玩家");
            MainControl.Instance.TakeDamage(damage, transform.position, bulletKnockbackForce);
            DestroyBullet();
            return;
        }

        // 如果是玩家的子弹击中Boss
        if (isPlayerBullet && collision.CompareTag("Boss"))
        {
            Debug.Log("玩家子弹击中Boss");
            Boss boss = collision.gameObject.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Debug.Log($"对Boss造成{damage}点伤害");
            }
            else
            {
                Debug.LogError("找不到Boss组件！");
            }
            DestroyBullet();
            return;
        }
    }

    private void DestroyBullet()
    {
        // 播放子弹销毁音效
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForDestroy, MusicType.Shoot, 1, false, MusicControl.Instance.AUDIOVolume);
        Destroy(gameObject);
    }
}
