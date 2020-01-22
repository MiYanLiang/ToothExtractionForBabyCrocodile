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

        private bool serverRunning; //服务器是否运行

        private Thread tListenner;

        public enum UDPServerState { DISCONNECTED, CONNECTED, ERROR, SENDING_MESSAGE };
        //服务器状态
        public UDPServerState udpServerState;

        private bool stopServer;

        private UdpClient udpServer;

        private int serverSocketPort;

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
                        MainSceneCanvasMg.instance.ShowAlertDialog("服务器已在网络上运行");
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

        string receivedMsg = string.Empty;
        string[] pack;

        //服务器监听函数
        private void OnListeningClients()
        {
            udpServer = new UdpClient(serverSocketPort);
            udpServer.Client.ReceiveTimeout = 300;
            while (!stopServer)
            {
                try
                {
                    udpServerState = UDPServerState.CONNECTED;
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpServer.Receive(ref anyIP);
                    receivedMsg = Encoding.ASCII.GetString(data);
                    pack = receivedMsg.Split(StaticManager.DELIMITER);

                    switch (pack[0])
                    {
                        //处理收到的不同数据包
                        default:
                            break;
                    }
                }
                catch (Exception err)
                {
                    Debug.Log("监听函数ERROR:" + err.ToString());
                }
            }
        }

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