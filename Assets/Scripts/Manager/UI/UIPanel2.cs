using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRFrameWork;
using DG.Tweening;
using System;

public class UIPanel2 : MonoBehaviour
{
    //Delegate for Timer to Call per second
    public delegate void AfterOneSecondHandler(int lastSeconds);

    //Private
    private string _currentPetName;
    private bool _isCaughtPet;
   
    //LoadingPage
    private Transform _loadingPage;

    //HintPage
    private Transform _hintPage;
    private Transform _hintTransform;
    private Image _countDownImage;

    //CapturePage
    private Transform _capturePage;
    private Button _backButton;
    private Button _captureButton;
    private Transform _backCheck;
    private Button _yesButton;
    private Button _noButton;
    private Text _timer;
    private Image _aimImage;

    private Sprite _aimedSprite;
    private Sprite _notAimedSprite;

    //RoomStatus
    private Transform _roomStatusPage;
    private Button _roomStatusCloseButton;
    private Image _roomStatusPetIcon;
    private Text _roomStatusNumText;

    //HistoryInfo
    private Transform _historyInfoPage;
    private Button _historyInfoCloseButton;
    private Button _historyInfoYesButton;
    private Transform _layoutGroupTran;

    //A Timer and Call handler per second 
    private IEnumerator Timer(int seconds, AfterOneSecondHandler handler)
    {
        int newSecond = seconds;

        while (newSecond > -1)
        {
            handler.Invoke(newSecond);
            yield return new WaitForSecondsRealtime(1);
            --newSecond;
        }
    }

    private void Awake()
    {
        _loadingPage = transform.Find("LoadingPage");


        _hintPage = transform.Find("HintPage");
        _hintTransform = transform.Find("HintPage/HintTransform");
        _countDownImage = transform.GetComponentByPath<Image>("HintPage/CountDown/Image");


        _capturePage = transform.Find("CapturePage");
        _backButton = transform.GetComponentByPath<Button>("CapturePage/BackButton");
        _captureButton = transform.GetComponentByPath<Button>("CapturePage/CaptureButton");
        _backCheck = transform.Find("CapturePage/BackCheck");
        _yesButton = transform.GetComponentByPath<Button>("CapturePage/BackCheck/YesButton");
        _noButton = transform.GetComponentByPath<Button>("CapturePage/BackCheck/NoButton");
        _timer = transform.GetComponentByPath<Text>("CapturePage/CountDown/Timer");
        _aimImage = transform.GetComponentByPath<Image>("CapturePage/AimImage/AimImage_1");


        _roomStatusPage = transform.Find("RoomStatusPage");
        _roomStatusCloseButton = transform.GetComponentByPath<Button>("RoomStatusPage/BackGround/CloseButton");
        _roomStatusPetIcon = transform.GetComponentByPath<Image>("RoomStatusPage/BackGround/PetBG/PetIcon");
        _roomStatusNumText = transform.GetComponentByPath<Text>("RoomStatusPage/BackGround/PlayerNumber/NumberText");


        _historyInfoPage = transform.Find("HistoryInfoPage");
        _historyInfoCloseButton = transform.GetComponentByPath<Button>("HistoryInfoPage/BackGround/CloseButton");
        _historyInfoYesButton = transform.GetComponentByPath<Button>("HistoryInfoPage/BackGround/YesButton");
        _layoutGroupTran = transform.Find("HistoryInfoPage/BackGround/PlayerInfoBG/Mask/ScrollRect");
    }

    private void OnEnable()
    {
        EventDispatcher.AddEvent("ShowHistory", ShowHistoryInfoHandler);
        EventDispatcher.AddEvent("ShowRoomStatus", ShowRoomStatusHandler);
        EventDispatcher.AddEvent("RefreshRoomStatus", RefreshRoomStatusHandler);
    }

    private void OnDisable()
    {
        EventDispatcher.RemoveEvent("ShowHistory", ShowHistoryInfoHandler);
        EventDispatcher.RemoveEvent("ShowRoomStatus", ShowRoomStatusHandler);
        EventDispatcher.RemoveEvent("RefreshRoomStatus", RefreshRoomStatusHandler);

    }

