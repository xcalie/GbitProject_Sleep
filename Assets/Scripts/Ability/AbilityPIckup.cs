using UnityEngine;

/* 注释，交互获取能力道具
 *这里已经于另一个脚本实现了道具的交互
 *此处作为管理器调用
 */


// 能力拾取类,用于处理玩家拾取能力道具的逻辑
public class AbilityPickup : SingletonAutoMono<AbilityPickup>
{
    //激活标志
    public bool isActive = false;

    // 序列化字段,用于在Unity编辑器中设置该拾取物对应的能力类型
    //[SerializeField] private AbilityType abilityType;
    
    // 当其他碰撞体进入触发器时调用
    public void GetAbility(AbilityType abilityType, Collider2D other)
    {
        // 检查进入触发器的对象是否是玩家
        if (other.CompareTag("Player"))
        {
            // 获取玩家身上的能力管理器组件
            AbilityManager abilityManager = other.GetComponent<AbilityManager>();
            // 如果能力管理器存在
            if (abilityManager != null)
            {
                Debug.Log(other + "开始获取能力");
                // 解锁对应类型的能力
                abilityManager.UnlockAbility(abilityType);
                // 销毁当前能力道具
                //Destroy(gameObject);
            }
        }
    }
}
