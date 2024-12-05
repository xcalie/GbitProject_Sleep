using UnityEngine;
using UnityEngine.UI;
using System;
public class pass : MonoBehaviour
{
    public Button Pass_button;
    public Button resume_button;
    public Button exit_button;
    public void PassGame()
    {
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Buttonappear()
    {
        resume_button.gameObject.SetActive(true);
        exit_button.gameObject.SetActive(true);
    }
    
    public void Awake()
    {
        resume_button.gameObject.SetActive(false);
        exit_button.gameObject.SetActive(false);
    }
    private void Start()
    {
        Pass_button.onClick.AddListener(PassGame);
        Pass_button.onClick.AddListener(Buttonappear);
        resume_button.onClick.AddListener(ResumeGame);
        resume_button.onClick.AddListener(Awake);
        exit_button.onClick.AddListener(Quit);
    }

   
}
