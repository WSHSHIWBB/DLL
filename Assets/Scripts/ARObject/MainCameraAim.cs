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

    private float lastTime=0;
    private bool isCapturePaused = false;
    private Transform worldRotate;
    private Transform LookAtCamera;
    private AudioSource audioSource;
    private AudioClip slowAlarmClip;
    private AudioClip fastAlarmClip;
    private GyroController gyroController;

    private void Start()
    {
        worldRotate = GameObject.Find("WorldRotate").transform;
        LookAtCamera = worldRotate.GetChild(0).GetChild(0);
        audioSource = GetComponent<AudioSource>();
        slowAlarmClip = Instantiate(Resources.Load<AudioClip>("Audio/Slow"));
        fastAlarmClip = Instantiate(Resources.Load<AudioClip>("Audio/Fast"));
        gyroController = GetComponent<GyroController>();
    }

    void Update ()
    {
        if (!isCapturePaused)
        {
            int x = Screen.width / 2;
            int y = Screen.height / 2;
            Ray aimRay = Camera.main.ScreenPointToRay(new Vector3(x, y));
            RaycastHit hit;
            if (Physics.Raycast(aimRay, out hit, 100f))
            {
                if (hit.collider.tag == "Pet")
                {
                    EventDispatcher.TriggerEvent("OnAimed");
                    lastTime += Time.deltaTime;
                    if (lastTime >= captureTime)
                    {
                        string name = hit.collider.attachedRigidbody.name.Replace("(Clone)", "");
                        EventDispatcher.TriggerEvent("OnCapture", name);
                        lastTime = 0;
                    }
                }
            }
            else
            {
                EventDispatcher.TriggerEvent("OnNotAimed");
                lastTime = 0f;
            }

            Vector3 cameraDir = transform.forward;
            Vector3 petDir = worldRotate.position.normalized;
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
            if(audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
	}

    private void PlayAlarm(AudioClip clip,float volume,float pitch)
    {
        if(audioSource.isPlaying&&audioSource.clip!=clip || !audioSource.isPlaying)
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

    private void ResetPetHandler()
    {
        transform.rotation = Quaternion.identity;
    }

    private void PauseCaptureHandler()
    {
        gyroController.DetachGyro();
        isCapturePaused = true;
        lastTime = 0;
        transform.LookAt(LookAtCamera.position);
        GameObject particle =Instantiate(Resources.Load<GameObject>("Particle/1"));
        particle.transform.SetParent(LookAtCamera);
        particle.transform.localPosition = new Vector3(0,0,1f);
        particle.transform.localRotation = Quaternion.identity;
    }

    private void GoOnCaptureHandler()
    {
        isCapturePaused = false;
        gyroController.AttachGyro();
    }

    private void PauseMainCameAimHandler()
    {
        gyroController.DetachGyro();
        isCapturePaused = true;
        lastTime = 0;
    }

   
    /*
    private void OnEnable()
    {
        
        EventDispatcher.AddEvent("OnCapture",PauseCaptureHandler);
        EventDispatcher.AddEvent("GoOnCapture", GoOnCaptureHandler);
        EventDispatcher.AddEvent("PauseMainCameAim", PauseMainCameAimHandler);
        EventDispatcher.AddEvent("ResetPet", ResetPetHandler);

    }

    private void OnDisable()
    {
        EventDispatcher.RemoveEvent("OnCapture",PauseCaptureHandler);
        EventDispatcher.RemoveEvent("GoOnCapture", GoOnCaptureHandler);
        EventDispatcher.RemoveEvent("PauseMainCameAim", PauseMainCameAimHandler);
        EventDispatcher.RemoveEvent("ResetPet", ResetPetHandler);
    }
    */

}
