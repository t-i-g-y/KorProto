using UnityEngine;
using UnityEngine.UI;

// to deprecate
public class TechManager : MonoBehaviour
{
    [SerializeField] private RailBuilderController railBuilder;
    [SerializeField] private Toggle lakeCrossingToggle;
    [SerializeField] private Toggle mountainTunnelToggle;
    [SerializeField] private Toggle seaTunnelToggle;

    private float lakeCrossingCost = 1000f;
    private float mountainTunnelCost = 10000f;
    private float seaTunnelCost = 20000f;

    void Start()
    {
        lakeCrossingToggle.onValueChanged.AddListener((value) => HandleUpgrade(0, value, lakeCrossingCost));
        mountainTunnelToggle.onValueChanged.AddListener((value) => HandleUpgrade(1, value, mountainTunnelCost));
        seaTunnelToggle.onValueChanged.AddListener((value) => HandleUpgrade(2, value, seaTunnelCost));
    }

    private void HandleUpgrade(int upgrade, bool value, float cost)
    {
        if (value)
        {
            if (FinanceManager.Instance.Balance >= cost)
            {
                FinanceManager.Instance.AdjustBalance(-cost);
                switch (upgrade)
                {
                    case 0:
                        railBuilder.AllowLakeCrossing(value);
                        break;
                    case 1:
                        railBuilder.AllowMountainTunnel(value);
                        break;
                    case 2:
                        railBuilder.AllowSeaTunnel(value);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                value = false;
                switch (upgrade)
                {
                    case 0:
                        lakeCrossingToggle.isOn = value;
                        break;
                    case 1:
                        mountainTunnelToggle.isOn = value;
                        break;
                    case 2:
                        seaTunnelToggle.isOn = value;
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            switch (upgrade)
            {
                case 0:
                    railBuilder.AllowLakeCrossing(value);
                    break;
                case 1:
                    railBuilder.AllowMountainTunnel(value);
                    break;
                case 2:
                    railBuilder.AllowSeaTunnel(value);
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateUI()
    {
        lakeCrossingToggle.interactable = FinanceManager.Instance.Balance >= lakeCrossingCost;
        mountainTunnelToggle.interactable = FinanceManager.Instance.Balance >= mountainTunnelCost;
        seaTunnelToggle.interactable = FinanceManager.Instance.Balance >= seaTunnelCost;
    }
}
