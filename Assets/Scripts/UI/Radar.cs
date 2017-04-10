using UnityEngine;


public class Radar : MonoBehaviour
{
    public float RadarRotateSpeed;
    private RectTransform radarImage;
    private RectTransform rotateImage;
    private RectTransform PetImage;
    private Transform mainCamera;
    private Transform LookAtCamera;
	
	void Start ()
    {
        radarImage = transform.GetChild(0).GetComponent<RectTransform>();
        rotateImage = transform.GetChild(1).GetComponent<RectTransform>();
        PetImage = rotateImage.GetChild(0).GetComponent<RectTransform>();
        mainCamera = Camera.main.transform;
        LookAtCamera = GameObject.Find("LookAtCamera").transform;
	}
	
	void Update ()
    {
        if (LookAtCamera.childCount==0)
        {
            PetImage.gameObject.SetActive(false);
        }
        else
        {
            PetImage.gameObject.SetActive(true);
        }
        var z = radarImage.localEulerAngles.z -RadarRotateSpeed * Time.deltaTime;
        radarImage.localEulerAngles = new Vector3(0,0,z);
        Vector3 CameraDir = mainCamera.forward.normalized;
        Vector3 CameraRightDir = mainCamera.right.normalized;
        Vector3 cameraHorizontalDir= new Vector3(CameraDir.x, 0, CameraDir.z);
        Vector3 cameraVerticalHorizontalDir = new Vector3(CameraRightDir.x, 0, CameraRightDir.z);
        Vector3 cubeParentDir = new Vector3(LookAtCamera.position.x, 0, LookAtCamera.position.z);
        float petAngle = Vector3.Angle(cameraHorizontalDir, cubeParentDir);
        if(Vector3.Dot(cameraVerticalHorizontalDir,cubeParentDir)<0)
        {
            petAngle = 360 - petAngle;
        }
        rotateImage.eulerAngles = new Vector3(0, 0, -petAngle);
        PetImage.localEulerAngles = new Vector3(0, 0, -rotateImage.localEulerAngles.z);
	}

}
