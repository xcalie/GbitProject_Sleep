using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 单例模式基类
/// </summary>
/// <typeparam name="T">类型</typeparam>
public abstract class BaseManager<T> where T : class//, new()
{
    private static T instance;

    //用于加锁得到对象
    protected static readonly object lockObj = new object();

    //属性的方式
    public static T Instance
    {
        get
        {
            //已经实例化过了就直接返回
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //instance = Activator.CreateInstance<T>();
                        //利用反射来得到无参私有的构造函数 来用于对象的实例化
                        Type type = typeof(T);
                        ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                        //参数依次是
                        //BindingFlags.Instance 说明是实例化的构造函数
                        //BindingFlags.NonPublic 说明是私有的构造函数
                        //null 说明没有参数
                        //Type.EmptyTypes 说明是无参构造函数
                        //null 说明没有参数
                        if (info != null)
                        {
                            instance = info.Invoke(null) as T;
                        }
                        else
                        {
                            Debug.LogError("没有得到对应的私有无参构造函数");
                        }
                    }
                }
            }
            return instance;
        }
    }

    //方法的方式
    //public static T GetInstance()
    //{
    //    if (instance == null)
    //    {
    //        instance = Activator.CreateInstance<T>();
    //    }
    //    return instance;
    //}
}
