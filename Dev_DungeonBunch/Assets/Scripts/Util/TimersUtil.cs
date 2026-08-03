using System;
using UnityEngine;

[Serializable]
public struct CountdownTimer
{
    public CountdownTimer(float interval)
    {
        this = default;
        Time = 0;
        Interval = interval;
    }

    public event EventHandler OnTick;

    [SerializeField] private float Time;
    [SerializeField] public float Interval;

    [HideInInspector] public readonly float Remaining => Time;
    [HideInInspector] public readonly float RemainingPercent => Time / Interval;
    [HideInInspector] public readonly float Progress => Interval - Time;
    [HideInInspector] public readonly float ProgressPercent => (Interval - Time) / Interval;

    /// <summary>
    /// Advances the countdown timer by the specified time delta and invokes the tick event when the timer completes.
    /// </summary>
    /// <param name="delta">The amount of time to subtract from the timer.</param>
    /// <returns>
    /// <c>true</c> when the timer reaches zero and has completed its tick; otherwise, <c>false</c>.
    /// </returns>
    public bool Tick(float delta)
    {
        if (Time <= 0)
            return true;

        Time -= delta;

        if (Time <= 0)
        {
            Time = 0;
            OnTick?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    public readonly bool IsTicking()
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