    private void Start()
    {
        _aimedSprite= Resources.Load<GameObject>("Sprites/Prefabs/CapturedAim").GetComponent<SpriteRenderer>().sprite;
        _notAimedSprite= Resources.Load<GameObject>("Sprites/Prefabs/NotCaptureAim").GetComponent<SpriteRenderer>().sprite;

        InitCapture();
    }

    /// <summary>
    /// The function 
    /// </summary>
    public void  InitCapture()
    {
        _currentPetName = null;
        _isCaughtPet = false;
        HideLoadingPage();
        InitHint();
    }


    #region HintAndCountDown
    private void HideLoadingPage()
    {
        if(_loadingPage.gameObject.activeInHierarchy)
        {
            _loadingPage.gameObject.SetActive(false);
        }
    }

    private void InitHint()
    {
        _capturePage.gameObject.SetActive(false);
        _roomStatusPage.gameObject.SetActive(false);
        _historyInfoPage.gameObject.SetActive(false);
        _hintPage.gameObject.SetActive(true);
        _countDownImage.gameObject.SetActive(false);
        Vector3 localPoint = _hintTransform.localPosition;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(_hintTransform.DOLocalMoveX(-120, 0.25f));
        mySequence.Append(_hintTransform.DOLocalMoveX(50, 0.25f));
        mySequence.Append(_hintTransform.DOLocalMoveX(0, 0.25f).SetEase(Ease.OutSine));
        mySequence.OnComplete(new TweenCallback(() => { StartCoroutine(Timer(5, Count5DownHandler)); }));
    }

    private void Count5DownHandler(int lastSeconds)
    {
        if(lastSeconds==0)
        {
            _hintPage.gameObject.SetActive(false);
            StartCapture();
            return;
        }
        Sprite sprite = Resources.Load<GameObject>("Sprites/Prefabs/" + lastSeconds.ToString()).GetComponent<SpriteRenderer>().sprite;
        _countDownImage.sprite = sprite;
        if(!_countDownImage.gameObject.activeInHierarchy)
        {
            _countDownImage.gameObject.SetActive(true);
        }
    }
    #endregion

    #region CaptureLogic
   // ShowCapturePage after CountDown
    private void StartCapture()
    {
        _captureButton.gameObject.SetActive(false);
        _roomStatusPage.gameObject.SetActive(false);
        _historyInfoPage.gameObject.SetActive(false);
        _backCheck.gameObject.SetActive(false);
        StartCoroutine(Timer(600, Count10minHandler));
        _capturePage.localScale = Vector3.zero;
        _capturePage.gameObject.SetActive(true);
        _capturePage.DOScale(1, 0.3f).SetUpdate(true);
        UIEventListener.AddUIListener(_backButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, new UIEventHandler(BackButtonOnClickHandler));
        // to do Show pet
        
        EventDispatcher.AddEvent("OnAimed", OnAimedHandler);
        EventDispatcher.AddEvent("OnNotAimed", OnNotAimedHandler);
        EventDispatcher.AddEvent<string>("OnCapture", OnCaptureHandler);
    }

    //The handler for timer to countdown 10 minutes and will finish the game after 10min
    private void Count10minHandler(int lastSeconds)
    {
        if (lastSeconds == 0)
        {
            EndCapture();
            OSBridgeManager.Instance.OnUnityBack(_isCaughtPet?1:0);
        }
        int hour = lastSeconds / 3600;
        int minute = (lastSeconds % 3600) / 60;
        int second = (lastSeconds % 60);
        _timer.text = (hour > 0 ? (hour <= 9 ? "0" + hour.ToString() : hour.ToString()) : "00") + ": " +
           (minute > 0 ? (minute <= 9 ? "0" + minute.ToString() : minute.ToString()) : "00") + ": " +
           (second > 0 ? (second <= 9 ? "0" + second.ToString() : second.ToString()) : "00");
    }


