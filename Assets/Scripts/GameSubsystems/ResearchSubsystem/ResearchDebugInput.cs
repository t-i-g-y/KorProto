using UnityEngine;
using UnityEngine.InputSystem;

public class ResearchDebugInput : MonoBehaviour
{
    [SerializeField] private int pointsPerClick = 100;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.rKey.wasPressedThisFrame)
            {
                ResearchIncomeSystem.Instance.AddGlobalResearchPerHour(2);
            }
        }

    }
}