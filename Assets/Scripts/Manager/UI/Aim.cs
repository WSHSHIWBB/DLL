using UnityEngine;

public class Aim: MonoBehaviour
{
    public float RotateSpeed = 50;
    public bool isClockkWise = false;
    private RectTransform rotateRect;

    void Start ()
    {
        rotateRect = GetComponent<RectTransform>();
    }

    void Update ()
    {
        var z = rotateRect.localEulerAngles.z +(isClockkWise?1:-1)* RotateSpeed * Time.deltaTime;
        rotateRect.localEulerAngles = new Vector3(0, 0, z);
    }
}
