using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public float Delay=2f;
	void Start ()
    {
        DestroyObject(this.gameObject, Delay);
	}
}
