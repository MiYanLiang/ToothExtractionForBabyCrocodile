using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UDPClientModule;


namespace TicTacToeServerModule
{

    public class TicTacToeServer : MonoBehaviour
    {
        //服务器单例
        public static TicTacToeServer instance;

        //from UDP Client Module API UDP客户端接口
        private UDPClientComponent udpClient;
        //端口号
        public int serverSocketPort;    

        UdpClient udpServer;

        private readonly object udpServerLock = new object();

        private readonly object connectedClientsLock = new object();

        static private readonly char[] Delimiter = new char[] { ':' };

        private const int bufSize = 8 * 1024;

        private State state = new State();

        private IPEndPoint endPoint;

        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

        private AsyncCallback recv = null;
        //UPD服务器状态
        public enum UDPServerState { DISCONNECTED, CONNECTED, ERROR, SENDING_MESSAGE };
        //记录服务器状态
        public UDPServerState udpServerState;

        public string[] pack;

        private Thread tListenner;

        public string serverHostName;

        string receivedMsg = string.Empty;

        private bool stopServer = false;

        public int serverPort = 3310;

        public bool tryCreateServer;

        public bool waitingAnswer;
        //面板设置为false
        public bool serverRunning;
        //在线人数
        public int onlinePlayers;

        //store all players in game
        //存放所有玩家
        public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();

        public class Client
        {
            public string id;

            public string type;

            public float timeOut = 0f;

            public IPEndPoint remoteEP;

        }

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Awake()
        {
            udpServerState = UDPServerState.DISCONNECTED;

        }


        // Use this for initialization
        void Start()
        {

            // if don't exist an instance of this class
            if (instance == null)
            {

                //it doesn't destroy the object, if other scene be loaded
                DontDestroyOnLoad(this.gameObject);

                instance = this;// define the class as a static variable

                udpClient = gameObject.GetComponent<UDPClientComponent>();

            }
            else
            {
                //it destroys the class if already other class exists
                Destroy(this.gameObject);
            }

        }

        public string GetServerStatus()
        {
            switch (udpServerState)
            {
                case UDPServerState.DISCONNECTED:
                    return "DISCONNECTED";

                case UDPServerState.CONNECTED:
                    return "CONNECTED";

                case UDPServerState.SENDING_MESSAGE:
                    return "SENDING_MESSAGE";

                case UDPServerState.ERROR:
                    return "ERROR";
            }

            return string.Empty;
        }


        //get local server ip address
        //获取本地服务器ip
        public string GetServerIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            string address = string.Empty;

            string subAddress = string.Empty;

