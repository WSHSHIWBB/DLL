using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using VRFrameWork;
using System.IO;
using System.Text;
using System;

public enum Enum_MessageType
{
    None=0,
    Pet=1,
    TimeCool=2,
    OverDis=3,
}

[Serializable]
public class json_Message
{
    public int MessageType;
    public string Message;
}

public class NativeBridge : MonoBehaviour
{
    public delegate void FinishDowloadHandler(WWW www);
    private GameObject currentPet = null;
    public GameObject LoadingPage;

    private string LOCAL_PERSISTENT_PARTH = null;
    private string LOCAL_STREAMING_PARTH = null;
    private string LOCAL_VERSION_URL = null;
    private string PLATFORM_FILE = null;
    private Dictionary<string, string> localVertsionDic = new Dictionary<string, string>();
    private Dictionary<string, string> serverVersionDic = new Dictionary<string, string>();
    private bool needToUpdateLocalVersion = false;


#if UNITY_ANDROID                                    //&& !UNITY_EDITOR
    private readonly string VERSION_FILE="AndroidVersion.txt";
    private static readonly string SERVER_VERSION_URL = "http://112.74.34.144/doulalaUnity/Pets/Android/";
    public AndroidJavaObject javaObject;
#endif

#if UNITY_EDITOR                                     && !UNITY_ANDROID && !UNITY_IPHONE
    private readonly string VERSION_FILE = "EditorVersion.txt";
    private static readonly string SERVER_VERSION_URL = "http://112.74.34.144/doulalaUnity/Pets/Editor/";
#endif

#if UNITY_IPHONE                                      && !UNITY_EDITOR
    private readonly string VERSION_FILE = "IOSVersion.txt";
    private static readonly string SERVER_VERSION_URL = "http://112.74.34.144/doulalaUnity/Pets/IOS/";
    [DllImport("__Internal")]
    private static extern void IOSNativeInitCenter(string callBackGameObject, string callBackFunc);
    [DllImport("__Internal")]
    private static extern void OnBack(int ret);
#endif

    public void OnUnityBack(int ret)
    {
#if UNITY_IPHONE  
        OnBack(ret);
#endif
    }

    private void Awake()
    {
        LOCAL_PERSISTENT_PARTH = Application.persistentDataPath+"/";
        LOCAL_STREAMING_PARTH = Application.streamingAssetsPath+"/";

#if UNITY_ANDROID                                     &&!UNITY_EDITOR
        //AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        LOCAL_VERSION_URL = "file://"+LOCAL_PERSISTENT_PARTH;
        PLATFORM_FILE = "Android/";
#endif

#if UNITY_EDITOR                                     //&&!UNITY_ANDROID && !UNITY_IPHONE
        LOCAL_VERSION_URL = "file:///" + LOCAL_PERSISTENT_PARTH;
        PLATFORM_FILE = "Editor/";
#endif

#if UNITY_IPHONE                                     && !UNITY_EDITOR
        LOCAL_VERSION_URL = "file://" + LOCAL_PERSISTENT_PARTH;
        PLATFORM_FILE = "IOS/";
#endif

        MoveFilesFromStreamingToPersistent();
        
        StartCoroutine(DownLoad(SERVER_VERSION_URL + VERSION_FILE, delegate (WWW serverVersion)
          {
              HandleVersionFile(serverVersion.text, serverVersionDic);
              StartCoroutine(DownLoad(LOCAL_VERSION_URL + VERSION_FILE, delegate (WWW localVersion)
                {
                    HandleVersionFile(localVersion.text, localVertsionDic);
                

#if UNITY_ANDROID                 &&!UNITY_EDITO
                    //javaObject.Call("unityInitialized"); 
#endif

#if UNITY_IPHONE                  &&!UNITY_EDITOR
                    IOSNativeInitCenter(this.gameObject.name, "InitializePet");
#endif
                    
                    json_Message obj = new json_Message();
                    obj.MessageType = 1;
                    obj.Message =(UnityEngine.Random.Range(1,29)).ToString()+"|小狮王|草原|100";
                    //obj.Message = 100.ToString();
                    string js=JsonUtility.ToJson(obj);
                    InitializePet(js);
                    
                }));
          }));        
    }

    private void MoveFilesFromStreamingToPersistent()
    {
#if UNITY_ANDROID              &&!UNITY_EDITOR         
        StreamWriter sw;
        FileInfo fi1 = new FileInfo(LOCAL_PERSISTENT_PARTH + VERSION_FILE);
        if(!fi1.Exists)
        {
            StartCoroutine(DownLoad(LOCAL_STREAMING_PARTH + VERSION_FILE, delegate (WWW file)
                 {
                     sw = fi1.CreateText();
                     sw.Write(file.text, 0, file.text.Length);
                     sw.Close();
                     sw.Dispose();
                 })); 
        }
#endif
#if !UNITY_ANDROID
        StreamWriter sw;
        FileInfo fi1 = new FileInfo(LOCAL_PERSISTENT_PARTH + VERSION_FILE);
        if(!fi1.Exists)
        {
            StartCoroutine(DownLoad("file://"+LOCAL_STREAMING_PARTH + VERSION_FILE, delegate (WWW file)
                 {
                     sw = fi1.CreateText();
                     sw.Write(file.text, 0, file.text.Length);
                     sw.Close();
                     sw.Dispose();
                 })); 
        }
#endif
    }

