using UnityEngine;
using UnityEngine.UI;
using VRFrameWork;

public class MainCameraAim : MonoBehaviour
{
    private float ALARM_LEVEL_1 = 60;
    private float ALARM_LEVEL_2 = 45;
    private float ALARM_LEVEL_3 = 30;
    private float ALARM_LEVEL_4 = 15;

    [SerializeField]
    private float captureTime = 1f;
    private float lastTime = 0;
    private bool isCapturePaused = true;

    private AudioSource audioSource;
    private AudioClip slowAlarmClip;
    private AudioClip fastAlarmClip;
    private GyroController gyroController;

    private Transform Pet1;
    private Transform Pet2;
    private Transform Pet3;

    private void OnEnable()
    {
		EventDispatcher.AddEvent("InitCapture", InitCameraHandler);
        EventDispatcher.AddEvent<string>("OnCapture", PauseCaptureHandler);
        EventDispatcher.AddEvent("GoOnCapture", GoOnCaptureHandler);   
    }

    private void OnDisable()
    {
		EventDispatcher.RemoveEvent("InitCapture", InitCameraHandler);
        EventDispatcher.RemoveEvent<string>("OnCapture", PauseCaptureHandler);
        EventDispatcher.RemoveEvent("GoOnCapture", GoOnCaptureHandler);     
    }

    private void Start()
    {
        Pet1 = GameObject.Find("Pet1") ? GameObject.Find("Pet1").transform : null;
        Pet2 = GameObject.Find("Pet2") ? GameObject.Find("Pet2").transform : null;
        Pet3 = GameObject.Find("Pet3") ? GameObject.Find("Pet3").transform : null;
        audioSource = GetComponent<AudioSource>();
        slowAlarmClip = Instantiate(Resources.Load<AudioClip>("Audio/Slow"));
        fastAlarmClip = Instantiate(Resources.Load<AudioClip>("Audio/Fast"));
        gyroController = GetComponent<GyroController>();   
    }


    void Update()
    {
        if (!isCapturePaused)
        {
            int x = Screen.width / 2;
            int y = Screen.height / 2;
            Ray aimRay = Camera.main.ScreenPointToRay(new Vector3(x, y));
            RaycastHit hit;
            if (Physics.Raycast(aimRay, out hit, 100f))
            {
                if (hit.collider.attachedRigidbody.tag == "Pet")
                {
                    EventDispatcher.TriggerEvent("OnAimed");
                    lastTime += Time.deltaTime;
                    if (lastTime >= captureTime)
                    {
                        string name = hit.collider.attachedRigidbody.name.Replace("(Clone)", "");
                        EventDispatcher.TriggerEvent("OnCapture", name);
                        lastTime = 0;
                        transform.LookAt(hit.collider.transform);
                    }
                }
            }
            else
            {
                EventDispatcher.TriggerEvent("OnNotAimed");
                lastTime = 0f;
            }
        }

        ControlAlarm(isCapturePaused, Pet1);
        ControlAlarm(isCapturePaused, Pet2);
        ControlAlarm(isCapturePaused, Pet3);
    }

    private void ControlAlarm(bool isPaused, Transform petTrans)
    {
        if(petTrans==null)
        {
            return;
        }
        if (!isCapturePaused)
        {
            Vector3 cameraDir = transform.forward;
            Vector3 petDir = petTrans.position.normalized;
            float cam_pet_Angle = Vector3.Angle(cameraDir, petDir);
            if (cam_pet_Angle >= 0 && cam_pet_Angle < ALARM_LEVEL_4)
            {
                PlayAlarm(fastAlarmClip, 0.3f, 2f);
            }
            else if (cam_pet_Angle >= ALARM_LEVEL_4 && cam_pet_Angle < ALARM_LEVEL_3)
            {
                PlayAlarm(fastAlarmClip, 0.2f, 1f);
            }
            else if (cam_pet_Angle >= ALARM_LEVEL_3 && cam_pet_Angle < ALARM_LEVEL_2)
            {
                PlayAlarm(slowAlarmClip, 0.2f, 1f);
            }
            else if (cam_pet_Angle >= ALARM_LEVEL_2 && cam_pet_Angle < ALARM_LEVEL_1)
            {
                PlayAlarm(slowAlarmClip, 0.2f, 0.5f);
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlayAlarm(AudioClip clip, float volume, float pitch)
    {
        if (audioSource.isPlaying && audioSource.clip != clip || !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }
    }

    private void InitCameraHandler()
    {
        transform.rotation = Quaternion.identity;
        isCapturePaused = false;
    }

    private void PauseCaptureHandler(string name)
    {
        gyroController.DetachGyro();
        lastTime = 0;
        isCapturePaused = true;
    }

    private void GoOnCaptureHandler()
    {
        gyroController.AttachGyro();
        isCapturePaused = false;
    }

}
