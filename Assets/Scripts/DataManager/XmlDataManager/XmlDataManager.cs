using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XmlDataManager
{
    private static XmlDataManager instance = new XmlDataManager();

    public static XmlDataManager Instance => instance;

    private XmlDataManager()
    {
    }

    /// <summary>
    /// 保存数据到xml文件中
    /// </summary>
    /// <param name="data">存储对象</param>
    /// <param name="fileName">文件名</param>
    public void SaveData(object data, string fileName)
    {
        //1.得到存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".xml";

        //2.存储文件
        using (StreamWriter writer = new StreamWriter(path))
        {
            //3.序列化
            XmlSerializer Ser = new XmlSerializer(data.GetType());
            Ser.Serialize(writer, data);
        }
    }

    /// <summary>
    /// 从xml文件中加载数据
    /// </summary>
    /// <param name="type">对象类型</param>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public object LoadData(Type type, string fileName)
    {
        //1.首选判断文件是否存在
        string path = Application.persistentDataPath + "/" + fileName + ".xml";
        if (!File.Exists(path))
        {
            path = Application.streamingAssetsPath + "/" + fileName + ".xml";
            if (!File.Exists(path))
            {
                //如果更不不存在文件 两个路径都找过了
                //那么直接new 一个对象 返回给外部 无非 里面都是默认值
                return Activator.CreateInstance(type);
            }
        }

        //2.存在就读取文件
        using (StreamReader reader = new StreamReader(path))
        {
            //3.反序列化 取出数据
            XmlSerializer Ser = new XmlSerializer(type);
            return Ser.Deserialize(reader);
        }
    }

}
