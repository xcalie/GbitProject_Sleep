using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextStage : MonoBehaviour
{
    public GameObject Speeksome;
    public TMP_Text Speeksometext;
    [Header("对话文本")]
    public string speeksomeText;
    public int currentText;


    private void Start()
    {
        currentText = 0;
        Speeksome = GameObject.Find("GuitarInteration");
        Speeksometext = Speeksome.GetComponentInChildren<TMP_Text>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (MainControl.Instance.numShard >= 3)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                //提示玩家需要收集碎片    
                Speeksome.SetActive(true);
                Speeksometext.text = speeksomeText;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Speeksometext.text = "";
            Speeksome.SetActive(false);
        }
    }
}
