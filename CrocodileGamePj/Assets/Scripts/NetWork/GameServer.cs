using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UDPClientModule;
using UnityEngine;

namespace GameServerModule
{
    public class GameServer : MonoBehaviour
    {
        public static GameServer instance;

        private UDPClientComponent udpClient;
        [HideInInspector]
        public int gameTypeIndex;   //所选游戏类型

        public bool serverRunning; //服务器是否运行

        private Thread tListenner;

        public enum UDPServerState { DISCONNECTED, CONNECTED, ERROR, SENDING_MESSAGE };
        //服务器状态
        public UDPServerState udpServerState;

        private bool stopServer;

        private UdpClient udpServer;

        private int serverSocketPort;

        private Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();

        private readonly object connectedClientsLock = new object();

        private int onlinePlayers;

        private class Client
        {
            public string id;

            public string type;

            public float timeOut = 0f;

            public IPEndPoint remoteEP;

        }

        private void Awake()
        {
            gameTypeIndex = -1;
            serverRunning = false;
            stopServer = false;
            udpServerState = UDPServerState.DISCONNECTED;
        }

        private void Start()
        {
            if (instance==null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
                udpClient = gameObject.GetComponent<UDPClientComponent>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 创建房间服务器端
        /// </summary>
        public void CreateRoomServer()
        {
            if (GetServerIP() != string.Empty)
            {
                if (GameNetworkManager.instance.serverFound && !serverRunning)
                {
                    MainSceneCanvasMg.instance.ShowAlertDialog("服务器正在网络上运行");
                }
                else
                {
                    if (!serverRunning)
                    {
                        StartServer(StaticManager.serverPort);
                        serverRunning = true;
                        MainSceneCanvasMg.instance.CreateServerRoom("5417");
                        MainSceneCanvasMg.instance.ShowAlertDialog("服务器运行成功");
                        Debug.Log("UDP Server listening on IP " + GetServerIP() + " and port " + StaticManager.serverPort);
                    }
                    else
                    {
                        MainSceneCanvasMg.instance.ShowAlertDialog("服务器已在网络上运行！");
                        //MainSceneCanvasMg.instance.ShowAlertDialog("aaa");
                        //StartServer(8888);
                        //serverRunning = true;
                        //MainSceneCanvasMg.instance.CreateServerRoom("5417");
                        //MainSceneCanvasMg.instance.ShowAlertDialog("服务器运行成功");
                        //Debug.Log("UDP Server listening on IP " + GetServerIP() + " and port " + 8888);
                    }
                }
            }
            else
            {
                MainSceneCanvasMg.instance.ShowAlertDialog("请连接到WIFI网络");
            }
        }

        //服务器开启监听开启
        private void StartServer(int serverPort)
        {
            if (tListenner!= null && tListenner.IsAlive)
            {
                CloseServer();

                while (tListenner != null && tListenner.IsAlive) { }
            }

            serverSocketPort = serverPort;

            tListenner = new Thread(new ThreadStart(OnListeningClients));
            tListenner.IsBackground = true;
            tListenner.Start();
        }

        

        //服务器监听函数
        private void OnListeningClients()
        {
            udpServer = new UdpClient(serverSocketPort);
            udpServer.Client.ReceiveTimeout = 300;
            while (!stopServer)
            {
                try
                {
                    string receivedMsg = string.Empty;
                    string[] pack;
                    udpServerState = UDPServerState.CONNECTED;
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpServer.Receive(ref anyIP);
                    receivedMsg = Encoding.ASCII.GetString(data);
                    pack = receivedMsg.Split(StaticManager.DELIMITER);

                    //switch (pack[0])
                    //{
                    //    //处理收到的不同数据包

                    //    default:
                    //        break;
                    //}
                    if (pack[0] == StaticManager.ping_packName)
                    {
                        OnReceivePing(pack, anyIP);
                    }
                    else
                    {
                        if (pack[0] == StaticManager.joinGame_packName)
                        {
                            OnReceiveJoinGame(pack, anyIP);
                        }
                        else
                        {
                            if (pack[0] == StaticManager.disconnect_packName)
                            {
                                OnReceiveDisconnect(pack, anyIP);
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    Debug.Log("监听函数ERROR:" + err.ToString());
                }
            }
        }

        private void OnReceiveDisconnect(string[] pack, IPEndPoint anyIP)
        {
            /*
		     * data.pack[0]= CALLBACK_NAME: "disconnect"
		     * data.pack[1]= player_id
		     * data.pack[2]= isMasterServer (true or false)
		    */
            Dictionary<string, string> send_pack = new Dictionary<string, string>();
            string response = string.Empty;
            byte[] msg = null;

            send_pack["callback_name"] = StaticManager.userDisconnect_receivePackName;   //回复的包名
            send_pack["msg"] = pack[1];
            send_pack["isMasterServer"] = pack[2];

            response = send_pack["callback_name"] + ':' + send_pack["msg"] + ':' + send_pack["isMasterServer"];
            msg = Encoding.ASCII.GetBytes(response);

            foreach (KeyValuePair<string, Client> entry in connectedClients)
            {
                Debug.Log("广播断开连接");
                
                udpServer.Send(msg, msg.Length, entry.Value.remoteEP);
            }
            connectedClients.Clear();
        }

        private void OnReceiveJoinGame(string[] pack, IPEndPoint anyIP)
        {
            /*
		        * pack[0] = CALLBACK_NAME: "JOIN_GAME"
		        * pack[1] = player id
		    */
            if (!connectedClients.ContainsKey(pack[1]))
            {
                string response = string.Empty;

                byte[] msg = null;

                Client client = new Client();

                client.id = pack[1];
                client.remoteEP = anyIP;

                lock (connectedClientsLock)
                {
                    connectedClients.Add(client.id.ToString(), client);

                    onlinePlayers = connectedClients.Count;
                }

                Dictionary<string, string> send_pack = new Dictionary<string, string>();

                if (onlinePlayers < int.Parse(LoadJsonFile.instance.GameTypeTableDates[gameTypeIndex][2]))  //当前游戏的最大房间人数
                {
                    send_pack["callback_name"] = StaticManager.joinSuccess_receivePackName;
                    send_pack["msg"] = client.id + "Player joined!";

                    response = send_pack["callBack_name"] + ':' + send_pack["msg"];

                    msg = Encoding.ASCII.GetBytes(response);

                    udpServer.Send(msg, msg.Length, anyIP);
                }
                else
                {
                    //人数满了应该的操作
                }
            }


        }

        private void OnReceivePing(string[] pack, IPEndPoint anyIP)
        {
            /*
                * pack[0] = CALLBACK_NAME: "PONG"
                * pack[1] = "ping"
            */

            Dictionary<string, string> send_pack = new Dictionary<string, string>();

            string response = string.Empty;

            byte[] msg = null;

            send_pack["callback_name"] = StaticManager.pong_receivePackName;
            send_pack["msg"] = "pong.";

            response = send_pack["callback_name"] + ':' + send_pack["msg"];

            msg = Encoding.ASCII.GetBytes(response);

            udpServer.Send(msg, msg.Length, anyIP);
        }

        //断开服务器
        private void CloseServer()
        {
            udpServerState = UDPServerState.DISCONNECTED;

            stopServer = true;

            if (udpServer != null)
            {
                udpServer.Close();
                udpServer = null;
            }

            if (tListenner != null)
            {
                tListenner.Abort();
            }
        }

        //获取本地服务器ip
        private string GetServerIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            string address = string.Empty;
            string subAddress = string.Empty;
            
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.Log("网内ip: " + ip.ToString());
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
        }

        //获取服务器状态
        public string GetServerStatus()
        {
            switch (udpServerState)
            {
                case UDPServerState.DISCONNECTED:
                    return "DISCONNECTED";
                    break;
                case UDPServerState.CONNECTED:
                    return "CONNECTED";
                    break;
                case UDPServerState.SENDING_MESSAGE:
                    return "SENDING_MESSAGE";
                    break;
                case UDPServerState.ERROR:
                    return "ERROR";
                    break;
            }
            return string.Empty;
        }
    }
}