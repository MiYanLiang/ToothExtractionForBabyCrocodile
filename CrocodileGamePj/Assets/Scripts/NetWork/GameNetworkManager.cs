using GameServerModule;
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

    public int clientPort = 4101;

    public int gameTypeIndex;   //所选游戏类型

    public bool serverFound;

    public bool waitingSearch;

    public string myId;

    private void Awake()
    {
        gameTypeIndex = -1;
        serverFound = false;
        myId = string.Empty;
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

    private void Update()
    {
        if (udpClient.noNetwork)
        {
            MainSceneCanvasMg.instance.ShowAlertDialog("请连接网络");

            serverFound = false;

        }
        else
        {
            if (!serverFound)
            {
                Debug.Log("没有服务器房间创建");
                if (udpClient.noNetwork)
                {
                    MainSceneCanvasMg.instance.ShowAlertDialog("请连接网络");
                }
                else
                {
                    MainSceneCanvasMg.instance.ShowAlertDialog("请创建房间");
                }
                //协程检测服务器
                StartCoroutine("PingPong");
            }
            else
            {

            }
        }
    }

    private IEnumerator PingPong()
    {
        if (waitingSearch)
        {
            yield break;
        }

        waitingSearch = true;

        //sends a ping to server
        EmitPing();

        // wait 1 seconds and continue
        yield return new WaitForSeconds(1);

        waitingSearch = false;
    }

    private void EmitPing()
    {
        Debug.Log("Ping服务器");

        Dictionary<string, string> data = new Dictionary<string, string>();

        data["callback_name"] = StaticManager.ping_packName;
        data["msg"] = "ping!";

        udpClient.Emit(data["callback_name"], data["msg"]);
    }

    /// <summary>
    /// 客户端发送加入游戏房间请求
    /// </summary>
    public void EmitJoinRoom()
    {
        if (!udpClient.noNetwork)
        {
            if (serverFound)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();

                data["callback_name"] = StaticManager.joinGame_packName;
                if (myId.Equals(string.Empty))
                {
                    myId = generateID();
                    data["player_id"] = myId;
                }
                else
                {
                    data["player_id"] = myId;
                }
                string msg = data["player_id"];

                udpClient.Emit(data["callback_name"], msg);
            }
            else
            {
                MainSceneCanvasMg.instance.ShowAlertDialog("请创建房间");
            }
        }
        else
        {
            MainSceneCanvasMg.instance.ShowAlertDialog("请连接网络");
        }
    }


    private void EmitDisconnect()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["callback_name"] = StaticManager.disconnect_packName;
        data["local_player_id"] = myId;
        if (GameServer.instance.serverRunning)
        {
            data["isMasterServer"] = "true";
        }
        else
        {
            data["isMasterServer"] = "false";
        }
        string msg = data["local_player_id"] + ":" + data["isMasterServer"];

        Debug.Log("发送客户端 断开连接消息");

        udpClient.Emit(data["callback_name"], msg);

        //断开连接-客户端接口置空
        if (udpClient != null)
        {
            udpClient.disconnect();
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

            udpClient.On(StaticManager.pong_receivePackName, OnPongMsg);

            udpClient.On(StaticManager.joinSuccess_receivePackName, OnJoinGame);

            udpClient.On(StaticManager.userDisconnect_receivePackName, OnUserDisconnected);

        }
    }

    private void OnUserDisconnected(SocketUDPEvent data)
    {
        /*
        * data.pack[0]= CALLBACK_NAME: "USER_DISCONNECTED"
        * data.pack[1]= msg
        * data.pack[2]= isMasterServer
        */

        Debug.Log("有人断开了连接");
        //是否是服务器断开连接
        if (bool.Parse(data.pack[2]))
        {
            RestartGame();
        }
        else
        {
            //有一个其他房客退出
        }

    }

    private void OnJoinGame(SocketUDPEvent data)
    {
        /*
        * data.pack[0]= CALLBACK_NAME: "JOIN_SUCCESS"
        * data.pack[1]= msg
        */

        Debug.Log("有人加入房间");

    }

    private void OnPongMsg(SocketUDPEvent data)
    {
        /*
		 * data.pack[0]= CALLBACK_NAME: "PONG"
		 * data.pack[1]= "pong."
		*/
        Debug.Log("收到服务器创建成功的消息，receive pong");

        serverFound = true;

        MainSceneCanvasMg.instance.ShowAlertDialog("有服务器运行了");
    }

    //服务器断开，退出房间
    private void RestartGame()
    {
        serverFound = false;
    }

    //生成一个随机id
    private string generateID()
    {
        string id = Guid.NewGuid().ToString("N");

        id = id.Remove(id.Length - 15);

        return id;
    }

    void OnApplicationQuit()
    {
        Debug.Log("程序结束 Application ending after " + Time.time + " seconds");

        CloseApplication();
    }

    void CloseApplication()
    {
        if (udpClient != null)
        {
            EmitDisconnect();

            udpClient.disconnect();
        }
    }

}
