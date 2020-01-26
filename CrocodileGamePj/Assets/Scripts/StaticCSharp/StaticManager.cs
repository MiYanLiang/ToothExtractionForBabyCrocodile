/// <summary>
/// 静态参数类
/// </summary>
public class StaticManager
{
    //分隔符，分隔数据包数据
    public static readonly char DELIMITER = ':';
    //服务器端口号
    public static readonly int serverPort = 4000;
    public static readonly int serverPort2 = 4001;

    //发起包名
    public static readonly string ping_packName = "PING";
    public static readonly string joinGame_packName = "JOIN_GAME";
    public static readonly string sendNews_packName = "SEND_NEWS";
    public static readonly string disconnect_packName = "DISCONNECT";
    public static readonly string gameOver_packName = "GAME_OVER";
    //回复包名
    public static readonly string pong_receivePackName = "PONG";
    public static readonly string joinSuccess_receivePackName = "JOIN_SUCCESS";
    public static readonly string userDisconnect_receivePackName = "USER_DISCONNECTED";

}