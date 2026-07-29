using System;
using UnityEngine;

[Serializable]
public struct CooldownTimer
{
    public CooldownTimer(float interval)
    {
        Time = 0;
        Interval = interval;
    }

    [SerializeField] private float Time;

    [SerializeField] public float Interval;

    public bool Tick(float delta)
    {
        if (Time <= 0)
        {
            return true;
        }
        else
        {
            Time -= delta;
            return false;
        }
    }

    public bool IsTicking()
    {
        return Time <= 0;
    }

    public void Reset()
    {
        Time = Interval;
    }

    public void Reset(bool readyToTick)
    {
        Time = readyToTick ? 0 : Interval;
    }
}