using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    // 重新开始 — 回到主菜单（场景索引 0）
    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(0);
    }

    // 退出游戏
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
