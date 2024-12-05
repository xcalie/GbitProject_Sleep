using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
  public void Play()
  {
      SceneManager.LoadScene("level-1");//输入场景
  }
    public void Quit()
    {
        Application.Quit();
    }
    
}


