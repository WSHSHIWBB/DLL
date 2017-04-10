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
    //Delegate for afer WWW class Get Successfully
    public delegate void AfterWWWGetHandler(string json);
    //Delegate for afer WWW class Post Successfully
    public delegate void AfterWWWPostHandler(string json);

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
    private Transform _countDown;
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
    }

    private IEnumerator Post(string url,WWWForm form,AfterWWWPostHandler handler)
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
        _countDown = transform.Find("CapturePage/CountDown");
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

        EventDispatcher.AddEvent("OnAimed", OnAimedHandler);
        EventDispatcher.AddEvent("OnNotAimed", OnNotAimedHandler);
        EventDispatcher.AddEvent("OnCapture", OnCaptureHandler);
    }

    //The handler for timer to countdown 10 minutes and will finish the game after 10min
    private void Count10minHandler(int lastSeconds)
    {
        if (lastSeconds == 0)
        {
            EndCapture();

            //to do
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


    private void OnCaptureHandler()
    {
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
        //to be end capture and failed
    }


    //Function be call when captureButton has been pressed
    private void CaptureButtonOnClickHandler(GameObject linster, object _arg, object[] _params)
    {
        EndCapture();
        StartRoomStatus();
    }

    private void EndCapture()
    {
        StopAllCoroutines();
        _captureButton.gameObject.SetActive(false);
        _backCheck.gameObject.SetActive(false);
        UIEventListener.RemoveUIListener(_backButton.gameObject);
        EventDispatcher.RemoveEvent("OnAimed", OnAimedHandler);
        EventDispatcher.RemoveEvent("OnNotAimed", OnNotAimedHandler);
        EventDispatcher.RemoveEvent("OnCapture", OnCaptureHandler);
        _capturePage.gameObject.SetActive(false);
    }
    #endregion

    #region RoomStatusLogic

    private void StartRoomStatus()
    {
        _capturePage.gameObject.SetActive(false);
        _hintPage.gameObject.SetActive(false);
        _historyInfoPage.gameObject.SetActive(false);
        _roomStatusPage.localScale = Vector3.zero;
        _roomStatusPage.gameObject.SetActive(true);
        _roomStatusPage.DOScale(1, 0.3f).SetUpdate(true);
    }

    #endregion

    #region HistoryInfoLogic

    #endregion
}
