using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// 序列化或者反序列化使用的是哪 种方式
/// </summary>
public enum JsonType
{
    JsonUtility,
    LitJson,
}

/// <summary>
/// Json数据管理类 主要用数据的序列化和反序列化
/// </summary>
public class JsonDataManager
{
    private static JsonDataManager instance = new JsonDataManager();
    public static JsonDataManager Instance => instance;
    private JsonDataManager() { }

    public void SaveData(object data, string fileName, JsonType type = JsonType.LitJson)
    {
        //确定存储路径
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtility:
                jsonStr = JsonUtility.ToJson(data);
                File.WriteAllText(path, jsonStr);
                break;
            case JsonType.LitJson:
                jsonStr = LitJson.JsonMapper.ToJson(data);
                File.WriteAllText(path, jsonStr);
                break;
        }
    }

    public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
    {
        //确定存储路径
        //判断默认路径下是否存在文件,不存在则从StreamingAssets文件夹中读取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }
        //都不存在就返回默认对象
        if (!File.Exists(path))
        {
            return default(T);
        }
        //反序列化
        string JsonStr = File.ReadAllText(path);
        //返回出去
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtility:
                data = JsonUtility.FromJson<T>(JsonStr);
                break;
            case JsonType.LitJson:
                data = LitJson.JsonMapper.ToObject<T>(JsonStr);
                break;
        }

        return data;
    }
}
