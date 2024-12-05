using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackHole : InteracitonBase
{
    protected override void Interact()
    {
        base.Interact();
        Debug.Log("BlackHole");
        MusicControl.Instance.CreateAndPlay(gameObject,ref ASForInteration,MusicType.Interaction, 0, false, MusicControl.Instance.AUDIOVolume);
        //这里进行场景切换
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void Awake()
    {
        //初始化交互物品类型
        type = IntractionType.BlackHole;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.E) && isKeyDown)
        {
            Interact();
        }
    }

}
