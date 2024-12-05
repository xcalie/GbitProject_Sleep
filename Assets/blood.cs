using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class blood : MonoBehaviour
{
    [SerializeField]private Image[] healthImages;

    public void UpdateHealthImages(bool isBlood)
    {
        if (MainControl.Instance.PlayerHP < 0)
        {
            //这里播放死亡动画
            return;
        }
        if (isBlood)
        {
            healthImages[MainControl.Instance.PlayerHP - 1].gameObject.SetActive(true);
        }
        else
        {
            healthImages[MainControl.Instance.PlayerHP].gameObject.SetActive(false);
        }
        //for (int i = 0; i<healthImages.Length ; i++)
        //{
        //    if (i < MainControl.Instance.PlayerHP - 1)
        //    {
        //    }
        //}
    }
}
