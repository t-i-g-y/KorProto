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
    [SerializeField] private float timeMultiplier = 1f;
    private float previousTimeMultiplier = 1f;
    public float TimeMultiplier => timeMultiplier;
    public float CustomDeltaTime => Time.deltaTime * timeMultiplier;

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
    }

    public void Pause() => timeMultiplier = 0f;
    public void Unpause() => timeMultiplier = 1f;
    public void SetSpeed(float speed)
    {
        previousTimeMultiplier = timeMultiplier;
        timeMultiplier = Mathf.Max(0f, speed);
    }

    private void Update()
    {
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

        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timeMultiplier == 0f) 
                Unpause();
            else 
                Pause();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
            SetSpeed(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) 
            SetSpeed(2f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) 
            SetSpeed(5f);
        */
    }
}

