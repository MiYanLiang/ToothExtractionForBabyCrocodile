using System;
using System.Collections;
using System.Collections.Generic;
using UDPClientModule;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    //UDP传输连接API
    private UDPClientComponent udpClient;

    public static GameNetworkManager instance;

    public int clientPort = 4001;

    [HideInInspector]
    public int gameTypeIndex;   //所选游戏类型

    private void Awake()
    {
        gameTypeIndex = -1;
    }

    private void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            udpClient = gameObject.GetComponent<UDPClientComponent>();
            
            ConnectToUDPServer();
        }
        else
        {
            //一个客户端只需要一个交互类
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 设置和服务器连接交互和回调方法
    /// </summary>
    private void ConnectToUDPServer()
    {
        if (udpClient.GetServerIP()!=string.Empty)
        {
            udpClient.connect(udpClient.GetServerIP(), StaticManager.serverPort, clientPort);


        }
    }
}
