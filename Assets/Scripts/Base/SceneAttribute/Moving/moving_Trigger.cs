using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moving_Trigger : MonoBehaviour
{
    //指定跟随目标
    public Transform Target;

    private void Start()
    {
        this.transform.localScale = Target.localScale;
        MonoManager.Instance.AddLateUpdateListener(OnAnimatorMove);
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 记录所有碰撞对象的信息
        Debug.Log($"碰撞对象信息: 名称={collision.gameObject.name}, 标签={collision.tag}, 层级={collision.gameObject.layer}");
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"玩家进入移动平台 - 时间状态: {Time.timeScale}");
            
            if (Time.timeScale >= 1.0f)
            {
                collision.transform.SetParent(this.transform);
                Debug.Log("玩家已设置为平台子对象");
            }
        }
        else
        {
            // 记录非玩家对象的碰撞
            Debug.Log($"非玩家对象触发平台: {collision.gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 记录所有离开碰撞的对象信息
        Debug.Log($"离开碰撞对象信息: 名称={collision.gameObject.name}, 标签={collision.tag}");
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"玩家离开移动平台 - 时间状态: {Time.timeScale}");
            
            if (Time.timeScale >= 1.0f)
            {
                collision.transform.SetParent(null);
                Debug.Log("已解除玩家与平台的父子关系");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.timeScale < 1.0f)
            {
                // 添加调试日志，记录时间减缓状态下的处理
                Debug.Log($"时间减缓状态 - 解除父子关系 - 时间状态: {Time.timeScale}");
                collision.transform.SetParent(null);
            }
            else if (collision.transform.parent != this.transform)
            {
                // 添加调试日志，记录重新建立父子关系
                Debug.Log("时间恢复正常 - 重新建立父子关系");
                collision.transform.SetParent(this.transform);
            }
        }
    }

    private void OnAnimatorMove()
    {
        this.transform.position = Target.position;
    }

    private void OnDestroy()
    {
        MonoManager.Instance.RemoveLateUpdateListener(OnAnimatorMove);
    }
}
