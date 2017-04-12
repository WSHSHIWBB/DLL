using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeParticle : MonoBehaviour
{
    public float Delay = 2f;

    private ParticleSystem _particle;
    private float _deltaTime;
    private float _lastFrameRealTime;

    private void Awake()
    {
        _particle = GetComponentInChildren<ParticleSystem>();  
    }

    private void Start()
    {
        _lastFrameRealTime = Time.realtimeSinceStartup;
        DestroyObject(gameObject, Delay);
    }

    void Update ()
    {
        if (_particle == null) return;
        _deltaTime =  Time.realtimeSinceStartup- _lastFrameRealTime;
        _lastFrameRealTime = Time.realtimeSinceStartup;
        if(Time.timeScale!=1)
        {
            _particle.Simulate(_deltaTime, true, false);
        }
	}
}
