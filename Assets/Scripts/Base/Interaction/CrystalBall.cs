using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBall : InteracitonBase
{
    //进入交互范围的东西
    private Collider2D other;

    protected override void Interact()
    {
        base.Interact();
        Debug.Log("CrystalBall");
        Debug.Log(other);
        if (other == null)
        {
            Debug.LogError("other is null");
            return;
        }
        //调用能力拾取类的GetAbility方法
        AbilityPickup.Instance.GetAbility(AbilityType.AntiGravity, other);
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);

        //Invoke("Disable", 0.1f);
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.CrystalBall;
    }


    protected override void Start()
    {
        base.Start();
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

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (other.CompareTag("Player"))
        {
            this.other = other;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        bool isTShow = isTextShow;
        base.OnTriggerExit2D(other);
        if (other.CompareTag("Player"))
        {
            this.other = null;
        }
        if (other.CompareTag("Player") && isTShow)
        {
            Destroy(gameObject);
        }
    }

    //private void Disable()
    //{
    //    this.gameObject.SetActive(false);
    //}
}
