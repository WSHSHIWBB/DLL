using UnityEngine;
using System.Collections;
using VRFrameWork;
using System;

public class CameraQuard: MonoBehaviour
{
    private WebCamTexture webCam;
    private Material material;

    private void Awake()
    {
        
    }

    void Start()
    {
        material = GetComponent<Renderer>().material;
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
                    webCam = new WebCamTexture(WebCamTexture.devices[i].name,Screen.width,Screen.height);
                    material.mainTexture = webCam;
                    webCam.Play();
                    float scalY = transform.localScale.x *(float)webCam.height / (float)webCam.width;
                    transform.localScale = new Vector3(transform.localScale.x, scalY, 1);

#if UNITY_IPHONE && !UNITY_EDITOR
                    material.mainTextureOffset = new Vector2(0, 1);
                    material.mainTextureScale = new Vector2(1, -1);
#endif

                }
            }
        }
        else
        {
            //to do can't get the authorization of webcam
        }
    }
}


/*
private void OnGUI()
{
    GUILayout.Label("mirroied?" + webCam.videoVerticallyMirrored);
    GUILayout.Label("Angle" + webCam.videoRotationAngle);
    GUILayout.Label("oRIATION" + Screen.orientation);
    if (GUILayout.Button("liling(0,1,1,-1)", GUILayout.Height(200)))
    {
        material.mainTextureOffset = new Vector2(0,1);
        material.mainTextureScale = new Vector2(1, -1);
    }
    if (GUILayout.Button("liling(1,0,-1,1)", GUILayout.Height(200)))
    {
        material.mainTextureOffset = new Vector2(1, 0);
        material.mainTextureScale = new Vector2(-1, 1);
    }
    if (GUILayout.Button("liling(1,0,1,-1)", GUILayout.Height(200)))
    {
        material.mainTextureOffset = new Vector2(1, 0);
        material.mainTextureScale = new Vector2(1,-1);
    }
    if (GUILayout.Button("liling(0,1,-1,1)", GUILayout.Height(200)))
    {
        material.mainTextureOffset = new Vector2(0, 1);
        material.mainTextureScale = new Vector2(-1, 1);
    }
}
*/

