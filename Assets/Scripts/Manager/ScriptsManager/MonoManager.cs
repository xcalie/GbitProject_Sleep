using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共Mono模块管理系统
/// </summary>
public class MonoManager : SingletonAutoMono<MonoManager>
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    /// <summary>
    /// 添加update帧更新监听函数
    /// </summary>
    /// <param name="updateFun"></param>
    public void AddUpdateListener(UnityAction updateFun)
    {
        updateEvent += updateFun;
    }

    /// <summary>
    /// 移除update帧更新监听函数
    /// </summary>
    /// <param name="updateFun"></param>
    public void RemoveUpdateListener(UnityAction updateFun)
    {
        updateEvent -= updateFun;
    }

    /// <summary>
    /// 添加FixedUpdate帧更新监听函数
    /// </summary>
    /// <param name="fixedUpdateFun"></param>
    public void AddFixedUpdateListener(UnityAction fixedUpdateFun)
    {
        fixedUpdateEvent += fixedUpdateFun;
    }

    /// <summary>
    /// 移除FixedUpdate帧更新监听函数
    /// </summary>
    /// <param name="fixedUpdateFun"></param>
    public void RemoveFixedUpdateListener(UnityAction fixedUpdateFun)
    {
        fixedUpdateEvent -= fixedUpdateFun;
    }

    /// <summary>
    /// 添加LateUpdate帧更新监听函数
    /// </summary>
    /// <param name="lateUpdateFun"></param>
    public void AddLateUpdateListener(UnityAction lateUpdateFun)
    {
        lateUpdateEvent += lateUpdateFun;
    }

    /// <summary>
    /// 移除LateUpdate帧更新监听函数
    /// </summary>
    /// <param name="lateUpdateFun"></param>
    public void RemoveLateUpdateListener(UnityAction lateUpdateFun)
    {
        lateUpdateEvent -= lateUpdateFun;
    }


    private void Update()
    {
        updateEvent?.Invoke();
    }

    private void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }
}
