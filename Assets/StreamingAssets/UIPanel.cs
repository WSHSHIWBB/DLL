using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using VRFrameWork;
using DG.Tweening;

public class UIPanel : MonoBehaviour
{
    public float CaptureButtonShowTime = 2f;
    public LookAtCamera lookAtCamera;
    private NativeBridge nativeBridge;
    private Button backButton;
    private Button CaptureButton;
    private Button backButton_0;
    private Button backButton_1;
    private Button backButton_2;
    private Button remoteButton;
    private Button notButton;
    private Button goOnButton;
    private Button iknowButton;
    private Text timerText;
    private Text metterText;
    private Text contentText;
    private Text PetNameText;
    private Text PetTypeText;
    private Text MoneyText;
    private Image AimImage;
    private Image PetIcon;
    private GameObject PetCapture;
    private GameObject WaitTimer;
    private GameObject OverDistance;
    private GameObject LoadingPage;
    private GameObject BackCheck;
    private GameObject CaptureCheck;
    private Transform HintTransform;
    private Vector3 HintInitialPosition;

	void Start ()
    {
        nativeBridge = GameObject.Find("WorldRotate").GetComponent<NativeBridge>();
        PetCapture = transform.Find("PetCapture").gameObject;
        WaitTimer = transform.Find("WaitTimer").gameObject;
        OverDistance = transform.Find("OverDistance").gameObject;
        LoadingPage = transform.Find("LoadingPage").gameObject;
        BackCheck = transform.Find("PetCapture/BackCheck").gameObject;
        CaptureCheck = transform.Find("PetCapture/CaptureCheck").gameObject;
        HintTransform = transform.Find("PetCapture/Hint");

        backButton = transform.GetComponentByPath<Button>("PetCapture/BackButton");
        CaptureButton = transform.GetComponentByPath<Button>("PetCapture/CaptureButton");
        backButton_0 = transform.GetComponentByPath<Button>("WaitTimer/BackButton_0");
        backButton_1 = transform.GetComponentByPath<Button>("OverDistance/BackButton_1");
        backButton_2 = transform.GetComponentByPath<Button>("OverDistance/BackButton_2");
        remoteButton = transform.GetComponentByPath<Button>("OverDistance/RemoteButton");
        notButton = transform.GetComponentByPath<Button>("PetCapture/BackCheck/NotButton");
        goOnButton = transform.GetComponentByPath<Button>("PetCapture/BackCheck/GoOnButton");
        iknowButton = transform.GetComponentByPath<Button>("PetCapture/CaptureCheck/IKnowButton");

        timerText = transform.GetComponentByPath<Text>("WaitTimer/Timer");
        metterText = transform.GetComponentByPath<Text>("OverDistance/Describe/Together/Ｍetter");
        contentText = transform.GetComponentByPath<Text>("OverDistance/Describe/Ｃontent");
        PetNameText = transform.GetComponentByPath<Text>("PetCapture/CaptureCheck/Pet/Name");
        PetTypeText = transform.GetComponentByPath<Text>("PetCapture/CaptureCheck/Pet/Type");

        MoneyText = transform.GetComponentByPath<Text>("PetCapture/CaptureCheck/Money/Amount");

        AimImage = transform.GetComponentByPath<Image>("PetCapture/AimImage/AimImage_1");
        PetIcon = transform.GetComponentByPath<Image>("PetCapture/CaptureCheck/Pet/Head");
       
        CaptureButton.onClick.AddListener(new UnityAction(OnCaptureClick));
        backButton_0.onClick.AddListener(new UnityAction(OnExitClick));
        backButton_1.onClick.AddListener(new UnityAction(OnExitClick));
        backButton_2.onClick.AddListener(new UnityAction(OnExitClick));
        remoteButton.onClick.AddListener(new UnityAction(OnRemoteClick));
        notButton.onClick.AddListener(new UnityAction(OnNotClick));
        goOnButton.onClick.AddListener(new UnityAction(OnGoOnClick));

        CaptureButton.gameObject.SetActive(false);
    }

    private void OnGoOnClick()
    {
        BackCheck.SetActive(false);
    }

