using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UDPClientModule;
using TicTacToeServerModule;

public class TicTacNetworkManager : MonoBehaviour
{

    //from UDP Socket API
    private UDPClientComponent udpClient;

    //定义分隔符变量
    static private readonly char[] Delimiter = new char[] { ':' };

    //单例类useful for any gameObject to access this class without the need of instances her or you declare her
    public static TicTacNetworkManager instance;

    //标记玩家是否在线
    public bool onLogged = false;

    //存放玩家obj --- store localPlayer
    public GameObject myPlayer;

    //local player id
    public string myId = string.Empty;

    //local player id
    public string local_player_id;

    public int serverPort = 3310;

    public int clientPort = 3000;

    public bool waitingAnswer;

    public bool serverFound;

    //记录是否寻找服务器 cs:false
    public bool waitingSearch;


    public List<string> _localAddresses { get; private set; }

    public enum PlayerType { SQUARE, X };

    public PlayerType playerType;

    public bool myTurn;

    // Use this for initialization
    void Start()
    {
        Debug.Log("TicTacNetworkManager_start() 网络传输控制类，负责与服务器交互");
        //创建单例类
        // if don't exist an instance of this class
        if (instance == null)
        {
            //it doesn't destroy the object, if other scene be loaded
            DontDestroyOnLoad(this.gameObject);

            instance = this;// define the class as a static variable

            udpClient = gameObject.GetComponent<UDPClientComponent>();

            //find any  server in others hosts
            //寻找服务器执行相对ui方法
            ConnectToUDPServer();
        }
        else
        {
            //it destroys the class if already other class exists
            Destroy(this.gameObject);
        }
    }


    /// <summary>
    /// Connect client to TicTactoeServer.cs
    /// </summary>
    public void ConnectToUDPServer()
    {
        if (udpClient.GetServerIP() != string.Empty)
        {
            Debug.Log("执行相应ui方法"); 
            //connect to TicTacttoeServer
            udpClient.connect(udpClient.GetServerIP(), serverPort, clientPort);

            udpClient.On("PONG", OnPrintPongMsg);

            udpClient.On("JOIN_SUCCESS", OnJoinGame);

            udpClient.On("START_GAME", OnStartGame);

            udpClient.On("UPDATE_BOARD", OnUpdateBoard);

            udpClient.On("GAME_OVER", OnGameOver);

            udpClient.On("USER_DISCONNECTED", OnUserDisconnected);
        }
    }

    void Update()
    {

        //是否连接到网络
        // if there is no wifi network
        if (udpClient.noNetwork)
        {
            Debug.Log("提示连接网络");

            TicTacCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot";

            serverFound = false;

            TicTacCanvasManager.instance.ShowLoadingImg();
        }

        //是否有服务器
        //if it was not found a server
        if (!serverFound)
        {
            Debug.Log("没有创建服务器房间");

            TicTacCanvasManager.instance.txtSearchServerStatus.text = string.Empty;

            // if there is a wifi connection but the server has not been started
            if (!udpClient.noNetwork)
            {
                TicTacCanvasManager.instance.txtSearchServerStatus.text = "Please start the server ";
            }
            else
            {
                TicTacCanvasManager.instance.txtSearchServerStatus.text = "Please Connect to Wifi Hotspot ";
            }
            //协程检测服务器 start routine to detect a server on wifi network
            StartCoroutine("PingPong");
        }
        //found server
        else
        {

        }
    }




    /// <summary>
    /// corroutine called  of times in times to send a ping to the server
    /// </summary>
    /// <returns>The pong.</returns>
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



    //it generates a random id for the local player
    //生成一个随机id
    public string generateID()
    {
        string id = Guid.NewGuid().ToString("N");

        //reduces the size of the id
        id = id.Remove(id.Length - 15);

        return id;
    }

    /// <summary>
    /// 接收服务器的创建消息
    /// receives an answer of the server.
    /// from  void OnReceivePing(string [] pack,IPEndPoint anyIP ) in server
    /// </summary>
    public void OnPrintPongMsg(SocketUDPEvent data)
    {
        /*
		 * data.pack[0]= CALLBACK_NAME: "PONG"
		 * data.pack[1]= "pong!!!!"
		*/

        Debug.Log("收到服务器创建成功的消息，receive pong");

        serverFound = true;

        //arrow the located text in the inferior part of the game screen
        TicTacCanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";
    }




    /// <summary>
    /// ping服务器
    /// sends ping message to UDPServer.
    ///     case "PING":
    ///     OnReceivePing(pack,anyIP);
    ///     break;
    /// take a look in TicTacttoeServer.cs script
    /// </summary>
    public void EmitPing()
    {
        Debug.Log("Ping服务器");
        //hash table <key, value>	
        Dictionary<string, string> data = new Dictionary<string, string>();

        //JSON package
        data["callback_name"] = "PING";

        //store "ping!!!" message in msg field
        data["msg"] = "ping!!!!";

        //CanvasManager.instance.ShowAlertDialog ("try emit ping");
        //The Emit method sends the mapped callback name to  the server
        udpClient.Emit(data["callback_name"], data["msg"]);
        //CanvasManager.instance.ShowAlertDialog ("ping sended: "+serverFound);
    }


