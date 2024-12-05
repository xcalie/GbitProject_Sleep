using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public enum IntractionType
{
    Sugar,
    Guitar,
    OverBridge,
    Shard,
    BlackHole,
    CrystalBall,
    Triangle,
    Clock,
}


public class CircleInfo
{
    public float radius;
    public Vector2 offset;
    public CircleInfo()
    {
        radius = 2f;
        offset = Vector2.zero;
    }
    public CircleInfo(float radius, Vector2 offset)
    {
        this.radius = radius;
        this.offset = offset;
    }

}


//交互物品基类
public abstract class InteracitonBase : MonoBehaviour
{
    //交互物品类型
    public IntractionType type;

    [SerializeField]
    protected float InteractionDistance= 2f;//交互距离

    //触发器
    protected CircleCollider2D Circle2D;
    [SerializeField]
    protected Vector2 CircleOffset = Vector2.zero;

    [Header("交互物品图片")]
    [SerializeField]
    private SpriteRenderer Tex;
    private Color OldColor;

    //交换音乐
    protected AudioSource ASForInteration;
    //protected float volume = 0.8f;


    //确定按键是否按下
    //protected bool IsKeyDown = false;
    //标准是否进入交互范围
    [SerializeField]
    protected bool isKeyDown = false;


    public GameObject Speeksome;
    public TMP_Text Speeksometext;
    public string[] speeksomeText;
    public int currentText;
    [SerializeField]
    //private PartialTypewriter typewriter;
    public bool isTextShow;


    protected virtual void Start()
    {
        //获取圆形信息
        CircleOffset = MainControl.Instance.IntractionInfo[type].offset;
        InteractionDistance = MainControl.Instance.IntractionInfo[type].radius;

        //创建触发器
        CreateTrigger();
        //设置图片
        Tex = gameObject.GetComponent<SpriteRenderer>();
        OldColor = Tex.color;

        //初始化文本
        Speeksome = GameObject.Find("GuitarInteration");
        Speeksometext = Speeksome.GetComponent<TMP_Text>();
        currentText = 0;
    }

    //后继加入管理器
    protected virtual void Update()
    {
        ShowImage(type);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isKeyDown = true;
            Shake();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isKeyDown = false;
            isTextShow = false;
            Speeksome.SetActive(false);
            currentText = 0;
            Speeksometext.text = "";
            ResetShake();
        }
    }


    protected virtual void Interact()
    {
        if (!isTextShow)
        {
            Debug.Log("文本启用");
            Speeksome.SetActive(true);
            Speeksometext.text = GetSpeeksomeText();
            isTextShow = true;
        }
        else
        {
            NextSpeeksomeText();
        }
    }



    //处于触发器时闪烁
    protected virtual void Shake()
    {
        //这里放黄色透明图片
        //测试：绘制精灵图片
        Tex.color = new Color(255, 255, 0, 0.5f);
    }
    protected virtual void ResetShake()
    {
        Tex.color = OldColor;
    }



    //根据类型显示图片
    protected virtual void ShowImage(IntractionType type)
    {
        switch (type)
        {
            case IntractionType.Sugar:
                //这里放糖果图片
                break;
            case IntractionType.Guitar:
                //这里放吉他图片
                break;
            case IntractionType.OverBridge:
                //这里放天桥图片
                break;
            case IntractionType.Shard:
                //这里放碎片图片
                break;
            case IntractionType.BlackHole:
                //这里放黑洞图片
                break;
        }
    }

    //绘制触发器
    protected virtual void CreateTrigger()
    {
        Circle2D = this.GetComponent<CircleCollider2D>();
        if (Circle2D == null)
        {
            Circle2D = gameObject.AddComponent<CircleCollider2D>();
            Circle2D.isTrigger = true;
            Circle2D.radius = InteractionDistance;
            Circle2D.offset = CircleOffset;
        }
        Circle2D.isTrigger = true;
    }

    protected string GetSpeeksomeText()
    {
        // 根据currentTextIndex返回对应的文本
        if (currentText >= speeksomeText.Length)
        {
            return "";
        }
        return speeksomeText[currentText];
    }

    protected void NextSpeeksomeText()
    {
        if (speeksomeText.Length == 0) return;
        // 切换到下一条文本
        currentText = (currentText + 1) % speeksomeText.Length; // 使用模运算确保索引不会超出范围
        Speeksometext.text = GetSpeeksomeText();
    }
}
