using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    /*
    ** timeMultiplier - модификатор скорости течение времени игровых процессов
    ** 0 = пауза
    ** 1 = базовая скорость
    ** 2 = ускоренная 2х скорость
    */
    [SerializeField] private TMP_Text dayHourText;
    [SerializeField] private float timeMultiplier = 1f;
    private float previousTimeMultiplier = 1f;

    [SerializeField] private float secondsPerHour = 1f;
    private float accumulatedSeconds = 0f;
    private int dayCounter = 0;
    private int hourCounter = 0;
    public float TimeMultiplier => timeMultiplier;
    public float CustomDeltaTime => Time.deltaTime * timeMultiplier;
    public bool IsPaused => timeMultiplier == 0f;
    public event Action<int, int> OnHourChanged;
    public event Action<int> OnDayChanged;
    public int DayCounter
    {
        get => dayCounter;
        set
        {
            if (value != dayCounter)
            {
                dayCounter = value > 0 ? value : 0;
            }
            
        }
    }
    public int HourCounter
    {
        get => hourCounter;
        set
        {
            int hourValue = value;
            if (hourValue > 23)
            {
                DayCounter += hourValue / 24;
                HourCounter = 0;
                HourCounter += hourValue % 24;
            }
            else if (hourValue < 0)
            {
                hourCounter = 0;
            }
            else
            {
                hourCounter = value;
            }
        }
    }

    public string DayHourString => hourCounter > 9 ? $"День {dayCounter} {hourCounter}:00" : $"День {dayCounter} 0{hourCounter}:00";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        RefreshUI();
    }

    public void Pause() => timeMultiplier = 0f;
    public void Unpause() => timeMultiplier = previousTimeMultiplier > 0f ? previousTimeMultiplier : 1f;
    public void SetSpeed(float speed)
    {
        previousTimeMultiplier = timeMultiplier;
        timeMultiplier = Mathf.Max(0f, speed);
    }

    private void Update()
    {
        if (MenuPauseState.IsPaused)
            return;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (timeMultiplier == 0f) 
                    Unpause(); 
                else 
                    Pause();
            }
            if (kb.digit1Key.wasPressedThisFrame) 
                SetSpeed(1f);
            if (kb.digit2Key.wasPressedThisFrame) 
                SetSpeed(2f);
            if (kb.digit3Key.wasPressedThisFrame) 
                SetSpeed(5f);
        }

        float delta = Time.deltaTime * timeMultiplier;
        if (delta <= 0f)
            return;
        
        accumulatedSeconds += delta;

        if (accumulatedSeconds >= secondsPerHour)
        {
            int hoursPassed = Mathf.FloorToInt(accumulatedSeconds / secondsPerHour);
            accumulatedSeconds -= hoursPassed * secondsPerHour;

            for (int i = 0; i < hoursPassed; i++)
                AdvanceOneHour();
        }
    }

    private void AdvanceOneHour()
    {
        int oldDay = dayCounter;

        HourCounter += 1;
        RefreshUI();

        OnHourChanged?.Invoke(dayCounter, hourCounter);

        if (dayCounter != oldDay)
            OnDayChanged?.Invoke(dayCounter);
    }

    public void AdvanceHoursForTests(int hours = 1)
    {
        for (int i = 0; i < hours; i++)
            AdvanceOneHour();
    }
    private void RefreshUI()
    {
        if (dayHourText != null)
            dayHourText.text = DayHourString;
    }

    #region save subsystem
    public TimeManagerSaveData GetSaveData()
    {
        return new TimeManagerSaveData
        {
            timeMultiplier = timeMultiplier,
            previousTimeMultiplier = previousTimeMultiplier,
            secondsPerHour = secondsPerHour,
            accumulatedSeconds = accumulatedSeconds,
            dayCounter = dayCounter,
            hourCounter = hourCounter
        };
    }

    public void LoadFromSaveData(TimeManagerSaveData data)
    {
        if (data == null)
            return;

        timeMultiplier = Mathf.Max(0f, data.timeMultiplier);
        previousTimeMultiplier = Mathf.Max(0f, data.previousTimeMultiplier);
        secondsPerHour = Mathf.Max(0.0001f, data.secondsPerHour);
        accumulatedSeconds = Mathf.Max(0f, data.accumulatedSeconds);

        DayCounter = data.dayCounter;
        HourCounter = data.hourCounter;
        Pause();
        RefreshUI();
    }

    #endregion
}

