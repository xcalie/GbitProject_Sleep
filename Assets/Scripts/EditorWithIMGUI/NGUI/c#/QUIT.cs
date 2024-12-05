using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QUIT : MonoBehaviour
{
    
    void Update()
    {
        // 检查鼠标左键是否被按下
        if (Input.GetMouseButtonDown(0))
        {
            ExitGame();
        }
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        // 如果我们在Unity编辑器中运行，则调用这一行代码退出播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在打包后的游戏中，调用这一行代码来关闭游戏
            Application.Quit();
#endif
    }
}

