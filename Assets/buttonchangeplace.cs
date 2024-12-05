using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class buttenchange : MonoBehaviour
{
    // 这个方法将在按钮被点击时调用
    public void ChangeToUI2Scene()
    {
        // 加载名为"UI2"的场景
        SceneManager.LoadScene("UI2");
    }
}

