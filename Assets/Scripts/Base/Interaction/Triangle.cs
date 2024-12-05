using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : InteracitonBase
{
    //判断一次触碰是否结束
    [SerializeField]
    private bool isHurt = false;

    protected override void Interact()
    {
        Debug.Log("Triangle");
        //MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, volume);
        MainControl.Instance.TakeDamage(1f, transform.position);
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.Triangle;
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
            if (isHurt)
            {
                isHurt = false;
                Interact();
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player"))
        {
            isHurt = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        if (collision.CompareTag("Player"))
        {
            isHurt = false;
        }
    }
}
