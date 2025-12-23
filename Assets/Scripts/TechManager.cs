using UnityEngine;
using UnityEngine.UI;

public class TechManager : MonoBehaviour
{
    [SerializeField] private RailBuilderController railBuilder;
    [SerializeField] private Toggle lakeCrossingToggle;
    [SerializeField] private Toggle mountainTunnelToggle;
    [SerializeField] private Toggle seaTunnelToggle;

    void Start()
    {
        lakeCrossingToggle.onValueChanged.AddListener((value) => railBuilder.AllowLakeCrossing(value));
        mountainTunnelToggle.onValueChanged.AddListener((value) => railBuilder.AllowMountainTunnel(value));
        seaTunnelToggle.onValueChanged.AddListener((value) => railBuilder.AllowSeaTunnel(value));
    }
}
