using GameServerModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneCanvasMg : MonoBehaviour
{
    [SerializeField]
    Canvas gamesLobbyCanvas;    //游戏大厅界面
    [SerializeField]
    Canvas gameRoomCanvas;      //游戏房间界面
    [SerializeField]
    Transform gameContentObj;
    [SerializeField]
    Text gameName;
    //单个游戏入口的Prefab
    GameObject gameEntranceObj;

    private void Awake()
    {
        gamesLobbyCanvas.enabled = false;
        gameRoomCanvas.enabled = false;

        gameEntranceObj = Resources.Load("Prefabs/GameEntrance", typeof(GameObject)) as GameObject;

    }

    private void Start()
    {
        InitGameContent();
        gamesLobbyCanvas.enabled = true;
    }

    //初始化游戏大厅
    private void InitGameContent()
    {
        int gameCount = LoadJsonFile.instance.GameTypeTableDates.Count;
        for (int i = 0; i < gameCount; i++)
        {
            int index = i;
            GameObject obj = Instantiate(gameEntranceObj, gameContentObj);
            obj.transform.GetChild(1).GetComponent<Text>().text = LoadJsonFile.instance.GameTypeTableDates[i][1];   //游戏名
            obj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () { OnClickGameEntrance(index); });  //添加各游戏入口点击事件
            obj.transform.GetChild(3).GetComponent<Image>().enabled = LoadJsonFile.instance.GameTypeTableDates[i][3] == "0";    //解锁遮罩
        }
    }

    //各个游戏入口的点击方法
    private void OnClickGameEntrance(int gameTypeIndex)
    {
        gameName.text = LoadJsonFile.instance.GameTypeTableDates[gameTypeIndex][1];
        GameServer.instance.gameTypeIndex = gameTypeIndex;
        GameNetworkManager.instance.gameTypeIndex = gameTypeIndex;
        gameRoomCanvas.enabled = true;
        gamesLobbyCanvas.enabled = false;
    }

}