    private void OnNotClick()
    {
        BackCheck.SetActive(false);
        CaptureCheck.SetActive(false);
        OverDistance.SetActive(false);
        WaitTimer.SetActive(false);
        backButton.onClick.RemoveAllListeners();
        StopCoroutine("Timer");
        EventDispatcher.TriggerEvent("ResetPet");
#if UNITY_ANDROID && !UNITY_EDITOR
        nativeBridge.javaObject.Call("onBack", 0);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
        nativeBridge.OnUnityBack(0);
#endif
    }

    private void OnCapturePetHandler(string message)
    {
        var contents = message.Split('|');
        string petNum = contents[0];
        string petName = contents[1];
        string petType = contents[2];
        string money = contents[3];
        if (LoadingPage.activeSelf)
        {
            LoadingPage.SetActive(false);
        }
        StopCoroutine("Timer");
        backButton.onClick.AddListener(new UnityAction(OnBackClick));
        iknowButton.onClick.AddListener(new UnityAction(OnCaptrueRet));

        PetIcon.sprite =Instantiate(Resources.Load<Sprite>("Sprites/PetIcon/" + petNum));
        PetNameText.text = petName;
        PetTypeText.text = petType;
        MoneyText.text = money;

        PetCapture.SetActive(true);
        OverDistance.SetActive(false);
        WaitTimer.SetActive(false);
        HintInitialPosition = HintTransform.localPosition;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScale(1, 1));
        mySequence.Append(HintTransform.DOLocalMoveX(-200,0.25f));
        mySequence.Append(HintTransform.DOLocalMoveX(50, 0.25f));
        mySequence.Append(HintTransform.DOLocalMoveX(0, 0.25f).SetEase(Ease.OutSine));
        mySequence.OnComplete(new TweenCallback(() => { StartCoroutine(SetHintInActive(2)); }));
    }

    private IEnumerator SetHintInActive(float dur)
    {
        yield return new WaitForSeconds(dur);
        HintTransform.localPosition = HintInitialPosition;
        EventDispatcher.TriggerEvent("GoOnCapture");
    }

    private void OnShowTimeCoolHandler(int time)
    {
        if (LoadingPage.activeSelf)
        {
            LoadingPage.SetActive(false);
        }
        StopCoroutine("Timer");
        backButton.onClick.AddListener(new UnityAction(OnDirectBack));
        OverDistance.SetActive(false);
        WaitTimer.SetActive(true);
        StartCoroutine("Timer",time);
    }

    private IEnumerator Timer(int seconds)
    {
        int newSecond = seconds;

        while (newSecond > -1)
        {
            int hour = (int)newSecond / 3600;
            int minute = (int)(newSecond % 3600) / 60;
            int second = (int)(newSecond % 60);
            timerText.text = (hour > 0 ?(hour<=9?"0"+hour.ToString():hour.ToString()):"00") + ": " +
            (minute > 0 ?(minute<=9?"0"+minute.ToString():minute.ToString()):"00") + ": " + 
            (second > 0 ?(second<=9?"0"+second.ToString():second.ToString()):"00");

            yield return new WaitForSeconds(1);
            --newSecond;
        }
    }

    private void OnShowOverDistanceHandler(int distance)
    {
        if (LoadingPage.activeSelf)
        {
            LoadingPage.SetActive(false);
        }
        backButton.onClick.AddListener(new UnityAction(OnDirectBack));
        WaitTimer.SetActive(false);
        OverDistance.SetActive(true);
        metterText.text = distance.ToString();
        if (distance>100)
        {
            contentText.text = @"请“返回”，前往目的地再捉宠～";
            backButton_2.gameObject.SetActive(true);
            backButton_1.gameObject.SetActive(false);
            remoteButton.gameObject.SetActive(false);
        }
        else if(distance>50&&distance<=100)
        {
            contentText.text = "已非常接近，您可以选择：";
            backButton_2.gameObject.SetActive(false);
            backButton_1.gameObject.SetActive(true);
            remoteButton.gameObject.SetActive(true);
        }
    }

    private void ShowCaptureButtonHandler()
    {
        if (!CaptureButton.gameObject.activeSelf)
        {
            CaptureButton.gameObject.SetActive(true);
        }
        UIEventListener.AddUIListener(AimImage.gameObject).SetEventHandler(EnumUIinputType.OnClick, OnAimClickHandler, null);
        StartCoroutine("HideCaptureButton", CaptureButtonShowTime);
        StartCoroutine("GoOnCapture", CaptureButtonShowTime);
    }

    IEnumerator HideCaptureButton(float time)
    {
        yield return new WaitForSeconds(time);
        CaptureButton.gameObject.SetActive(false);
        if(AimImage.GetComponent<UIEventListener>())
        {
            var component = AimImage.GetComponent<UIEventListener>();
            DestroyImmediate(component);
        } 
    }

    IEnumerator GoOnCapture(float time)
    {
        yield return new WaitForSeconds(time);
        EventDispatcher.TriggerEvent("GoOnCapture");
    }

    private void OnAimClickHandler(GameObject linster, object _arg, object[] _params)
    {
        OnCaptureClick();
    }

    private void OnExitClick()
    {
        EventDispatcher.TriggerEvent("ResetPet");
        backButton.onClick.RemoveAllListeners();
        StopCoroutine("Timer");
        WaitTimer.SetActive(false);
        OverDistance.SetActive(false);
        BackCheck.SetActive(false);
#if UNITY_ANDROID && !UNITY_EDITOR
        nativeBridge.javaObject.Call("onBack",0);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
        nativeBridge.OnUnityBack(0);
#endif
    }

    private void OnRemoteClick()
    {
        OverDistance.SetActive(false);
        backButton.onClick.RemoveAllListeners();
#if UNITY_ANDROID && !UNITY_EDITOR
        nativeBridge.javaObject.Call("onBack",2);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
        nativeBridge.OnUnityBack(2);
#endif
    }

    private void OnDirectBack()
    {
        backButton.onClick.RemoveAllListeners();
        StopCoroutine("Timer");
        BackCheck.SetActive(false);
        EventDispatcher.TriggerEvent("ResetPet");
#if UNITY_ANDROID && !UNITY_EDITOR
        nativeBridge.javaObject.Call("onBack", 0);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
        nativeBridge.OnUnityBack(0);
#endif
    }

    private void OnCaptrueRet()
    {
        StopCoroutine("Timer");
        CaptureCheck.SetActive(false);
        iknowButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        BackCheck.SetActive(false);
        EventDispatcher.TriggerEvent("ResetPet");
#if UNITY_ANDROID && !UNITY_EDITOR
        nativeBridge.javaObject.Call("onBack", 1);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
        nativeBridge.OnUnityBack(1);
#endif
    }

    private void OnBackClick()
    {
        if (BackCheck.activeSelf == false)
        {
            BackCheck.transform.localScale = Vector3.zero;
            BackCheck.SetActive(true);
            BackCheck.transform.DOScale(1, 0.3f);
        }
    }

    private void OnCaptureClick()
    {
        StopCoroutine("GoOnCapture");
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnCaptrueRet);
        if (CaptureCheck.activeSelf==false)
        {
            CaptureCheck.transform.localScale = Vector3.zero;
            CaptureCheck.SetActive(true);
            CaptureCheck.transform.DOScale(1, 0.3f);
        }
    }

    private void OnHideCaptureButtonHandler()
    {
        CaptureButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventDispatcher.AddEvent("OnCaptured", ShowCaptureButtonHandler);
        EventDispatcher.AddEvent("HideCaptureButton", OnHideCaptureButtonHandler);
        EventDispatcher.AddEvent<string>("CapturePet", OnCapturePetHandler);
        EventDispatcher.AddEvent<int>("ShowTimeCool", OnShowTimeCoolHandler);
        EventDispatcher.AddEvent<int>("ShowOverDistance", OnShowOverDistanceHandler);
    }

    private void OnDisable()
    {
        EventDispatcher.RemoveEvent("OnCaptured", ShowCaptureButtonHandler);
        EventDispatcher.RemoveEvent("HideCaptureButton", OnHideCaptureButtonHandler);
        EventDispatcher.RemoveEvent<string>("CapturePet", OnCapturePetHandler);
        EventDispatcher.RemoveEvent<int>("ShowTimeCool", OnShowTimeCoolHandler);
        EventDispatcher.RemoveEvent<int>("ShowOverDistance", OnShowOverDistanceHandler);
    }
}
