using UnityEngine;
using TMPro;

public class FinanceBalanceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text dayBalanceText;
    [SerializeField] private GameObject warningRoot;
    [SerializeField] private TMP_Text warningText;

    private void Update()
    {
        balanceText.text = $"Баланс: {FinanceSystem.Instance.Balance}";
        dayBalanceText.text = GetBalanceChangeString(FinanceSystem.Instance.DayBalance);

        RefreshWarning(FinanceSystem.Instance.DaysLeftBeforeGameOver);
    }

    private string GetBalanceChangeString(float change) => change > 0 ? $"+{change:F2}" : $"{change:F2}"; 

    private void RefreshWarning(int daysLeft)
    {
        if (FinanceSystem.Instance == null)
        {
            if (warningRoot != null)
                warningRoot.SetActive(false);

            return;
        }

        bool show = FinanceSystem.Instance.IsInGameOverWarning;

        if (warningRoot != null)
            warningRoot.SetActive(show);

        if (!show || warningText == null)
            return;
        string dayText = daysLeft < 5 ? (daysLeft == 1 ? "день" : (daysLeft > 1 ? "дня" : "дней")) : "дней";
        warningText.text = $"Исправьте баланс!\nУ вас {daysLeft} {dayText}";
    }
}
