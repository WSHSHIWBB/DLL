using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRFrameWork;
using LitJson;
using System;


public class roomInfo
{
    public int roomID = -1;
    public string userToken = "";
}

public class RoomStatus
{
    public class Roomdata
    {
        public int id;
        public int player_cur_num;
        public int pet_caught_num;
        public string status;
        public int pet1_count;
        public int pet2_count;
        public int pet3_count;
    }
    public int code;
    public Roomdata data;
}

public class HistoryInfo
{
    public class AvatarInfo
    {
        public string username;
        public string avatar;
    }
    public int code;
    public AvatarInfo[] data;
}

public class OSBridgeManager : MonoSingleton<OSBridgeManager>
{
#if UNITY_ANDROID
    public AndroidJavaObject javaObject;
#endif
#if UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern void IOSNativeInitCenter(string callBackGameObject, string callBackFunc);
    [DllImport("__Internal")]
    private static extern void OnBack(int ret);
#endif
#if UNITY_EDITOR
#endif

    //Delegate for after WWW class Get Successfully
    public delegate void AfterWWWGetHandler(string json);
    //Delegate for after Down Successfully
    public delegate void DownLoadHandler(WWW www,string name,int finishCount);
    //Delegate for after WWW class Post Successfully
    public delegate void AfterWWWPostHandler(string json);

    private int _roomID;
    private string _userToken;
    private RoomStatus _roomStatus;
    private HistoryInfo _historyInfo;
    private Dictionary<string, Sprite> _name_Sprite_Dic;

    private readonly string _roomStatusURL = "http://112.74.34.144:3001/hidepet/getRoomStatus?id=";
    private readonly string _historyURL = "http://112.74.34.144:3001/hidepet/getHistoryByRoomId?id=";
    private readonly string _caughtPetURL = "http://112.74.34.144:3001/api/hidepet/caughtPet?id=";
    private readonly int _roomStatusUpdateInterval = 1;

    private void Awake()
    {
        _name_Sprite_Dic = new Dictionary<string, Sprite>();
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
#endif

#if UNITY_EDITOR
        Test();
#endif
    }

    private void Test()
    {
        //StartCoroutine(Get(_roomStatusURL + 9, GetRoomStatusHandler));
        roomInfo info = new roomInfo();
        info.roomID = 195;
        info.userToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjQ4LCJpYXQiOjE0OTE5NTkxNDgsImV4cCI6MTQ5MjEzMTk0OH0.8T9sKTGRXFXdwRNs2Cz9ZoCPX7_2tI5jQH0K8-a8Ovs";
        string json = JsonMapper.ToJson(info);
        InitRoom(json);
    }

    public void InitRoom(string json)
    {
        StopAllCoroutines();
        roomInfo info = JsonMapper.ToObject<roomInfo>(json);
        if(info==null)
        {
            Debug.LogError("Json Error!");
            return;
        }
        _roomID = info.roomID;
        _userToken = info.userToken;
        StartCoroutine(Get(_roomStatusURL + _roomID, GetRoomStatusHandler));
        EventDispatcher.TriggerEvent("InitCapture");
    }

    #region PublicFunc
    public void OnUnityBack(int isCaught)
    {
#if UNITY_ANDROID
        javaObject.Call("onBack", isCaught);
#endif
#if UNITY_IPHONE
        OnBack(isCaught);
#endif
#if UNITY_EDITOR

#endif
    }

    public string GetRoomStatusPlayerNum()
    {
        if(_roomStatus==null)
        {
            return null;
        }
        else
        {
            return "(" + _roomStatus.data.pet_caught_num + '/' + _roomStatus.data.player_cur_num + ")";
        }
    }

    public string[] GetUserNames()
    {
        if(_name_Sprite_Dic.Count==0)
        {
            Debug.LogError("The NameSpriteDic is null!");
            return null;
        }
        List<string> list = new List<string>();
        foreach(var pair in _name_Sprite_Dic)
        {
            list.Add(pair.Key);
        }
        if(list.Count==0)
        {
            return null;
        }
        return list.ToArray();
    }

