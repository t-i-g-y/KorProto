using System;

[Serializable]
public class TimeManagerSaveData
{
    public float timeMultiplier;
    public float previousTimeMultiplier;
    public float secondsPerHour;
    public float accumulatedSeconds;
    public int dayCounter;
    public int hourCounter;
}
