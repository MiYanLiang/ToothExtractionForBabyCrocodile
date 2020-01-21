using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            gameTypeIndex = -1;
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

        public void CreateRoomServer()
        {

        }
    }
}