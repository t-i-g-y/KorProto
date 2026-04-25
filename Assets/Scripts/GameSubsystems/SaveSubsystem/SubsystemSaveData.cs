using System;

[Serializable]
public class SubsystemSaveData
{
    public RailAnchorRegistrySaveData anchorData;
    public RailManagerSaveData railData;
    public RelayStopRegistrySaveData relayStopData;
    public GlobalDemandSystemSaveData globalDemandData;
    public FinanceSystemSaveData financeData;
    public TimeManagerSaveData timeData;
    public TrainManagerSaveData trainData;
    public EconomyManagerSaveData economyData;
    public ResearchSystemSaveData researchData;
}
