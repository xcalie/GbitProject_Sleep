using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverBridge : InteracitonBase
{
    public Transform Target;

    private Collider2D collsion;

    protected override void Interact()
    {
        base.Interact();
        Debug.Log("OverBridge");
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
        //这里放动画

        //放完后传送到目的地
        collsion.transform.position = Target.position;
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.OverBridge;
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
            collsion = other;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (other.CompareTag("Player"))
        {
            collsion = null;
        }
    }
}