            //search WiFI Local Network
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!ip.ToString().Contains("127.0.0.1"))
                    {
                        address = ip.ToString();
                    }
                }
            }

            if (address == string.Empty)
            {
                return string.Empty;
            }
            else
            {
                subAddress = address.Remove(address.LastIndexOf('.'));

                return subAddress + "." + 255;
            }

            return string.Empty;
        }

        /// <summary>
        /// 创建服务器端
        /// Creates a UDP Server in in the associated client
        /// called method when the button "start" on HUDCanvas is pressed
        /// </summary>
        public void CreateServer()
        {
            if (GetServerIP() != string.Empty)
            {

                if (TicTacNetworkManager.instance.serverFound && !serverRunning)
                {
                    TicTacCanvasManager.instance.ShowAlertDialog("THERE ARE SERVER RUNNING ON NETWORK!");


                }

                else
                {
                    if (!serverRunning)
                    {

                        StartServer(serverPort);

                        serverRunning = true;

                        Debug.Log("UDP Server listening on IP " + GetServerIP() + " and port " + serverPort);

                        Debug.Log("------- server is running -------");

                    }
                    else
                    {
                        TicTacCanvasManager.instance.ShowAlertDialog("SERVER ALREADY RUNNING ON NETWORK!");
                    }


                }

            }
            else
            {
                TicTacCanvasManager.instance.ShowAlertDialog("PLEASE CONNECT TO A WIFI NETWORK");
            }


        }



        /// <summary>
        /// 服务器端口号设置
        /// Starts the server.
        /// </summary>
        /// <param name="_serverPort">Server port.</param>
        public void StartServer(int _serverPort)
        {
            Debug.Log("创建服务器");

            //判断线程不为空，并且处于活跃状态
            if (tListenner != null && tListenner.IsAlive)
            {

                CloseServer();

                while (tListenner != null && tListenner.IsAlive) { }

            }

            //设置端口号
            this.serverSocketPort = _serverPort;

            //开启监听线程
            tListenner = new Thread(
                new ThreadStart(OnListeningClients));

            //后台线程，此设置  线程 会随着 主线程 的退出而退出
            tListenner.IsBackground = true;
            //启动线程 由NEW状态，变为RUNABLE状态
            tListenner.Start();

            Debug.Log("创建服务器成功");

        }


        /// <summary>
        /// 引发监听客户端事件
        /// Raises the listening clients event.
        /// </summary>
        public void OnListeningClients()
        {
            Debug.Log("服务器监听函数");

            udpServer = new UdpClient(serverSocketPort);

            udpServer.Client.ReceiveTimeout = 300; // msec 超时设置

            //服务器未停止
            while (stopServer == false)
            {
                try
                {

                    udpServerState = UDPServerState.CONNECTED;

                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                    byte[] data = udpServer.Receive(ref anyIP);

                    string text = Encoding.ASCII.GetString(data);

                    receivedMsg = text;

                    pack = receivedMsg.Split(Delimiter);

                    Debug.Log("收到数据包");
                    for (int i = 0; i < pack.Length; i++)
                    {
                        Debug.Log("pack["+i+"]--: "+pack[i]);
                    }

                    switch (pack[0])
                    {
                        //处理收到的不同数据包
                        case "PING":
                            OnReceivePing(pack, anyIP);//processes the received package
                            break;

                        case "JOIN_GAME":
                            OnReceiveJoinGame(pack, anyIP);//propocessa o comando recebido da aplicação do professor
                            break;

                        case "UPDATE_BOARD":
                            OnReceiveUpdateBoard(pack, anyIP);//propocessa o comando recebido da aplicação do professor
                            break;

                        case "GAME_OVER":
                            OnReceiveGameOver(pack, anyIP);//propocessa o comando recebido da aplicação do professor
                            break;

                        case "disconnect":
                            OnReceiveDisconnect(pack, anyIP);//processes the received package
                            break;

                    }//END_SWTCH

                }//END_TRY
                catch (Exception err)
                {
                    print("监听ERROR:" + err.ToString());
                }
            }//END_WHILE
        }

        public string generateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        void OnReceivePing(string[] pack, IPEndPoint anyIP)
        {
            /*
		       * pack[0]= CALLBACK_NAME: "PONG"
		       * pack[1]= "ping"
		    */

            Debug.Log("receive ping");
            Dictionary<string, string> send_pack = new Dictionary<string, string>();

            Dictionary<string, string> data2 = new Dictionary<string, string>();

            var response = string.Empty;

            byte[] msg = null;

            //JSON package
            send_pack["callback_name"] = "PONG";

            //store "pong!!!" message in msg field
            send_pack["msg"] = "pong!!!!";

            //format the data with the sifter comma for they be send from turn to udp client
            response = send_pack["callback_name"] + ':' + send_pack["msg"];

            //将消息转化成二进制码
            msg = Encoding.ASCII.GetBytes(response);
            //回复客户端
            udpServer.Send(msg, msg.Length, anyIP); // echo to client
        }



        //响应加入游戏方法
        void OnReceiveJoinGame(string[] pack, IPEndPoint anyIP)
        {

            /*
		        * pack[0] = CALLBACK_NAME: "JOIN_GAME"  包名
		        * pack[1] = player id    玩家id
		    */

            //判断玩家id是否存在
            if (!connectedClients.ContainsKey(pack[1]))
            {
                var response = string.Empty;

                byte[] msg = null;
                //创建一个客户端连接
                Client client = new Client();

                client.id = pack[1];//set client id

                //set  clients's port and ip address 字面意思
                client.remoteEP = anyIP;

                Debug.Log("[INFO] player " + client.id + ": 已记录!");

                lock (connectedClientsLock)
                {
                    //添加客户端到记录表中
                    connectedClients.Add(client.id.ToString(), client);
                    //设置在线人数
                    onlinePlayers = connectedClients.Count;

                    Debug.Log("[INFO] Total players在游戏中的玩家数: " + connectedClients.Count);

                }//END_LOCK

                Dictionary<string, string> send_pack = new Dictionary<string, string>();

                //first player connected
                if (onlinePlayers == 1)
                {

                    //JSON package
                    send_pack["callback_name"] = "JOIN_SUCCESS";

                    //store  player info in msg field
                    send_pack["msg"] = "player joined!";

                    //format the data with the sifter comma for they be send from turn to udp client
                    response = send_pack["callback_name"] + ':' + send_pack["msg"];

                    msg = Encoding.ASCII.GetBytes(response);

                    //send answer to client that called me 
                    udpServer.Send(msg, msg.Length, anyIP); // echo

                    Debug.Log("[INFO]sended to first connected player : JOIN_SUCCESS");



                }//END_IF
                else if (onlinePlayers <= 2) // already exist a connected player waiting
                {

                    send_pack["callback_name"] = "START_GAME";

                    //store  player info in msg field
                    send_pack["msg"] = "starting game for 2 players connected!";

                    //sends the client sender to all clients in game
                    foreach (KeyValuePair<string, Client> entry in connectedClients)
                    {

                        //format the data with the sifter comma for they be send from turn to udp client
                        response = send_pack["callback_name"] + ':' + send_pack["msg"];

                        msg = Encoding.ASCII.GetBytes(response);

                        //send answer to all clients in connectClients list
                        udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

                    }//END_FOREACH


                }//END_ELSE

            }//END_IF


        }




        void OnReceiveUpdateBoard(string[] pack, IPEndPoint anyIP)
        {

            /*
		        * pack[0] = CALLBACK_NAME: "UPDATE_BOARD"
		        * pack[1] = player_id
				* pack[2] = player_type
				* pack[3] = i
				* pack[4] = j
		    */

            Debug.Log("receive update board");

            Dictionary<string, string> send_pack = new Dictionary<string, string>();

            var response = string.Empty;

            byte[] msg = null;


            //JSON package
            send_pack["callback_name"] = "UPDATE_BOARD";

            send_pack["player_id"] = pack[1];

            send_pack["player_type"] = pack[2];

            send_pack["i"] = pack[3];

            send_pack["j"] = pack[4];

            //sends the client sender to all clients in game
            foreach (KeyValuePair<string, Client> entry in connectedClients)
            {


                Debug.Log("send update board");
                //format the data with the sifter comma for they be send from turn to udp client
                response = send_pack["callback_name"] + ':' + send_pack["player_id"] + ':' +
                                                             send_pack["player_type"] + ':' + send_pack["i"] + ':' + send_pack["j"];

                msg = Encoding.ASCII.GetBytes(response);

                //send answer to all clients in connectClients list
                udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

            }//END_FOREACH



        }


        void OnReceiveGameOver(string[] pack, IPEndPoint anyIP)
        {

            /*
		        * pack[0] = CALLBACK_NAME: "GAME_OVER"
		        * pack[1] = player_id
		    */

            Debug.Log("receive game over");

            Dictionary<string, string> send_pack = new Dictionary<string, string>();

            send_pack["player_id"] = pack[1];

            var response = string.Empty;

            byte[] msg = null;


            //JSON package
            send_pack["callback_name"] = "GAME_OVER";


            //sends the client sender to all clients in game
            foreach (KeyValuePair<string, Client> entry in connectedClients)
            {



                Debug.Log("send game over");
                //format the data with the sifter comma for they be send from turn to udp client
                response = send_pack["callback_name"] + ':' + send_pack["player_id"];

                msg = Encoding.ASCII.GetBytes(response);

                //send answer to all clients in connectClients list
                udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

            }//END_FOREACH

            connectedClients.Clear();//clear the players list


        }



        void OnReceiveDisconnect(string[] pack, IPEndPoint anyIP)
        {
            /*
		     * data.pack[0]= CALLBACK_NAME: "disconnect"
		     * data.pack[1]= player_id
		     * data.pack[2]= isMasterServer (true or false)
		    */


            Dictionary<string, string> send_pack = new Dictionary<string, string>();


            var response = string.Empty;

            byte[] msg = null;


            //JSON package
            send_pack["callback_name"] = "USER_DISCONNECTED";

            send_pack["msg"] = pack[1];

            send_pack["isMasterServer"] = pack[2];

            response = send_pack["callback_name"] + ':' + send_pack["msg"] + ':' + send_pack["isMasterServer"];

            msg = Encoding.ASCII.GetBytes(response);

            //sends the client sender to all clients in game
            foreach (KeyValuePair<string, Client> entry in connectedClients)
            {

                Debug.Log("send disconnect");

                //send answer to all clients in connectClients list
                udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

            }//END_FOREACH

            connectedClients.Clear();//clear the players list



        }




        /**
         *  断开服务器
         *  DISCONNECTS SERVER
         */
        public void CloseServer()
        {
            //更改服务器状态为断开
            udpServerState = UDPServerState.DISCONNECTED;
            //标记服务器是否停止
            stopServer = true;

            //停止udp服务区
            if (udpServer != null)
            {
                udpServer.Close();
                udpServer = null;
            }
            //停止服务器监听线程
            if (tListenner != null)
            {
                tListenner.Abort();
            }

        }
    }
}