    public Sprite GetSpriteByName(string name)
    {
        if(!_name_Sprite_Dic.ContainsKey(name))
        {
            Debug.LogError("The NameSpriteDic don't contain the name of " + name);
            return null;
        }
        return _name_Sprite_Dic[name];
    }

    public void JudgeCapturePetOnServer(string petName)
    {
        Debug.Log(petName);
        if (!string.IsNullOrEmpty(petName))
        {
            string url = _caughtPetURL + _roomID + "&petName=" + WWW.EscapeURL(petName) + "&token=" + _userToken;
            Debug.Log(url);
            StartCoroutine(Get(url, JudgeCapturePetHandler));
        }
        else
        {
            EventDispatcher.TriggerEvent("RefreshRoomStatus",false);
        }
    }

    private void JudgeCapturePetHandler(string json)
    {
        Debug.Log(json);
        JsonData data = JsonMapper.ToObject(json);
        if((int)data["code"] == 0)
        {
            Debug.Log("成功！");
            EventDispatcher.TriggerEvent("ShowRoomStatus");
        }
        else
        {
            Debug.Log("失败");
            EventDispatcher.TriggerEvent("RefreshRoomStatus",true);
        }
    }
    #endregion

    #region PrivateFunc
    private void GetRoomStatusHandler(string json)
    {
        JsonData data = JsonMapper.ToObject(json);
        if ((int)data["code"] == 0)
        {
            _roomStatus = JsonMapper.ToObject<RoomStatus>(json);
            if (_roomStatus.data.status == "end")
            {
                StartCoroutine(Get(_historyURL + _roomID, GetHistoryInfoHandler));
            }
            else
            {
                EventDispatcher.TriggerEvent("RefreshRoomStatus",false);
            }
        }
        else
        {
            Debug.LogError("Json Error " + data["msg"]);
        }
        StartCoroutine(Timer(_roomStatusUpdateInterval, RepateGetRoomStatusHandler));
    }

    private void RepateGetRoomStatusHandler()
    {
        StartCoroutine(Get(_roomStatusURL + _roomID, GetRoomStatusHandler));
    }

    private void GetHistoryInfoHandler(string json)
    {
        StopAllCoroutines();
        JsonData data = JsonMapper.ToObject(json);
        if ((int)data["code"] == 0)
        {
            _historyInfo = JsonMapper.ToObject<HistoryInfo>(json);
            if (_historyInfo.data.Length == 0)
            {
                Debug.LogError("The historyInfo Avatar Array is null!");
                return;
            }
            else
            {
                Debug.Log("");              //Why?
                _name_Sprite_Dic.Clear();
                for (int i = 0; i < _historyInfo.data.Length; ++i)
                {   
                    StartCoroutine(Get(_historyInfo.data[i].avatar, DownSpriteHandler,_historyInfo.data[i].username,_historyInfo.data.Length));
                }
            }
        }
        else
        {
            Debug.LogError("Json Error " + data["msg"]);
        }
    }

    private void DownSpriteHandler(WWW www,string userName,int finishCount)
    {
        Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
        if(_name_Sprite_Dic.ContainsKey(userName))
        {
            _name_Sprite_Dic[userName] = sprite;
        }
        else
        {
            _name_Sprite_Dic.Add(userName, sprite);
        }
        if(_name_Sprite_Dic.Count==finishCount)
        {
            EventDispatcher.TriggerEvent("ShowHistory");
        }
    }

    private IEnumerator Timer(int seconds, Action handler)
    {
        yield return new WaitForSecondsRealtime(seconds);
        handler.Invoke();
    }

    private IEnumerator Get(string url, AfterWWWGetHandler handler)
    {
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print("WWW error " + www.error);
        }
        else
        {
            handler(www.text);
        }
        www.Dispose();
    }

    private IEnumerator Get(string url, DownLoadHandler handler,string userName, int finishCount)
    {
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print("WWW error " + www.error);
        }
        else
        {
            handler(www, userName,finishCount);
        }
        www.Dispose();
    }

    private IEnumerator Post(string url, WWWForm form, AfterWWWPostHandler handler)
    {
        WWW www = new WWW(url, form);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print("WWW error " + www.error);
        }
        else
        {
            handler(www.text);
        }
        www.Dispose();
    }
    #endregion
}
