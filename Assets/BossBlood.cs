using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossBlood : MonoBehaviour
{
    private RectTransform rectTransform;
    public GameObject Boss;
    private Boss boss;
    float OriginalSize;

    //获取血条的图片组件
    private void Start()
    {
        //获取血条的图片组件
        rectTransform = GetComponent<RectTransform>();
        boss = Boss.GetComponent<Boss>();
        OriginalSize = rectTransform.localScale.x;
    }

    private void Update()
    {
        if (boss.isActivated)
        {
            //实时更新Boss血量
            float value = boss.currentHealth / boss.maxHealth;
            rectTransform.localScale = new Vector3(value * OriginalSize, rectTransform.localScale.y, 1);
        }
    }
}
