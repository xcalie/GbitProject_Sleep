using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sugar : InteracitonBase
{
    protected override void Interact()
    {
        base.Interact();
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            //第一关则回血
            case 1:
                MainControl.Instance.GetHP(1);
                MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
                Destroy(gameObject, 0.1f);
                break;
            //第二三关恢复清醒度并回血
            case 2:
            case 3:
                SleepSystem.Instance.FullyAwake();
                MainControl.Instance.GetHP(1);
                Debug.Log("Sugar");
                MusicControl.Instance.CreateAndPlay(gameObject, ref ASForInteration, MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
                Destroy(gameObject, 0.1f);
                break;
        }
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.Sugar;
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
}