    /// <summary>
    /// 客户端发送加入游戏请求
    /// Emits the join game to ticTacttoeServer.
    /// case "JOIN_GAME":
    ///   OnReceiveJoinGame(pack,anyIP);
    ///  break;
    /// take a look in TicTactToeServer.cs script
    /// </summary>
    public void EmitJoinGame()
    {
        // check if there is a wifi connection
        if (!udpClient.noNetwork)
        {
            // check if there is a server running
            if (serverFound)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

                data["callback_name"] = "JOIN_GAME";

                //设置玩家id -- it is already verified an id was generated
                if (myId.Equals(string.Empty))
                {
                    myId = generateID();
                    data["player_id"] = myId;
                }
                else
                {
                    data["player_id"] = myId;
                }

                //send the message join to TicTactToeServer
                string msg = data["player_id"];

                udpClient.Emit(data["callback_name"], msg);
            }
            else
            {
                TicTacCanvasManager.instance.ShowAlertDialog("please start the server");
            }
        }
        else
        {
            TicTacCanvasManager.instance.ShowAlertDialog("Please Connect to Wifi Hotspot");
        }
    }


    /// <summary>
    /// 加入游戏房间方法
    /// Raises the join game event from TictactToeServer.
    /// only the first player to connect gets this feedback from the server
    /// </summary>
    /// <param name="data">Data.</param>
    void OnJoinGame(SocketUDPEvent data)
    {
        Debug.Log("\n joining ...\n");

        // open game screen only for the first player, as the second has not logged in yet
        //加入房间打开游戏UI面板
        TicTacCanvasManager.instance.OpenScreen(1);

        Debug.Log("try to loading board");

        //初始化棋盘 load the board only for the first player, because the second one hasn't logged in yet
        BoardManager.instance.LoadBoard();

        // set square to the first player to connect
        // 应该是设置玩家棋子类型
        SetPlayerType("square");

        TicTacCanvasManager.instance.txtHeader.text = "You are player O";

        TicTacCanvasManager.instance.txtFooter.text = "connected! \n Waiting for another player";

        Debug.Log("\n first player SQUARE joined...\n");

    }


    /// <summary>
    /// 游戏开始
    /// Raises the start game event.
    /// both players receive this response from the server
    /// </summary>
    /// <param name="data">Data.</param>
    void OnStartGame(SocketUDPEvent data)
    {
        Debug.Log("\n game is runing...\n");

        // 查看玩家棋子类型
        // check if it's the first player to connect
        if (GetPlayerType().Equals("square"))
        {
            // define as first to play	
            myTurn = true;

            TicTacCanvasManager.instance.txtHeader.text = "You are player O";
        }
        else// if you are the second player
        {

            myTurn = false;

            TicTacCanvasManager.instance.txtHeader.text = "You are player X";

            // load the game screen for this player only,
            //as the screen has already been loaded for the first player logged into the OnJoinGame method
            TicTacCanvasManager.instance.OpenScreen(1);

            //load the board for this player only
            BoardManager.instance.LoadBoard();
        }

        // check if you are the first player to connect
        if (myTurn)
        {
            TicTacCanvasManager.instance.txtFooter.text = "您的回合";
        }
        else
        {
            TicTacCanvasManager.instance.txtFooter.text = "对手回合";
        }

        Debug.Log("\n game loaded...\n");

    }


    /// <summary>
    /// 落棋子，更新棋盘
    /// Emits the update board to TictactToeServer
    /// </summary>
    public void EmitUpdateBoard(int i, int j)
    {
        Debug.Log("客户端落子，告知服务器");

        Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

        // 更改自身回合标记 -- now the turn belongs to the opposing player
        myTurn = false;

        TicTacCanvasManager.instance.txtFooter.text = "对手回合Opponent move";

        data["callback_name"] = "UPDATE_BOARD";
        data["player_id"] = myId;
        data["player_type"] = GetPlayerType();
        data["i"] = i.ToString();
        data["j"] = j.ToString();

        //send the message  to ticTacttoeServer
        string msg = data["player_id"] + ":" + data["player_type"] + ":" + data["i"] + ":" + data["j"];

        //sends to the server through socket UDP the jo package 
        udpClient.Emit(data["callback_name"], msg);

    }

    /// <summary>
    /// 下棋更新面板
    /// updates the board with information from TicTactToeServer
    /// </summary>
    void OnUpdateBoard(SocketUDPEvent data)
    {
        /*
		 * data.data.pack[0] = CALLBACK_NAME: "UPDATE_BOARD" from server
		 * data.data.pack[1] = 最后一步玩家id -- id of the opponent who made the last move
		 * data.data.pack[2] = player_type
		 * data.data.pack[3]= j
		 * data.data.pack[4] = i
		*/

        // how the server message is transmitted to both players,
        // we should check if we are the next player to play, message target
        //data.pack[1] stores the id of the player who finished his move
        //如果落子玩家不是此客户端
        if (!data.pack[1].Equals(myId))
        {
            //设置棋盘落子行列值
            // set row i and column j which should be updated in BoardManager
            BoardManager.instance.current_i = int.Parse(data.pack[3]);
            // set row i and column j which should be updated in BoardManager
            BoardManager.instance.current_j = int.Parse(data.pack[4]);
            //检查是什么类型的棋子落下
            // check the type of cell to update O or X
            if (data.pack[2].Equals("square"))
            {
                BoardManager.instance.SpawnSquare();
            }
            else
            {
                BoardManager.instance.SpawnX();
            }
            //此客户端的回合
            TicTacCanvasManager.instance.txtFooter.text = "Your move";

            myTurn = true;
        }
    }


    /// <summary>
    /// 通知服务器获得胜利
    /// Send a message to the server to notify you that the next player has lost the game.
    /// </summary>
    public void EmitGameOver()
    {
        Debug.Log("通知服务器 "+ myId + " 玩家获胜");

        Dictionary<string, string> data = new Dictionary<string, string>();//pacote JSON

        myTurn = false;

        TicTacCanvasManager.instance.txtFooter.text = " ";

        data["callback_name"] = "GAME_OVER";    //数据包类型
        data["player_id"] = myId;               //获胜玩家id

        string msg = data["player_id"];

        //sends to the server through socket UDP the msg package 
        udpClient.Emit(data["callback_name"], msg);
    }

    /// <summary>
    /// 服务器广播的结束游戏方法
    /// get the server update that the player of this instance lost the game
    /// </summary>
    void OnGameOver(SocketUDPEvent data)
    {
        /*
		 * data.data.pack[0] = CALLBACK_NAME: "GAME_OVER" from server
		 * data.data.pack[1] = player_id
		*/

        // how the server message is transmitted to both players,
        // we should check if we are the next player to play, the loser
        //data.pack[1] stores the id of the player who won the match
        //判断此客户端是否不是赢家，重置游戏
        if (!data.pack[1].Equals(myId))
        {
            BoardManager.instance.ResetGameForLoserPlayer();
        }
        myTurn = false;
    }


    /// <summary>
    /// 客户端断开连接 通知服务器
    /// Emits the disconnect to server
    /// </summary>
    void EmitDisconnect()
    {
        //hash table <key, value>
        Dictionary<string, string> data = new Dictionary<string, string>();

        //JSON package
        data["callback_name"] = "disconnect";
        data["local_player_id"] = myId;

        //如果这个客户端的服务器正在运行
        if (TicTacToeServer.instance.serverRunning)
        {
            data["isMasterServer"] = "true";
        }
        else
        {
            data["isMasterServer"] = "false";
        }

        string msg = data["local_player_id"] + ":" + data["isMasterServer"];

        Debug.Log("发送客户端 断开连接消息emit disconnect");

        udpClient.Emit(data["callback_name"], msg);

        //如果这个客户端的服务器正在运行，关闭服务器
        //不知道为啥分开在判断一次，让服务器有个响应过程？
        if (TicTacToeServer.instance.serverRunning)
        {
            TicTacToeServer.instance.CloseServer();
            Debug.Log("本地服务器关闭");
        }

        //断开连接-客户端接口置空
        if (udpClient != null)
        {
            udpClient.disconnect();
        }
    }

    /// <summary>
    /// 服务器断开连接回调函数
    /// inform the local player to destroy offline network player
    /// </summary>
    void OnUserDisconnected(SocketUDPEvent data)
    {
        /*
		 * data.pack[0]  = USER_DISCONNECTED
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = isMasterServer
		*/
        Debug.Log("断开连接disconnect!");

        //如果点开链接的客户端是服务器，此客户端重置游戏
        // check if the disconnected player was the master server
        if (bool.Parse(data.pack[2]))
        {
            RestartGame();
        }
        else
        {
            //? ? ?
            BoardManager.instance.ResetGameForWOPlayer();
            myTurn = false;
        }


    }

    public void RestartGame()
    {
        serverFound = false;

        TicTacCanvasManager.instance.OpenScreen(0);
    }

    //断开客户端连接方法
    void CloseApplication()
    {
        if (udpClient != null)
        {
            EmitDisconnect();

            udpClient.disconnect();
        }
    }

    //当应用被关闭时执行
    void OnApplicationQuit()
    {
        Debug.Log("程序结束 Application ending after " + Time.time + " seconds");

        CloseApplication();
    }


    //获得当前下棋玩家类型
    public string GetPlayerType()
    {
        switch (playerType)
        {
            case PlayerType.SQUARE:
                return "square";
                break;
            case PlayerType.X:
                return "x";
                break;

        }
        return string.Empty;
    }


    /// <summary>
    /// Sets the type of the user.
    /// </summary>
    /// <param name="_userType">User type.</param>
    public void SetPlayerType(string _playerType)
    {
        Debug.Log("设置玩家棋子类型是： "+_playerType);
        switch (_playerType)
        {
            
            case "square":
                playerType = PlayerType.SQUARE;
                break;
            case "x":
                playerType = PlayerType.X;
                break;
        }
    }
}