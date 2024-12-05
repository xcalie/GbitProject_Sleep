using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shard : InteracitonBase
{
    [SerializeField]
    private int numShardAdd = 1;

    protected override void Interact()
    {
        base.Interact();
        Debug.Log("Shard");
        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
        MainControl.Instance.numShard += numShardAdd;
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.Shard;
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

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        bool isTShow = isTextShow;
        base.OnTriggerExit2D(collision);
        if (collision.CompareTag("Player") && isTShow)
        {
            Destroy(gameObject);
        }
    }
}
