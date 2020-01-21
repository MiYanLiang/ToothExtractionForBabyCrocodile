using LitJson;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class LoadJsonFile : MonoBehaviour
{
    public static LoadJsonFile instance;

    //Resources文件夹下
    private static readonly string Folder = "Jsons/";
    //存放json数据名(用;分号分开)
    private static readonly string tableNameStrs = "TestTable;GameTypeTable";

    /// <summary>
    /// Test数据表
    /// </summary>
    public List<List<string>> TestTableDates;
    /// <summary>
    /// 游戏类型表
    /// </summary>
    public List<List<string>> GameTypeTableDates;



    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);//跳转场景等不销毁

            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        string[] arrStr = tableNameStrs.Split(';');
        if (arrStr.Length > 0)
            JsonDataToSheets(arrStr);   //传递Json文件名进行加载
        else
            Debug.Log("////请检查Json表名");
    }


    /// <summary>
    /// 加载json文件获取数据至链表中
    /// </summary>
    private void JsonDataToSheets(string[] tableNames)
    {
        //Json数据控制类
        Roots root = new Roots();
        //存放json数据
        string jsonData = string.Empty;
        //记录读取到第几个表
        int indexTable = 0;

        //Test测试表数据:TestTable
        {
            jsonData = LoadJsonByName(tableNames[indexTable]);
            root = JsonMapper.ToObject<Roots>(jsonData);
            TestTableDates = new List<List<string>>(root.TestTable.Count);
            for (int i = 0; i < root.TestTable.Count; i++)
            {
                TestTableDates.Add(new List<string>());
                TestTableDates[i].Add(root.TestTable[i].id);
                TestTableDates[i].Add(root.TestTable[i].warDrumName);
                TestTableDates[i].Add(root.TestTable[i].unlockLevel);
            }
            indexTable++;
            //Debug.Log("Json文件加载成功---" + tableNames[indexTable] + ".Json");
        }
        //游戏大厅游戏类型:GameTypeTable
        {
            jsonData = LoadJsonByName(tableNames[indexTable]);
            root = JsonMapper.ToObject<Roots>(jsonData);
            GameTypeTableDates = new List<List<string>>(root.GameTypeTable.Count);
            for (int i = 0; i < root.GameTypeTable.Count; i++)
            {
                GameTypeTableDates.Add(new List<string>());
                GameTypeTableDates[i].Add(root.GameTypeTable[i].id);
                GameTypeTableDates[i].Add(root.GameTypeTable[i].gameName);
                GameTypeTableDates[i].Add(root.GameTypeTable[i].numberOfRooms);
                GameTypeTableDates[i].Add(root.GameTypeTable[i].isUnlock);
            }
            indexTable++;
            //Debug.Log("Json文件加载成功---" + tableNames[indexTable] + ".Json");
        }

        if (indexTable >= tableNames.Length)
            Debug.Log("所有Json数据加载成功。");
        else
            Debug.Log("还有Json数据未进行加载。");
    }

    /// <summary>
    /// 深拷贝List数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="List">The list.</param>
    /// <returns>List{``0}.</returns>
    public List<T> DeepClone<T>(object List)
    {
        using (Stream objectStream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, List);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as List<T>;
        }
    }

    /// <summary>
    /// 通过json文件名获取json数据
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string LoadJsonByName(string fileName)
    {
        string path = string.Empty;
        string data = string.Empty;
        if (Application.isPlaying)
        {
            path = Path.Combine(Folder, fileName);  //合并文件路径
            var asset = Resources.Load<TextAsset>(path);
            Debug.Log("Loading..." + fileName + "\nFrom:" + path);
            if (asset == null)
            {
                Debug.LogError("No text asset could be found at resource path: " + path);
                return null;
            }
            data = asset.text;
            Resources.UnloadAsset(asset);
        }
        else
        {
#if UNITY_EDITOR
            path = Application.dataPath + "/Resources/" + Folder + "/" + fileName + ".json";
            Debug.Log("Loading JsonFile " + fileName + " from: " + path);
            var asset1 = System.IO.File.ReadAllText(path);
            data = asset1;
#endif
        }
        return data;
    }

    /*
    /// <summary>
    /// 通过StreamReader读取json,json存在StreamingAssets文件夹下
    /// </summary>
    public JsonReader LoadJsonUseStreamReader(string fileName)
    {
        StreamReader streamreader = new StreamReader(Application.dataPath + "/StreamingAssets/Jsons/" + fileName + ".json");  //读取数据，转换成数据流
        JsonReader js = new JsonReader(streamreader);   //再转换成json数据
        //Root r = JsonMapper.ToObject<Root>(js);     //读取
        //for (int i = 0; i < r.LevelTable.Count; i++)  //遍历获取数据
        //{
        //    textone.text += r.LevelTable[i].experience + "   ";
        //}
        streamreader.Close();
        return js;
    }

    /// <summary>
    /// 通过WWW方法读取Json数据，json存在StreamingAssets文件夹下
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public string LoadJsonUseWWW(string fileName)
    {
        string localPath = string.Empty;
        if (Application.platform == RuntimePlatform.Android)
        {
            //localPath = Application.streamingAssetsPath + "/" + path;
            localPath = "jar:file://" + Application.dataPath + "!/assets/" + fileName + ".json";
        }
        else
        {
            localPath = "file:///" + Application.streamingAssetsPath + "/" + fileName + ".json";
        }
        WWW www = new WWW(localPath);     //格式必须是"ANSI"，不能是"UTF-8"
        if (www.error != null)
        {
            Debug.LogError("error : " + localPath);
            return null;          //读取文件出错
        }
        return www.text;
    }
    */
}