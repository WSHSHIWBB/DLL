using UnityEngine;
using VRFrameWork;


public class Radar : MonoBehaviour
{
    public float RadarRotateSpeed;
    private RectTransform radarImage;
    private Transform mainCamera;

    private RectTransform Pet1RotateImage;
    private RectTransform Pet2RotateImage;
    private RectTransform Pet3RotateImage;
    
    private Transform Pet1;
    private Transform Pet2;
    private Transform Pet3;
	
	void Start ()
    {
        radarImage = transform.GetComponentByPath<RectTransform>("RadarImage");
        mainCamera = Camera.main.transform;
        Pet1RotateImage = transform.GetComponentByPath<RectTransform>("PetPoints/Pet1RotateImage");
        Pet2RotateImage = transform.GetComponentByPath<RectTransform>("PetPoints/Pet2RotateImage");
        Pet3RotateImage = transform.GetComponentByPath<RectTransform>("PetPoints/Pet3RotateImage");
        Pet1 = GameObject.Find("Pet1").transform;
        Pet2 = GameObject.Find("Pet2").transform;
        Pet3 = GameObject.Find("Pet3").transform;
    }
	
	void Update ()
    {
        RaderAutoRotate();
        ShowPetPos(Pet1RotateImage, Pet1);
        ShowPetPos(Pet2RotateImage, Pet2);
        ShowPetPos(Pet3RotateImage, Pet3);
    }

    private void RaderAutoRotate()
    {
        var z = radarImage.localEulerAngles.z - RadarRotateSpeed * Time.deltaTime;
        radarImage.localEulerAngles = new Vector3(0, 0, z);
    }

    private void ShowPetPos(RectTransform Image,Transform trans)
    {
        RectTransform PetPoint = Image.GetComponentByPath<RectTransform>("PetPoint");
        if (trans.childCount == 0||!transform.GetChild(0).gameObject.activeInHierarchy)
        {
            Image.gameObject.SetActive(false);
        }
		else if(transform.GetChild(0).gameObject.activeInHierarchy)
        {
            Image.gameObject.SetActive(true);
        }
        Vector3 CameraDir = mainCamera.forward.normalized;
        Vector3 CameraRightDir = mainCamera.right.normalized;
        Vector3 cameraHorizontalDir = new Vector3(CameraDir.x, 0, CameraDir.z);
        Vector3 cameraVerticalHorizontalDir = new Vector3(CameraRightDir.x, 0, CameraRightDir.z);
        Vector3 PetDir = new Vector3(trans.position.x, 0, trans.position.z);
        float petAngle = Vector3.Angle(cameraHorizontalDir, PetDir);
        if (Vector3.Dot(cameraVerticalHorizontalDir, PetDir) < 0)
        {
            petAngle = 360 - petAngle;
        }
        Image.eulerAngles = new Vector3(0, 0, -petAngle);
        PetPoint.localEulerAngles = new Vector3(0, 0, -Image.localEulerAngles.z);
    }

}