     //Aim image
    private void OnAimedHandler()
    {
        if(_aimImage.sprite!=_aimedSprite)
        {
            _aimImage.sprite = _aimedSprite;
        }
    }


    //Aim image
    private void OnNotAimedHandler()
    {
        if(_aimImage.sprite!=_notAimedSprite)
        {
            _aimImage.sprite = _notAimedSprite;
        }
    }

    private void OnCaptureHandler(string petName)
    {
        _currentPetName = petName;
        StartCoroutine(Timer(3, GoOnCaptureHandler));
    }

    private void GoOnCaptureHandler(int lastSeconds)
    {
        if(lastSeconds==0)
        {
            _captureButton.gameObject.SetActive(false);
            UIEventListener.RemoveUIListener(_captureButton.gameObject);
            UIEventListener.RemoveUIListener(_aimImage.gameObject);
            if (Time.timeScale!=1)
            {
                Time.timeScale = 1;
            }
            _currentPetName = null;
        }
        else
        {
            if(!_captureButton.gameObject.activeInHierarchy)
            {
                _captureButton.gameObject.SetActive(true);
                UIEventListener.AddUIListener(_captureButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, new UIEventHandler(CaptureButtonOnClickHandler));
                UIEventListener.AddUIListener(_aimImage.gameObject).SetEventHandler(EnumUIinputType.OnClick, new UIEventHandler(CaptureButtonOnClickHandler));
            }
            if(Time.timeScale!=0)
            {
                Time.timeScale = 0;
            }
        }
    }

    
    //Function of BackButton 
    private void BackButtonOnClickHandler(GameObject linster, object _arg, object[] _params)
    {
        if(!_backCheck.gameObject.activeSelf)
        {
            ShowBackCheck();
        }
    }


    //Show BackCheck after BackButton is Click
    private void ShowBackCheck()
    {
        _backCheck.transform.localScale = Vector3.zero;
        _backCheck.gameObject.SetActive(true);
        Tweener tweener = _backCheck.DOScale(1, 0.3f);
        tweener.OnComplete(new TweenCallback(() => { Time.timeScale = 0; }));
        UIEventListener.AddUIListener(_yesButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, new UIEventHandler(YesButtonOnClickHandler));
        UIEventListener.AddUIListener(_noButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, new UIEventHandler(NoButtonOnClickHandler));
    }
 
    //Function of NoButton on BackCheck
    private void NoButtonOnClickHandler(GameObject linster, object _arg, object[] _params)
    {
        HideBackCheck();
    }

    //Hide BackCheck and GoOn Game after NoButton is Been pressed
    private void HideBackCheck()
    {
        UIEventListener.RemoveUIListener(_yesButton.gameObject);
        UIEventListener.RemoveUIListener(_noButton.gameObject);
        Tweener tweener = _backCheck.DOScale(0, 0.1f).SetUpdate(true);
        tweener.OnComplete(new TweenCallback(() => { Time.timeScale = 1; _backCheck.gameObject.SetActive(false); }));
    }

    //Function of YesButton on BackCheck
    private void YesButtonOnClickHandler(GameObject linster, object _arg, object[] _params)
    {
        EndCapture();
        OSBridgeManager.Instance.OnUnityBack(0);
    }


    //Function be call when captureButton has been pressed
    private void CaptureButtonOnClickHandler(GameObject linster, object _arg, object[] _params)
    {
        OSBridgeManager.Instance.JudgeCapturePetOnServer(_currentPetName);
        Debug.Log(_currentPetName);
    }

    private void EndCapture()
    {
        StopAllCoroutines();
        _captureButton.gameObject.SetActive(false);
        _backCheck.gameObject.SetActive(false);
        UIEventListener.RemoveUIListener(_backButton.gameObject);
        EventDispatcher.RemoveEvent("OnAimed", OnAimedHandler);
        EventDispatcher.RemoveEvent("OnNotAimed", OnNotAimedHandler);
        EventDispatcher.RemoveEvent<string>("OnCapture", OnCaptureHandler);
        _capturePage.gameObject.SetActive(false);
    }
    #endregion

    #region RoomStatusLogic

