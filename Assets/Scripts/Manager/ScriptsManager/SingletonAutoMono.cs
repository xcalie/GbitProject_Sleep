using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动挂载单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("尝试在编辑器模式下访问单例实例。");
                    return null;
                }

                //动态创建 动态挂载
                //在场景上创建一个空物体，然后挂载脚本
                GameObject obj = new GameObject();
                //通过得到T脚本的类名 在改名 可以在编辑器中明确的看到单例模式的脚本
                obj.name = typeof(T).ToString();
                //动态挂载 单例模式脚本
                instance = obj.AddComponent<T>();
                //过场景时不销毁 保证单例模式
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

}
