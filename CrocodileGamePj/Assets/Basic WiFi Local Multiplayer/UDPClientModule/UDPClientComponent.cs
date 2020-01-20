using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// UDP client component.
/// </summary>
namespace UDPClientModule
{
    public class UDPClientComponent : MonoBehaviour
    {

        private string serverURL;

        private int serverPort;

        int clientPort;

        private UdpClient udpClient;

        private readonly object udpClientLock = new object();

        static private readonly char[] Delimiter = new char[] { ':' };

        string receivedMsg = string.Empty;

        private Dictionary<string, List<Action<SocketUDPEvent>>> handlers;

        private Queue<SocketUDPEvent> eventQueue;

        private object eventQueueLock;

        private IPEndPoint endPoint;

        private string listenerInput = string.Empty;

        public enum UDPSocketState { DISCONNECTED, CONNECTED, ERROR, SENDING_MESSAGE };

        public UDPSocketState udpSocketState;

        private Thread tListenner;

        public string serverIP = string.Empty;

        public bool noNetwork;

        public string localNetworkIP;


        public void Awake()
        {
            Debug.Log("UDPClientComponent_Awake()");

            handlers = new Dictionary<string, List<Action<SocketUDPEvent>>>();

            eventQueueLock = new object();

            eventQueue = new Queue<SocketUDPEvent>();


            udpSocketState = UDPSocketState.DISCONNECTED;
        }




        /// <summary>
        /// open a connection with the specific server using the server URL (IP) and server Port.
        /// </summary>
        /// <param name="_serverURL">Server IP.</param>
        /// <param name="_serverPort">Server port.</param>
        /// <param name="_clientPort">Client port.</param>
        public void connect(string _serverURL, int _serverPort, int _clientPort)
        {

            //	Debug.Log ("tentando estabelecer conexão com o servidor");
            if (tListenner != null && tListenner.IsAlive)
            {

                disconnect();

                while (tListenner != null && tListenner.IsAlive)
                {


                }
            }

            //host udp server
            this.serverURL = _serverURL;

            //server port
            this.serverPort = _serverPort;

            //client port
            this.clientPort = _clientPort;

            // start  listener thread
            tListenner = new Thread(
                new ThreadStart(OnListeningServer));

            tListenner.IsBackground = true;

            tListenner.Start();



        }


        public void OnListeningServer()
        {

            try
            {

                lock (udpClientLock)
                {

                    udpClient = new UdpClient();

                    udpClient.ExclusiveAddressUse = false;

                    udpClient.Client.SetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    IPEndPoint localEp = new IPEndPoint(IPAddress.Any, clientPort);

                    udpClient.Client.Bind(localEp);

                    udpSocketState = UDPSocketState.CONNECTED;

                    udpClient.BeginReceive(new AsyncCallback(OnWaitPacketsCallback), null);


                }

            }
            catch
            {

                throw;
            }
        }



        public void OnWaitPacketsCallback(IAsyncResult res)
        {

            lock (udpClientLock)
            {

                byte[] recPacket = udpClient.EndReceive(res, ref endPoint);
                MessageReceived(recPacket, endPoint.Address.ToString(), endPoint.Port);

                if (recPacket != null && recPacket.Length > 0)
                {
                    lock (eventQueueLock)
                    {

                        //decode the received bytes vector in string fotmat
                        //receivedMsg = "callback_name,param 1,param 2,param n, etc."
                        receivedMsg = Encoding.UTF8.GetString(recPacket);

                        //separates the items contained in the package using the two points ":" as sifter
                        //and it puts them separately in the vector package []
                        /*
		                  * package[0]= callback_name: e.g.: "PONG"
		                  * package[1]= message: e.g.: "pong!!!"
		                  * package[2]=  other message for example!
			            */

                        var package = receivedMsg.Split(Delimiter);

                        //enqueue
                        eventQueue.Enqueue(new SocketUDPEvent(package[0], receivedMsg));

                        receivedMsg = string.Empty;
                    }//END_LOCK
                }//END_IF

                udpClient.BeginReceive(new AsyncCallback(OnWaitPacketsCallback), null);

            }//END_LOCK
        }