    private void ShowRoomStatusHandler()
    {
        EndCapture();
        _isCaughtPet = true;
        if(_currentPetName==null)
        {
            Debug.LogError("Big Error!");
        }
        _roomStatusPetIcon.sprite = Resources.Load<Sprite>("Sprites/PetIcon/" + _currentPetName);
        _capturePage.gameObject.SetActive(false);
        _hintPage.gameObject.SetActive(false);
        _historyInfoPage.gameObject.SetActive(false);
        _roomStatusPage.localScale = Vector3.zero;
        _roomStatusPage.gameObject.SetActive(true);
        _roomStatusPage.DOScale(1, 0.3f).SetUpdate(true);

        UIEventListener.AddUIListener(_roomStatusCloseButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, RoomStatusCloseButtonOnClickhandler);
    }

    private void EndRoomStatus()
    {
        _roomStatusPage.gameObject.SetActive(false);
        UIEventListener.RemoveUIListener(_roomStatusCloseButton.gameObject);
    }

    private void RoomStatusCloseButtonOnClickhandler(GameObject linster, object _arg, object[] _params)
    {
        EndRoomStatus();
        OSBridgeManager.Instance.OnUnityBack(1);
    }

    private void RefreshRoomStatusHandler()
    {
        Debug.Log(11);
        string cont = OSBridgeManager.Instance.GetRoomStatusPlayerNum();
        if(cont!=null)
        {
            _roomStatusNumText.text = cont;
        }
    }

    #endregion

    #region HistoryInfoLogic

    private void ShowHistoryInfoHandler()
    {
        EndCapture();
        EndRoomStatus();
        string[] userNames = OSBridgeManager.Instance.GetUserNames();
        if(userNames==null)
        {
            Debug.LogError("The userNames is null!");
            return;
        }

        if(_layoutGroupTran.childCount!=0)
        {
            for(int i=0;i<_layoutGroupTran.childCount;++i)
            {
                _layoutGroupTran.GetChild(i).gameObject.SetActive(false);
            }
        }

        for(int i=0;i<userNames.Length;++i)
        {
            if (i< _layoutGroupTran.childCount)
            {
                _layoutGroupTran.GetChild(i).GetComponentByPath<Text>("BG/NameText").text = userNames[i];
                _layoutGroupTran.GetChild(i).GetComponentByPath<Image>("BG/PlayerIcon").sprite = OSBridgeManager.Instance.GetSpriteByName(userNames[i]);
                _layoutGroupTran.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                GameObject prefab = Instantiate(Resources.Load<GameObject>("Sprites/Prefabs/UserInfoItem"), _layoutGroupTran, false);
                prefab.GetComponentByPath<Text>("BG/NameText").text = userNames[i];
                prefab.GetComponentByPath<Image>("BG/PlayerIcon").sprite = OSBridgeManager.Instance.GetSpriteByName(userNames[i]); 
            }
        }

        _capturePage.gameObject.SetActive(false);
        _hintPage.gameObject.SetActive(false);
        _roomStatusPage.gameObject.SetActive(false);
        _historyInfoPage.gameObject.SetActive(false);
        _historyInfoPage.localScale = Vector3.zero;
        _historyInfoPage.gameObject.SetActive(true);
        _historyInfoPage.DOScale(1, 0.3f).SetUpdate(true);

        UIEventListener.AddUIListener(_historyInfoCloseButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, OnHistoryInfoCloseClickHandler);
        UIEventListener.AddUIListener(_historyInfoYesButton.gameObject).SetEventHandler(EnumUIinputType.OnClick, OnHistoryInfoCloseClickHandler);
    }

    private void OnHistoryInfoCloseClickHandler(GameObject linster, object _arg, object[] _params)
    {
        UIEventListener.RemoveUIListener(_historyInfoCloseButton.gameObject);
        UIEventListener.RemoveUIListener(_historyInfoYesButton.gameObject);
        _historyInfoPage.gameObject.SetActive(false);
        OSBridgeManager.Instance.OnUnityBack(_isCaughtPet ? 1 : 0);
    }

    #endregion
}
