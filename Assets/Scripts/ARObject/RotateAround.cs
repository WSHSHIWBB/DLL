using UnityEngine;
using System.Collections;
using VRFrameWork;
using DG.Tweening;


public class RotateAround : MonoBehaviour
{

    private Vector3 AxisDir = Vector3.up;
    private const float PETDIS = 4.5f;
    private Vector3 rotatePoint;
    private Transform LookAtCamera;
    private Camera mainCamera;
    private float zMinDis;
    private Animation screenAnimation;
    private float lastAnimationTime;
    private string lastAnimationName = "screenmove1";
    private ConfigModule configModule;
    private bool isCapturePaused = true;
    private GyroController gyroController;
	private Transform _pet;
	private float _rotateSpeed;

    void Start()
    {
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}

		rotatePoint = mainCamera.transform.position;
		LookAtCamera = transform.Find("LevelAxis").GetChild(0);
		gyroController = mainCamera.GetComponent<GyroController>();
		configModule = ModuleManager.Instance.Get<ConfigModule>();
		screenAnimation = transform.GetChild(0).GetComponent<Animation>();
		_pet = LookAtCamera.GetChild (0);
		InitialRotate();
        //StartCoroutine(GameHelper(28));
    }

    private void OnEnable()
    {
        EventDispatcher.AddEvent("InitCapture", InitialRotate);
        EventDispatcher.AddEvent<string>("OnCapture", RotatePauseHandler);
        EventDispatcher.AddEvent("GoOnCapture", RotateGoOnHandler);
		EventDispatcher.AddEvent("ShowPet", ShowPetHandler);
		EventDispatcher.AddEvent<bool> ("RefreshRoomStatus", RefreshRoomStatusHandler);
    }

    private void OnDisable()
    {
		EventDispatcher.RemoveEvent("InitCapture", InitialRotate);
        EventDispatcher.RemoveEvent<string>("OnCapture", RotatePauseHandler);
        EventDispatcher.RemoveEvent("GoOnCapture", RotateGoOnHandler);
		EventDispatcher.RemoveEvent("ShowPet", ShowPetHandler);
		EventDispatcher.RemoveEvent<bool> ("RefreshRoomStatus", RefreshRoomStatusHandler);
    }

    private void InitialRotate()
    {
		float low;
		float high;
		switch (_pet.name) 
		{
		case "普通宠物":
			low = 40;
			high = 90;
			_rotateSpeed = 0.9f;			
			break;
		case "高级宠物":
			low = 130;
			high = 200;
			_rotateSpeed = 1.1f;
			break;
		case "金宠":
			low = 240;
			high = 320;
			_rotateSpeed = 1.3f;
			break;
		default:
			Debug.LogError ("petName Error!");
			return;
		}
		StartCoroutine(WorldRotate(false, true, Random.Range(low, high)));
        lastAnimationTime = RandomUtil.Range(0, screenAnimation[lastAnimationName].length);
        HandleScreenMoveAnimation(true);
		SetPetActive (false);
    }

	private void SetPetActive(bool isActive)
	{
		if (isActive) {
			if (_pet.gameObject.activeInHierarchy) {
				_pet.gameObject.SetActive (true);
			}
		} else {
			_pet.gameObject.SetActive (false);
		}
	}

	private void ShowPetHandler()
	{
		bool isInit = OSBridgeManager.Instance.IsRoomStatusInit ();
		bool isShow=OSBridgeManager.Instance.IsShowPetByName (_pet.name);
		if (isInit && isShow) {
			SetPetActive (true);
			return;
		} else if (isInit && !isShow) {
			return;
		}
		else if(!isInit && !isShow)
		{
			Invoke ("ShowPetHandler", 1f);
		}
	}

	private void RefreshRoomStatusHandler(bool a)
	{
		bool isShow=OSBridgeManager.Instance.IsShowPetByName (_pet.name);
		if (false == isShow) {
			SetPetActive (false);
		} else {
			SetPetActive (true);
		}
	}

    private void RotatePauseHandler(string name)
    {
        if (name == LookAtCamera.GetChild(0).name)
        {
            GameObject particle = Instantiate(Resources.Load<GameObject>("Particle/1"));
            particle.transform.SetParent(LookAtCamera);
            particle.transform.localPosition = new Vector3(0, 0, 1f);
            particle.transform.localRotation = Quaternion.identity;
            isCapturePaused = true;
            HandleScreenMoveAnimation(false);
        }
    }

    private void RotateGoOnHandler()
    {
        isCapturePaused = false;
		StartCoroutine(WorldRotate(true, RandomUtil.Bool(), Random.Range(30f, 90f),_rotateSpeed));
		HandleScreenMoveAnimation(true,_rotateSpeed);
    }

	private void HandleScreenMoveAnimation(bool isStart,float speed=1f)
    {
		screenAnimation [lastAnimationName].speed = speed;
        if (isStart)
        {
            screenAnimation[lastAnimationName].time = lastAnimationTime;
            screenAnimation.Play(lastAnimationName);
        }
        else
        {
            foreach (AnimationState state in screenAnimation)
            {
                if (screenAnimation.IsPlaying(state.name))
                {
                    lastAnimationName = state.name;
                    lastAnimationTime = screenAnimation[state.name].time;
                    screenAnimation.Stop(state.name);
                }
            }
        }
    }

	private IEnumerator WorldRotate(bool isContinuous, bool isClockWise, float rotateAngle,float rotateSpeed=1f)
    {
        if (isContinuous)
        {
            while (rotateAngle > 0)
            {
                if (isClockWise)
                {
					transform.RotateAround(rotatePoint, AxisDir, rotateAngle * Time.deltaTime*rotateSpeed);
                }
                else
                {
					transform.RotateAround(rotatePoint, -AxisDir, rotateAngle * Time.deltaTime*rotateSpeed);
                }
				rotateAngle -= rotateAngle * Time.deltaTime*rotateSpeed;
                rotateAngle = (rotateAngle < 0) ? 0 : rotateAngle;
                yield return null;
            }
        }
        else
        {
            if (isClockWise)
            {
                transform.RotateAround(rotatePoint, AxisDir, rotateAngle);
            }
            else
            {
                transform.RotateAround(rotatePoint, -AxisDir, rotateAngle);
            }
        }
    }

    private IEnumerator GameHelper(int petNum)
    {
        if (petNum < 0 || petNum > 30)
        {
            Debug.LogError("The petNum is illegal");
            yield break;
        }

        double[] duration = null;
        double[] outOfFOV = null;
        double[] moveSpeed = null;
        double[] moveDis = null;
        if (petNum >= 0 && petNum <= 25)
        {
            duration = new double[3] { configModule.dllConfig.captureDuration.Tx1, configModule.dllConfig.captureDuration.Tx2, configModule.dllConfig.captureDuration.Tx3 };
            outOfFOV = new double[2] { configModule.dllConfig.longestOutOfFOV.T1, configModule.dllConfig.longestOutOfFOV.T2 };
            moveSpeed = new double[2] { configModule.dllConfig.petMoveSpeed.V1, configModule.dllConfig.petMoveSpeed.V2 };
            moveDis = new double[3] { configModule.dllConfig.petMoveDistance.L1, configModule.dllConfig.petMoveDistance.L2, configModule.dllConfig.petMoveDistance.L3 };
        }
        else if (petNum >= 26 && petNum <= 30)
        {
            duration = new double[3] { configModule.dllConfig.captureDuration.Tx3, configModule.dllConfig.captureDuration.Tx4, configModule.dllConfig.captureDuration.Tx5 };
            outOfFOV = new double[2] { configModule.dllConfig.longestOutOfFOV.T2, configModule.dllConfig.longestOutOfFOV.T3 };
            moveSpeed = new double[2] { configModule.dllConfig.petMoveSpeed.V3, configModule.dllConfig.petMoveSpeed.V4 };
            moveDis = new double[3] { configModule.dllConfig.petMoveDistance.L3, configModule.dllConfig.petMoveDistance.L4, configModule.dllConfig.petMoveDistance.L5 };
        }
        float captureDuration = (float)RandomUtil.Array(duration);
        float longestOutOfFov = (float)RandomUtil.Array(outOfFOV);
        float petMoveSpeed = (float)RandomUtil.Array(moveSpeed);
        float petMoveDistance = (float)RandomUtil.Array(moveDis);
        float durationCounter = captureDuration;
        float outOfFOVCounter = longestOutOfFov;
        while (true)
        {

            while (durationCounter > 0)
            {
                yield return null;

                if (!isCapturePaused)
                {
                    durationCounter -= Time.deltaTime;

                    if (outOfFOVCounter > 0)
                    {
                        outOfFOVCounter -= Time.deltaTime;
                    }
                    else
                    {
                        outOfFOVCounter = longestOutOfFov;
                        //ToDo OutFOV
                        ForceIntoFOV();
                    }
                }
            }

            if (!isCapturePaused)
            {
                durationCounter = captureDuration;
                //ToBe CaptureDuration
                ForceCapture();
            }
        }
    }

    private void ForceCapture()
    {
        if (gyroController != null)
        {
            gyroController.DetachGyro();
        }
        mainCamera.transform.eulerAngles = new Vector3(0, mainCamera.transform.eulerAngles.y, 0);
        HandleScreenMoveAnimation(false);
        //Debug.Log(LookAtCamera.position);
        mainCamera.transform.DOLookAt(LookAtCamera.position, 1f);
        //mainCamera.transform.LookAt(LookAtCamera.position);
    }

    private void ForceIntoFOV()
    {

        Vector3 cameraDir = mainCamera.transform.forward;
        Vector3 cameraRightDir = mainCamera.transform.right;
        cameraDir = new Vector3(cameraDir.x, 0, cameraDir.z);
        cameraRightDir = new Vector3(cameraRightDir.x, 0, cameraRightDir.z);
        Vector3 petDir = transform.position.normalized;
        float currentRotateAngle = Vector3.Angle(cameraDir, petDir);
        float currentParalelAngle = Vector3.Angle(cameraRightDir, petDir);

        if (currentRotateAngle > 10)
        {
            bool clockWis = currentParalelAngle > 90 ? true : false;
            StartCoroutine(WorldRotate(true, clockWis, currentRotateAngle));
        }
    }
}

























