using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenScale : MonoBehaviour
{
    public float LargeScale = 2;
    public float Duration = 1f;
	
	void Start ()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScale(LargeScale, Duration));
        mySequence.SetLoops(-1, LoopType.Yoyo);
    }
	
	
	
}
