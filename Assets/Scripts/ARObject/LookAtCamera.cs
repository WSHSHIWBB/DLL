using UnityEngine;
using System.Collections;
using VRFrameWork;

public class LookAtCamera : MonoBehaviour
{
    private Vector3 mainCameraPos;

    void Update ()
    {
        mainCameraPos = Camera.main.transform.position;
        transform.LookAt(mainCameraPos);
	}

    private void ResetPetHandler()
    {
        if(transform.childCount!=0)
        {
            for(int i=0;i<transform.childCount;++i)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    private void OnEnable()
    {
        EventDispatcher.AddEvent("ResetPet", ResetPetHandler);
    }

    private void OnDisable()
    {
        EventDispatcher.RemoveEvent("ResetPet", ResetPetHandler);
    }


}