        private void InvokEvent(SocketUDPEvent ev)
        {

            if (!handlers.ContainsKey(ev.name)) { return; }

            foreach (Action<SocketUDPEvent> handler in this.handlers[ev.name])
            {

                try
                {

                    handler(ev);
                }
                catch (Exception ex) { }
            }
        }


        public void MessageReceived(byte[] data, string ipHost, int portHost)
        {

            //Debug.Log(string.Format("Received data:: {0} of IP:: {1} and Port:: {2}", Encoding.UTF8.GetString (data), ipHost, portHost));

        }

        /// <summary>
        /// listening server messages.
        /// </summary>
        /// <param name="ev">Callback name</param>
        /// <param name="callback">Callback function.</param>
        public void On(string ev, Action<SocketUDPEvent> callback)
        {
            if (!handlers.ContainsKey(ev))
            {

                handlers[ev] = new List<Action<SocketUDPEvent>>();
            }

            handlers[ev].Add(callback);
        }

        /// <summary>
        /// Emit the pack or message to server.
        /// </summary>
        /// <param name="callbackID">Callback ID.</param>
        /// <param name="_pack">message</param>
        public void Emit(string callbackID, string _pack)
        {

            try
            {

                if (udpSocketState == UDPSocketState.CONNECTED)
                {
                    lock (udpClientLock)
                    {

                        if (udpClient == null)
                        {
                            udpClient = new UdpClient();

                            udpClient.ExclusiveAddressUse = false;

                            udpClient.Client.SetSocketOption(
                                SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, clientPort);

                            udpClient.Client.Bind(localEp);
                        }


                        udpSocketState = UDPSocketState.SENDING_MESSAGE;

                        string new_pack = callbackID + ":" + _pack;

                        byte[] data = Encoding.UTF8.GetBytes(new_pack.ToString()); //convert to bytes

#if UNITY_ANDROID && !UNITY_EDITOR
						var endPoint = new IPEndPoint(IPAddress.Parse( CaptiveReality.Jni.Util.StaticCall<string>("GetServerIP", "Invalid Response From JNI", "com.rio3dstudios.basicwifilocalmultiplayerplugin.IPManager")), serverPort);
                     
#else
                        var endPoint = new IPEndPoint(IPAddress.Parse(GetServerIP()), serverPort);

#endif

                        try
                        {

                            udpClient.EnableBroadcast = true;

                            udpClient.Send(data, data.Length, endPoint);

                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e.ToString());
                        }


                        udpSocketState = UDPSocketState.CONNECTED;
                    }
                }
            }
            catch (Exception e)
            {
                listenerInput = e.InnerException.Message;
            }
        }


        //get local server ip address
        public string GetServerIP()
        {

            serverIP = string.Empty;

            string address = string.Empty;

            string subAddress = string.Empty;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

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

                noNetwork = true;

                return string.Empty;
            }
            else
            {
                noNetwork = false;

                subAddress = address.Remove(address.LastIndexOf('.'));

                serverIP = subAddress + "." + 255;

                return subAddress + "." + 255;
            }
            return string.Empty;


        }





        private void OnDestroy()
        {

            disconnect();
        }

        public void Update()
        {
            lock (eventQueueLock)
            {

                while (eventQueue.Count > 0)
                {

                    InvokEvent(eventQueue.Dequeue());
                }
            }




        }

        void OnApplicationQuit()
        {

            disconnect();

        }

        /// <summary>
        /// 断开此客户端的连接。
        /// Disconnect this client.
        /// </summary>
        public void disconnect()
        {
            lock (udpClientLock)
            {
                if (udpClient != null)
                {
                    udpClient.Close();

                    udpClient = null;
                }

            }//END_LOCK

            if (tListenner != null)
            {
                tListenner.Abort();
            }
        }
    }
}