/*
 private bool IsInCameraFOV()
 {
     Vector3 localPos = mainCamera.transform.InverseTransformPoint(transform.position);
     if (localPos.z <= 0)
     {
         return false;
     }
     else
     {
         float halfFOV = mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
         float aspet = mainCamera.aspect;
         float height = localPos.z * Mathf.Tan(halfFOV);
         float width = height * aspet;

         if (localPos.x < -width || localPos.x > width || localPos.y < -height || localPos.y > height)
         {
             return false;
         }
     }
     return true;
 }

 private Vector3[] GetRotateAxis(Transform trans)
 {
     Vector3 Axis1 = (trans.position - mainCamera.transform.position).normalized;
     if (Axis1.z != 0)
     {
         Vector3[] values = new Vector3[2];
         float TAN15 = Mathf.Tan(15 * Mathf.Deg2Rad);
         values[0].y = 1 / Mathf.Sqrt(1 + POW2(TAN15));
         values[1].y = values[0].y;
         if (Axis1.x != 0)
         {
             values[0].z = -Axis1.y * Axis1.z * Mathf.Sqrt(1 + POW2(TAN15)) +
             Mathf.Sqrt(POW2(Axis1.y * Axis1.z) * (1 + POW2(TAN15)) - (1 + POW2(TAN15)) * (POW2(Axis1.x) + POW2(Axis1.z)) * (POW2(Axis1.y) - POW2(Axis1.x * TAN15)));
             values[1].z = -Axis1.y * Axis1.z * Mathf.Sqrt(1 + POW2(TAN15)) -
             Mathf.Sqrt(POW2(Axis1.y * Axis1.z) * (1 + POW2(TAN15)) - (1 + POW2(TAN15)) * (POW2(Axis1.x) + POW2(Axis1.z)) * (POW2(Axis1.y) - POW2(Axis1.x * TAN15)));
             values[0].x = -(Axis1.z * values[0].z + Axis1.y / (Mathf.Sqrt(1 + POW2(TAN15)))) / Axis1.x;
             values[1].x = -(Axis1.z * values[1].z + Axis1.y / (Mathf.Sqrt(1 + POW2(TAN15)))) / Axis1.x;

         }
         else
         {
             values[0].z = -Axis1.y * values[0].y / Axis1.z;
             values[1].z = values[0].z;
             values[0].x = Mathf.Sqrt(1 - POW2(values[0].y) - POW2(values[0].z));
             values[1].x = -values[0].x;
         }
         return values;
     }
     else
     {
         if (Axis1.x != 0)
         {
             Vector3[] values = new Vector3[2];
             float TAN15 = Mathf.Tan(15 * Mathf.Deg2Rad);
             values[0].y = 1 / Mathf.Sqrt(1 + POW2(TAN15));
             values[0].x = -(values[0].y * Axis1.y) / Axis1.x;
             values[0].z = Mathf.Sqrt(POW2(Axis1.x * TAN15) - POW2(Axis1.y) / (POW2(Axis1.x) * (1 + POW2(TAN15))));
             values[1].y = values[0].y;
             values[1].x = values[0].x;
             values[1].z = -values[0].z;
             return values;
         }
         return null;
     }
 }

 private float POW2(float v)
 {
     return Mathf.Pow(v, 2);
 }
 */
