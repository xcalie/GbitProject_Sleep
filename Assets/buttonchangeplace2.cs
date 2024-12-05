using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class buttonchangeplace2 : MonoBehaviour
{

    // 这个方法将在按钮被点击时调用
    public void ChangeToUIScene()
    {
        // 加载名为"UI"的场景
        SceneManager.LoadScene("UI");
    }

}
