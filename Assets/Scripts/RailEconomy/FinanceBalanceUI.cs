using UnityEngine;
using TMPro;

public class FinanceBalanceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text dayBalanceText;

    private void Update()
    {
        balanceText.text = $"Balance: {FinanceManager.Instance.Balance}";
        dayBalanceText.text = GetBalanceChangeString(FinanceManager.Instance.DayBalance);
    }

    private string GetBalanceChangeString(float change) => change > 0 ? $"+{change}" : $"{change}"; 
}
