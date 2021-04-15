using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Entities;
using UniRx.Triggers;
using UniRx;
using System;
using UnityEngine.UI;



public class Test : MonoBehaviour
{
    public ReactiveProperty<int> CurrentTime;
    public Button btn;

    private void Start()
    {   
        CurrentTime = new IntReactiveProperty(30);        
        
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ => { if (CurrentTime.Value > 0) { CurrentTime.Value--; } }).AddTo(this);
        CurrentTime.Subscribe(x => Debug.Log($"CurrentTime value {x}"));

        btn.OnClickAsObservable().Subscribe(b => Debug.Log($"OnClick {b}"));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
