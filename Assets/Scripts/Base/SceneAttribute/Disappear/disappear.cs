using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 控制物体消失和重建的类
/// </summary>
public class Disappear : MonoBehaviour
{
    [Header("时间设置")]
    [SerializeField] private float destroyTime = 3.0f;    // 物体消失所需时间
    [SerializeField] private float rebuildTime = 5.0f;    // 物体重建所需时间
    
    private Coroutine disappearRoutine;    // 当前运行的消失协程
    private bool isDisappearing;           // 是否正在消失

    // 添加静态管理器
    private static DisappearManager manager;
    
    private void Awake()
    {
        // 确保场景中有管理器
        if (manager == null)
        {
            // 创建一个新的游戏对象作为管理器
            GameObject managerObj = new GameObject("DisappearManager");
            // 添加DisappearManager组件
            manager = managerObj.AddComponent<DisappearManager>();
            // 确保管理器在场景切换时不被销毁
            DontDestroyOnLoad(managerObj);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否可以触发消失效果
        if (!CanTriggerDisappear(collision))
            return;

        // 触发消失效果
        TriggerDisappear();
    }

    /// <summary>
    /// 检查是否可以触发消失效果
    /// </summary>
    private bool CanTriggerDisappear(Collision2D collision)
    {
        // 只检查是否是玩家以及是否正在消失
        if (isDisappearing || !collision.gameObject.CompareTag("Player"))
            return false;

        // 获取碰撞点的信息
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        
        // 检查是否是垂直碰撞（上或下）
        return Mathf.Abs(normal.y) >= 0.9f;
    }

    /// <summary>
    /// 触发消失效果
    /// </summary>
    public void TriggerDisappear()
    {
        // 检查对象是否有效
        if (this == null || !gameObject.activeInHierarchy)
            return;

        // 停止之前的消失协程（如果有）
        if (disappearRoutine != null)
        {
            StopCoroutine(disappearRoutine);
            disappearRoutine = null;
        }
        // 启动新的消失协程
        disappearRoutine = StartCoroutine(DisappearAndRebuildRoutine());
    }

    private IEnumerator DisappearAndRebuildRoutine()
    {
        try 
        {
            isDisappearing = true;
            
            // 等待destroyTime秒
            yield return new WaitForSeconds(destroyTime);
            
            // 通知管理器处理重建
            manager.ScheduleRebuild(this, rebuildTime);
            
            // 隐藏游戏对象
            gameObject.SetActive(false);
        }
        finally 
        {
            // 重置状态
            isDisappearing = false;
            disappearRoutine = null;
        }
    }
}

/// <summary>
/// 管理所有消失物体的重建
/// </summary>
public class DisappearManager : MonoBehaviour
{
    // 内部类，用于存储重建信息
    private class RebuildInfo
    {
        public Disappear target;     // 目标消失物体
        public float rebuildTime;    // 重建时间
        public float timer;          // 计时器

        public RebuildInfo(Disappear target, float rebuildTime)
        {
            this.target = target;
            this.rebuildTime = rebuildTime;
            this.timer = 0;
        }
    }

    // 待重建物体列表
    private List<RebuildInfo> pendingRebuilds = new List<RebuildInfo>();

    /// <summary>
    /// 安排一个物体的重建
    /// </summary>
    public void ScheduleRebuild(Disappear target, float delay)
    {
        pendingRebuilds.Add(new RebuildInfo(target, delay));
    }

    private void Update()
    {
        // 从后向前遍历，以便安全地移除元素
        for (int i = pendingRebuilds.Count - 1; i >= 0; i--)
        {
            var info = pendingRebuilds[i];
            // 更新计时器
            info.timer += Time.deltaTime;
            
            // 检查是否到达重建时间
            if (info.timer >= info.rebuildTime)
            {
                // 重新激活目标物体
                if (info.target != null && info.target.gameObject != null)
                {
                    info.target.gameObject.SetActive(true);
                }
                // 从列表中移除已重建的物体
                pendingRebuilds.RemoveAt(i);
            }
        }
    }
}
