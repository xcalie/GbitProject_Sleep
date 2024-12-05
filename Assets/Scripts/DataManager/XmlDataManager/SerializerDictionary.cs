using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class SerializerDictionary<Tkey, Tvalue> : Dictionary<Tkey, Tvalue>, IXmlSerializable
{
    public XmlSchema GetSchema()
    {
        return null;
    }

    //自定义字典的序列化规则
    public void ReadXml(XmlReader reader)
    {
        XmlSerializer keySer = new XmlSerializer(typeof(Tkey));
        XmlSerializer valueSer = new XmlSerializer(typeof(Tvalue));

        //跳过根节点
        reader.Read();

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            Tkey key = (Tkey)keySer.Deserialize(reader);
            Tvalue value = (Tvalue)valueSer.Deserialize(reader);
        }
    }

    //自定义字典的反序列化规则
    public void WriteXml(XmlWriter writer)
    {
        XmlSerializer keySer = new XmlSerializer(typeof(Tkey));
        XmlSerializer valueSer = new XmlSerializer(typeof(Tvalue));

        foreach (KeyValuePair<Tkey, Tvalue> kv in this)
        {
            //键值对序列化
            keySer.Serialize(writer, kv.Key);
            valueSer.Serialize(writer, kv.Value);
        }
    }
}
