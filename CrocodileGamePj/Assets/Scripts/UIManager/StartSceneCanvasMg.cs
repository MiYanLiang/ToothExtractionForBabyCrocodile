using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneCanvasMg : MonoBehaviour
{
    [SerializeField]
    Button startGameBtn;

    private void Start()
    {
        AddListenerToBtn();
    }

    //添加监听事件
    private void AddListenerToBtn()
    {
        startGameBtn.onClick.AddListener(delegate () { OnClickStartGame(); });
    }

    //点击开始游戏
    private void OnClickStartGame()
    {
        SceneManager.LoadScene("Main");
    }

}
