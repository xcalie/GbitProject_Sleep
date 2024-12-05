using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Guitar : InteracitonBase
{
    protected override void Interact()
    {
        base.Interact();
        Debug.Log("Guitar");
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
        //这里放UI文本，放音乐
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.Guitar;
    }


    protected override void Start()
    {
        base.Start();
        currentText = 0;
    }


    protected override void Update()
    {
        base.Update();
        if (isKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

}
