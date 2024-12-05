using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class jita : MonoBehaviour
{
    public GameObject speeksome;
    public TMP_Text Speeksometext;
    public string speeksomeText1;
    public string speeksomeText2;
    public string speeksomeText3;
    public string speeksomeText4;
    private bool isPlayerinjita;
    private int currentText;
    [SerializeField]
    private PartialTypewriter typewriter;
    // Start is called before the first frame update
    void Start()
    {
        currentText = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerinjita)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                speeksome.SetActive(true);
                if (typewriter != null)
                {
                    typewriter.OutputText(GetSpeeksomeText());
                }
                else
                {
                    Debug.LogError("Typewriter component is not assigned!");
                    Speeksometext.text = GetSpeeksomeText();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space)) // 检测E键按下
            {
                NextSpeeksomeText(); // 切换到下一条文本
            }
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ( other. gameObject. CompareTag("Player")
             && other. GetType(). ToString() == "UnityEngine. CapsuleCollider2D")
        {
            isPlayerinjita = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if ( other. gameObject. CompareTag("Player")
             && other. GetType(). ToString() == "UnityEngine. CapsuleCollider2D")
        {
            isPlayerinjita = false;
            speeksome.SetActive(false);
        }
    }
    string GetSpeeksomeText()
    {
        // 根据currentTextIndex返回对应的文本
        switch (currentText)
        {
            case 0: return speeksomeText1;
            case 1: return speeksomeText2;
            case 2: return speeksomeText3;
            case 3: return speeksomeText4;
            default: return "";
        }
    }

    void NextSpeeksomeText()
    {
        // 切换到下一条文本
        currentText = (currentText + 1) % 4; // 使用模运算确保索引不会超出范围
        Speeksometext.text = GetSpeeksomeText();
    }
}
