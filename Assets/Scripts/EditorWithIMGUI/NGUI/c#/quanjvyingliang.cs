using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    
    //public float globalAudioVolume = 1.0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

   #region 设置全局音量

   public void setGlobalAudioVolume(float volume)
   {
       //globalAudioVolume = volume;
       MusicControl.Instance.AllVolume = volume;
        ApplyVolumeToAllAudioSources();
   }

   private void ApplyVolumeToAllAudioSources()
   {
       foreach (AudioSource audioSource in GetComponents<AudioSource>())
       {
           audioSource.volume = MusicControl.Instance.AllVolume; 
       }
   }
#endregion
   
}
