using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// PlayPrefs数据管理类 统一管理数据的存储和读取
/// </summary>
public class PlayerPrefsDataManager
{
    private static PlayerPrefsDataManager instance = new PlayerPrefsDataManager();

    public static PlayerPrefsDataManager Instance
    {
        get
        {
            return instance;
        }
    }

    private PlayerPrefsDataManager()
    {

    }

    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="data">数据对象</param>
    /// <param name="keyName">数据对象的唯一key 自己控制</param>
    public void SaveData(object data, string keyName)
    {
        // 通过Type得到传入数据的对象所有的
        // PlayerPrefs的方法进行存储

        #region 得到所有字段

        Type dataType = data.GetType();
        // 得到所有字段
        FieldInfo[] infos = dataType.GetFields();
        //for (int i = 0; i < infos.Length; i++)
        //{
        //    Debug.Log(infos[i]);
        //}

        #endregion

        #region 自己定义一个key

        //存储通过PlayerPrefs来进行存储
        //保证key的唯一性 需要自己定key的规则

        // keyName_数据类型_字段类型_字段名

        #endregion

        #region 遍历字段进行数据存储

        string saveKeyName = "";
        FieldInfo info;
        for (int i = 0; i < infos.Length; i++)
        {
            //对每一个字段进行存储
            // 得到具体的字段信息
            info = infos[i];
            // 字段的类型info.FieldType.Name
            // 字段的名字info.Name

            // 根据拼接规则进行key的生成
            // Player1_PlayerInfo_Int32_age
            // keyName_数据类型_字段类型_字段名
            saveKeyName = keyName + "_" + dataType.Name + "_" + info.FieldType.Name + "_" + info.Name;

            // 得到key之后按照规则通过PlayerPrefs来存储
            // 获取值info.GetValue(data)
            SaveValue(info.GetValue(data), saveKeyName);
            
        }

        #endregion

        //中途存储，防止闪退
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 分装存储数据的方法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="keyName"></param>
    private void SaveValue(object value, string keyName)
    {
        // 直接通过PlayerPrefs进行存储
        // 只支持3种类型 int float string
        Type fieldType = value.GetType();

        // 类型判断
        if (fieldType == typeof(int))
        {
            PlayerPrefs.SetInt(keyName, (int)value);
        }
        else if (fieldType == typeof(float))
        {
            PlayerPrefs.SetFloat(keyName, (float)value);
        }
        else if (fieldType == typeof(string))
        {
            PlayerPrefs.SetString(keyName, (string)value);
        }
        else if (fieldType == typeof(bool))
        {
            PlayerPrefs.SetInt(keyName, (bool)value? 1 : 0);
        }
        // 如何判断 泛型的类型
        // 通过反射 判断父子关系
        // 判断字段是不是List的子类
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            // 父类装子类
            IList list = value as IList;
            // 先存储数量
            int index = 0;
            PlayerPrefs.SetInt(keyName, list.Count);
            foreach ( object obj in list )
            {
                // 存储具体的值
                SaveValue(obj, keyName + index);
                ++index;
            }
        }
        // 判断是不是Dictionary类型 通过Dictionary 的父类来判断
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //父类装子类
            IDictionary dic = value as IDictionary;
            //先存字典长度
            PlayerPrefs.SetInt(keyName, dic.Count);
            //遍历存储Dic里面的具体值
            //用于区分 表示的 区分 key
            int index = 0;
            foreach (object key in dic.Keys)
            {
                SaveValue(key, keyName + "_key_" + index);
                SaveValue(dic[key], keyName + "_value_" + index);
                ++index;
            }
        }
        else
        {
            // 再嵌套一遍，可以解构类中类
            SaveData(value, keyName);
        }
    }


    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="type">想要读取数据的 数据类型</param>
    /// <param name="keyName">数据对象的唯一key 自己控制</param>
    /// <returns></returns>
    public object LoadData(Type type, string keyName)
    {
        // 不用object 对像传入 使用 Type传入
        // 主要目的是节约一行代码
        //假设要读取一个Player类型里的数据 如果是object 必须在外部new一个对象传入
        //现在有type的 只用传入 应该 Type typeof(Player) 然后 在内部动态创建一个对象并返回出来
        //达到了在外部少写一行的目的

        // 根据传入的类型 和 keyName
        // 依据数据的类型 存储数据时key的凭借来进行数据的获取和返回

        // 根据传入的Type创建应该对象 用于存储数据
        object data = Activator.CreateInstance(type);
        //要往new出来的对象中存储数据 填充数据
        //得到所有的字段
        FieldInfo[] infos = type.GetFields();
        // 用于拼接key的字符串
        string loadKeyName = "";
        // 用于存储单个对象信息的对象
        FieldInfo info;
        for ( int i = 0; i < infos.Length; i++ )
        {
            info = infos[i];
            // key的拼接规则 一定会save的规则一模一样
            loadKeyName = keyName + "_" + type.Name + "_" + info.FieldType.Name + "_" + info.Name;
            
            // 有key 就可以结合 PlayerPrefs来读取
            info.SetValue(data, LoadValue(info.FieldType, loadKeyName));
        }

        return data;
    }

    /// <summary>
    /// 得到单个数据的方法
    /// </summary>
    /// <param name="fieldType"></param>
    /// <param name="keyName"></param>
    /// <returns></returns>
    private object LoadValue(Type fieldType, string keyName)
    {
        // 根据字段类型 来判断用哪个API
        if (fieldType == typeof(int))
        {
            return PlayerPrefs.GetInt(keyName);
        }
        else if (fieldType == typeof(float))
        {
            return PlayerPrefs.GetFloat(keyName);
        }
        else if (fieldType == typeof(string))
        {
            return PlayerPrefs.GetString(keyName);
        }
        else if (fieldType == typeof(bool))
        {
            return PlayerPrefs.GetInt(keyName, 0 ) == 1 ? true : false;
        }
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            // 得到数量
            int count = PlayerPrefs.GetInt(keyName, 0);
            // 实例化
            IList list = Activator.CreateInstance(fieldType) as IList;
            for ( int i = 0; i < count; i++ )
            {
                // 目的是得到list中泛型的类型
                list.Add(LoadValue(fieldType.GetGenericArguments()[0], keyName + i));
            }
            return list;
        }
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            // 得到字典的长度
            int count = PlayerPrefs.GetInt(keyName, 0);
            // 实例化字典
            IDictionary dic = Activator.CreateInstance(fieldType) as IDictionary;
            Type[] kvType = fieldType.GetGenericArguments();
            for ( int i = 0; i < count; i++)
            {
                dic.Add(LoadValue(kvType[0], keyName + "_key_" + i), LoadValue(kvType[1], keyName + "_value_" + i));
            }
            return dic;
        }
        else
        {
            return LoadData(fieldType, keyName);
        }
    }
}
