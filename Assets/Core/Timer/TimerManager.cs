using System;
using UnityEngine;

public class TimerManager : Timer
{
    public static TimerManager Singleton { get; private set; }

    void Awake()
    {
        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `TimerManager`s in this scene.");
            throw new Exception();
        }
        Singleton = this;
    }

    void Update()
    {
        Tick(Time.deltaTime);
    }
}