    void InitializePet(string json)
    {
        var message=JsonUtility.FromJson<json_Message>(json);
        switch((Enum_MessageType)message.MessageType)
        {
            case Enum_MessageType.Pet :
                GetPetFromAssetBundle(message.Message);break;
            case Enum_MessageType.TimeCool :
                ShowTimeCool(int.Parse(message.Message));break;
            case Enum_MessageType.OverDis :
                ShowOverDistance(int.Parse(message.Message));break;
            default: break;
        }
    }

    void ShowTimeCool(int time)
    {
        EventDispatcher.TriggerEvent("PauseMainCameAim");
        EventDispatcher.TriggerEvent("ShowTimeCool", time);
    }

    void ShowOverDistance(int distance)
    {
        EventDispatcher.TriggerEvent("PauseMainCameAim");
        EventDispatcher.TriggerEvent("ShowOverDistance", distance);
    }
    
    private GameObject LoadFromAssetBundles(string petNum)
    {
        var assetBundle = AssetBundle.LoadFromFile(LOCAL_PERSISTENT_PARTH + petNum);
        if (assetBundle==null)
        {
            Debug.Log("not in local");
            assetBundle = AssetBundle.LoadFromFile(LOCAL_STREAMING_PARTH + "Pets/"+ PLATFORM_FILE + petNum);
            if(assetBundle==null)
            {
                Debug.Log("The assetBundle is null!");
                return null;
            }
        }
        GameObject petPrefab = assetBundle.LoadAsset<GameObject>(petNum+".prefab");
        if(petPrefab==null)
        {
            Debug.Log("The petPerfab of numner "+petNum+ " is null");
            return null;
        }
        assetBundle.Unload(false);
        return (GameObject)petPrefab;
    }

    private void GetPetFromAssetBundle(string petMessage)
    {
        //LoadingPage.SetActive(true);
        var messages = petMessage.Split('|');
        string petNum = messages[0];
        
        CompareVersion(petNum);
        if(needToUpdateLocalVersion)
        {
            StartCoroutine(DownLoad(SERVER_VERSION_URL + petNum, delegate (WWW bundle)
               {
                   ReplaceLocalRes(petNum, bundle.bytes);
                   UpdateLocalVersionFile();
                   StartCoroutine(DownLoad(LOCAL_VERSION_URL + petNum, delegate (WWW www)
                      {
                          currentPet =Instantiate(www.assetBundle.LoadAsset<GameObject>(petNum + ".prefab"));
                          www.assetBundle.Unload(false);
                          currentPet.transform.SetParent(transform.GetChild(0).GetChild(0));
                          currentPet.transform.localPosition = Vector3.zero;
                          currentPet.transform.localRotation = Quaternion.identity;
                          EventDispatcher.TriggerEvent("CapturePet",petMessage);
                      }));
               }));
        }
        else
        {
            currentPet = Instantiate(LoadFromAssetBundles(petNum));
            if (currentPet != null)
            {
                currentPet.transform.SetParent(transform.GetChild(0).GetChild(0));
                currentPet.transform.localPosition = Vector3.zero;
                currentPet.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.Log(1111);
            }
            EventDispatcher.TriggerEvent("CapturePet",petMessage);
        }
    }

    private void ReplaceLocalRes(string petNum, byte[] data)
    {
        FileStream stream = new FileStream(LOCAL_PERSISTENT_PARTH + petNum, FileMode.Create);
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }

    private void UpdateLocalVersionFile()
    {
        if(needToUpdateLocalVersion)
        { 
            StringBuilder sb = new StringBuilder();
            foreach(var item in localVertsionDic)
            {
                sb.Append(item.Key).Append('|').Append(item.Value).Append('\n');
            }

            FileStream fs = new FileStream(LOCAL_PERSISTENT_PARTH + VERSION_FILE, FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
    }

    private void CompareVersion(string petNum)
    {
        if(!serverVersionDic.ContainsKey(petNum))
        {
            Debug.Log(petNum);
            Debug.Log("The pet is not exit!");
            return;
        }
        if(!localVertsionDic.ContainsKey(petNum))
        {
            needToUpdateLocalVersion = true;
            localVertsionDic.Add(petNum, serverVersionDic[petNum]);
        }
        else if(localVertsionDic[petNum]!=serverVersionDic[petNum])
        {
            needToUpdateLocalVersion = true;
            localVertsionDic[petNum] = serverVersionDic[petNum];
        }
        else
        {
            needToUpdateLocalVersion = false;
        }
    }

    private IEnumerator DownLoad(string url, FinishDowloadHandler finishHandler)
    {
        WWW www = new WWW(url);
        yield return www;
        if(finishHandler!=null)
        {
            finishHandler(www);
        }
        www.Dispose();
    }

    private void HandleVersionFile(string version,Dictionary<string,string> saveDic)
    {
        if(version==null||version.Length==0)
        {
            return;
        }
        string[] items = version.Split(new char[] { '\n' });
        for(int i=0;i<items.Length;++i)
        {
            string[] infos = items[i].Split(new char[] { '|' });
            if(infos!=null&&infos.Length==2)
            {
               saveDic.Add(infos[0], infos[1]);
            }
        }
    }

}
