using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moving : MonoBehaviour
{
    //对象起点
    public Transform StartPos;
    //对象终点
    public Transform EndPos;
    //移动速度
    public float Speed = 1;

    private void Start()
    {
        this.transform.position = StartPos.position;
        MonoManager.Instance.AddUpdateListener(MoveOn);
    }


    private Boolean isMove = false;
    private float timer = 0;
    private void MoveOn()
    {
        timer += Time.deltaTime;
        switch (isMove)
        {
            case true:
                this.transform.position = Vector3.Lerp(StartPos.position, EndPos.position, timer * Speed);
                break;
            case false:
                this.transform.position = Vector3.Lerp(EndPos.position, StartPos.position, timer * Speed);
                break;
        }

        if (isMove && this.transform.position == EndPos.position)
        {
            timer = 0;
            isMove = false;
        }
        else if (!isMove && this.transform.position == StartPos.position)
        {
            timer = 0;
            isMove = true;
        }
    }


    private void OnDestroy()
    {
        MonoManager.Instance.RemoveUpdateListener(MoveOn);
    }
}
