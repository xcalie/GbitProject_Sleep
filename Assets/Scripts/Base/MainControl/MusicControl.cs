using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MusicType
{
    BGM,
    Death,
    Movement,
    Interaction,
    Ability,
    Shoot,
    Pass,
    Scene,
    Control,
}


public class MusicControl : SingletonAutoMono<MusicControl>
{
    //激活标致
    public bool isMusic = false;


    /*
     * 音乐列表
     * 0:背景音乐
     * 1:死亡音乐
     * 2:运动音乐
     * 3:交互音乐
     * 4:能力音乐
     * 5:射击音乐
     * 6:通关音乐
     * 7:场景音乐
     * 8:控制音乐
     */
    private int musicCount = 9; //音乐种类数量(手动记录)

    public List<List<AudioClip>> MusicList;

    //背景音乐音量
    private float theBGMVolume = 0.5f;
    public float BGMVolume
    {
        get => theBGMVolume * AllVolume;
        set => theBGMVolume = value;
    }

    //音效音量
    private float theAUDIOVolume = 0.5f;
    public float AUDIOVolume
    {
        get => theAUDIOVolume * AllVolume;
        set => theAUDIOVolume = value;
    }

    public float AllVolume = 1f;


    private void Start()
    {
        //初始化音乐
        MusicList = new List<List<AudioClip>>();

        StartCoroutine(MusicAdd());
    }


    public void CreateAndPlay(GameObject gameObject,ref AudioSource audioSource, MusicType musicType, int index, bool loop, float volume)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log(index);
            Debug.Log(MusicList[(int)musicType][index]);
            audioSource.clip = MusicList[(int)musicType][index];
            audioSource.loop = loop;
            audioSource.volume = volume;
        }
        audioSource.Play();
    }

    IEnumerator MusicAdd()
    {
        AudioClip audioClip = null;
        ResourceRequest rq;
        string path;

        //加载音乐
        int j = 0;//索引
        for (int i = 0; i < musicCount; i++)
        {
            MusicList.Add(new List<AudioClip>());
            do
            {
                path = "MusicControl/" + (MusicType)i + "/" + (MusicType)i + j;
                //Debug.Log("正在加载" + path);
                rq = Resources.LoadAsync<AudioClip>(path);
                yield return rq;
                audioClip = rq.asset as AudioClip;
                //Debug.Log(audioClip);
                if (audioClip != null)
                {
                    MusicList[i].Add(audioClip);
                    j++;
                }
            }
            while (audioClip != null);
            j = 0;
        }
        MainControl.Instance.isBack = this;
    } 
}
