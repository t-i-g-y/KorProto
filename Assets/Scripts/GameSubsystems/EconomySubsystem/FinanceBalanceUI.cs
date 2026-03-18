using UnityEngine;
using TMPro;

public class FinanceBalanceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text dayBalanceText;

    private void Update()
    {
        balanceText.text = $"Баланс: {FinanceSystem.Instance.Balance}";
        dayBalanceText.text = GetBalanceChangeString(FinanceSystem.Instance.DayBalance);
    }

    private string GetBalanceChangeString(float change) => change > 0 ? $"+{change}" : $"{change}"; 
}
