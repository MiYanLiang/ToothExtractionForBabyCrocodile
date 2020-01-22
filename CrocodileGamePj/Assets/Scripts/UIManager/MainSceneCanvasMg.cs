using GameServerModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneCanvasMg : MonoBehaviour
{
    public static MainSceneCanvasMg instance;

    [SerializeField]
    Canvas gamesLobbyCanvas;    //游戏大厅界面
    [SerializeField]
    Canvas gameRoomCanvas;      //游戏房间界面
    [SerializeField]
    Transform gameContentObj;
    [SerializeField]
    Transform roomContentObj;
    [SerializeField]
    Text gameName;
    [SerializeField]
    GameObject alertDialogObj;  //提示框


    private GameObject gameEntranceObj; //单个游戏入口Prefab

    private GameObject roomEntranceObj; //房间入口Prefab

    private int gameTypeIdx;

    private void Awake()
    {
        gameTypeIdx = -1;
        gamesLobbyCanvas.enabled = false;
        gameRoomCanvas.enabled = false;

        gameEntranceObj = Resources.Load("Prefabs/GameEntrance", typeof(GameObject)) as GameObject;
        roomEntranceObj = Resources.Load("Prefabs/roomEntrance", typeof(GameObject)) as GameObject;
    }

    private void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

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
        gameTypeIdx = gameTypeIndex;
        gameRoomCanvas.enabled = true;
        gamesLobbyCanvas.enabled = false;
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="contentStr"></param>
    public void ShowAlertDialog(string contentStr)
    {
        if (!alertDialogObj.activeSelf)
        {
            alertDialogObj.transform.GetChild(0).GetComponent<Text>().text = contentStr;
            alertDialogObj.SetActive(true);
        }
    }

    /// <summary>
    /// 创建服务器房间
    /// </summary>
    public void CreateServerRoom(string roomNumber)
    {
        GameObject obj = Instantiate(roomEntranceObj, roomContentObj);
        obj.transform.GetComponentsInChildren<Text>()[0].text += roomNumber;   //房间号
        obj.transform.GetComponentsInChildren<Text>()[1].text += LoadJsonFile.instance.GameTypeTableDates[gameTypeIdx][2];   //房间人数
        obj.transform.GetComponentsInChildren<Text>()[2].text += "等待中";   //房间状态
    }
}