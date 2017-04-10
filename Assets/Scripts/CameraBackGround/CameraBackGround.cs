using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraBackGround : MonoBehaviour
{
    private RawImage rawImage;
    private WebCamTexture webCamTexture;
    private AspectRatioFitter asRF;

    void Start()
    {
        asRF = GetComponent<AspectRatioFitter>();
        rawImage = GetComponent<RawImage>();
        StartCoroutine(StartBackWebCamera());
    }

    private IEnumerator StartBackWebCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            for (int i = 0; i < WebCamTexture.devices.Length; ++i)
            {
                if (!WebCamTexture.devices[i].isFrontFacing)
                {
                    webCamTexture = new WebCamTexture(WebCamTexture.devices[i].name, Screen.width, Screen.height, 60);
                    rawImage.texture = webCamTexture;
                    webCamTexture.Play();
                }
            }
        }
        else
        {
            //to do can't get the authorization of webcam
        }
    }

    
    private void Update()
    {
        if(!webCamTexture.isPlaying)
        {
            return;
        }

        float rotateAngle = -webCamTexture.videoRotationAngle;
        if(webCamTexture.videoVerticallyMirrored)
        {
            rotateAngle += 180;
        }
        
        rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotateAngle);
        float rato = (float)webCamTexture.width / (float)webCamTexture.height;
        asRF.aspectRatio = rato;
    }
    

    /*
    private void OnGUI()
    {
       
    }
    */
}
