using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SubsystemSaveLoadTests
{
    [Test]
    public void SubsystemSaveDataIntegrityTest()
    {
        SubsystemSaveData saveData = new SubsystemSaveData
        {
            anchorData = new RailAnchorRegistrySaveData(),
            railData = new RailManagerSaveData(),
            relayStopData = new RelayStopRegistrySaveData(),
            globalDemandData = new GlobalDemandSystemSaveData(),
            financeData = new FinanceSystemSaveData(),
            timeData = new TimeManagerSaveData(),
            trainData = new TrainManagerSaveData(),
            economyData = new EconomyManagerSaveData(),
            researchData = new ResearchSystemSaveData()
        };
        
        Assert.IsNotNull(saveData.anchorData);
        Assert.IsNotNull(saveData.railData);
        Assert.IsNotNull(saveData.relayStopData);
        Assert.IsNotNull(saveData.globalDemandData);
        Assert.IsNotNull(saveData.financeData);
        Assert.IsNotNull(saveData.timeData);
        Assert.IsNotNull(saveData.trainData);
        Assert.IsNotNull(saveData.economyData);
        Assert.IsNotNull(saveData.researchData);
    }

    [Test]
    public void RailLineSaveDataTest()
    {
        RailLineSaveData saveData = new RailLineSaveData
        {
            ID = 5,
            cells = new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0)
            }
        };

        Assert.AreEqual(5, saveData.ID);
        Assert.AreEqual(2, saveData.cells.Count);
    }

    [Test]
    public void TrainSaveDataTest()
    {
        TrainSaveData data = new TrainSaveData
        {
            ID = 10,
            assignedLineID = 3,
            speedLevel = 2,
            dir = -1,
            currentTileIndex = 4,
            headDistance = 12.5f,
            atStation = true,
            isOperational = false,
            isBroken = true
        };

        Assert.AreEqual(10, data.ID);
        Assert.AreEqual(3, data.assignedLineID);
        Assert.AreEqual(2, data.speedLevel);
        Assert.AreEqual(-1, data.dir);
        Assert.AreEqual(4, data.currentTileIndex);
        Assert.AreEqual(12.5f, data.headDistance);
        Assert.IsTrue(data.atStation);
        Assert.IsFalse(data.isOperational);
        Assert.IsTrue(data.isBroken);
    }

    [Test]
    public void ResourceDestinationQueueSaveDataTest()
    {
        ResourceDestinationQueueSaveData saveData = new ResourceDestinationQueueSaveData
        {
            resourceType = 1,
            destinationStationIDs = new List<int> { 10, 20, 30 }
        };

        Assert.AreEqual(1, saveData.resourceType);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, saveData.destinationStationIDs);
    }

    [Test]
    public void ResearchSystemSaveDataTest()
    {
        ResearchSystemSaveData saveData = new ResearchSystemSaveData
        {
            currentResearchID = 2,
            technologies = new List<TechnologySaveData>
            {
                new TechnologySaveData
                {
                    techID = 2,
                    progress = 40f,
                    isUnlocked = false,
                    isResearching = true
                }
            }
        };

        Assert.AreEqual(2, saveData.currentResearchID);
        Assert.AreEqual(1, saveData.technologies.Count);
        Assert.AreEqual(40f, saveData.technologies[0].progress);
        Assert.IsTrue(saveData.technologies[0].isResearching);
    }

    [Test]
    public void TrainConsistSaveDataTest()
    {
        TrainConsistSaveData saveData = new TrainConsistSaveData
        {
            headLocomotive = new TrainConsistUnitSaveData
            {
                capacity = 6,
                maintenance = 1f
            },
            wagons = new List<TrainConsistUnitSaveData>
            {
                new TrainConsistUnitSaveData { capacity = 6, maintenance = 1f }
            },
            cargo = new List<ResourceAmountSaveData>
            {
                new ResourceAmountSaveData { resourceType = 0, amount = 2 }
            },
            cargoDestinations = new List<ResourceDestinationQueueSaveData>
            {
                new ResourceDestinationQueueSaveData
                {
                    resourceType = 0,
                    destinationStationIDs = new List<int> { 100, 101 }
                }
            }
        };

        string json = JsonUtility.ToJson(saveData, true);
        TrainConsistSaveData restored = JsonUtility.FromJson<TrainConsistSaveData>(json);

        Assert.AreEqual(6, restored.headLocomotive.capacity);
        Assert.AreEqual(1, restored.wagons.Count);
        Assert.AreEqual(2, restored.cargo[0].amount);
        CollectionAssert.AreEqual(new[] { 100, 101 }, restored.cargoDestinations[0].destinationStationIDs);
    }

    [Test]
    public void GlobalDemandSaveDataTest()
    {
        GlobalDemandSystemSaveData saveData = new GlobalDemandSystemSaveData
        {
            stationTransit = new List<StationTransitSaveData>
            {
                new StationTransitSaveData
                {
                    stationID = 4,
                    resourceType = 2,
                    amount = 3,
                    destinationStationIDs = new List<int> { 9, 10, 11 }
                }
            }
        };

        string json = JsonUtility.ToJson(saveData, true);
        GlobalDemandSystemSaveData restored =
            JsonUtility.FromJson<GlobalDemandSystemSaveData>(json);

        Assert.AreEqual(1, restored.stationTransit.Count);
        Assert.AreEqual(4, restored.stationTransit[0].stationID);
        Assert.AreEqual(2, restored.stationTransit[0].resourceType);
        Assert.AreEqual(3, restored.stationTransit[0].amount);
        CollectionAssert.AreEqual(new[] { 9, 10, 11 }, restored.stationTransit[0].destinationStationIDs);
    }

    [Test]
    public void RelayStopSaveDataTest()
    {
        RelayStopSaveData saveData = new RelayStopSaveData
        {
            ID = 12,
            cell = new Vector3Int(5, 6, 0),
            maintenancePerDay = 2.5f,
            storedCargo = new List<ResourceAmountSaveData>
            {
                new ResourceAmountSaveData { resourceType = 1, amount = 4 }
            },
            cargoDestinations = new List<ResourceDestinationQueueSaveData>
            {
                new ResourceDestinationQueueSaveData
                {
                    resourceType = 1,
                    destinationStationIDs = new List<int> { 20, 21 }
                }
            }
        };

        string json = JsonUtility.ToJson(saveData, true);
        RelayStopSaveData restored = JsonUtility.FromJson<RelayStopSaveData>(json);

        Assert.AreEqual(12, restored.ID);
        Assert.AreEqual(new Vector3Int(5, 6, 0), restored.cell);
        Assert.AreEqual(2.5f, restored.maintenancePerDay);
        Assert.AreEqual(4, restored.storedCargo[0].amount);
        CollectionAssert.AreEqual(new[] { 20, 21 }, restored.cargoDestinations[0].destinationStationIDs);
    }

    [Test]
    public void SubsystemBaseSaveLoadCycleTest()
    {
        SubsystemSaveData saveData = new SubsystemSaveData
        {
            railData = new RailManagerSaveData
            {
                nextID = 7,
                selectedLineID = 3,
                lines = new List<RailLineSaveData>
                {
                    new RailLineSaveData
                    {
                        ID = 3,
                        cells = new List<Vector3Int>
                        {
                            new Vector3Int(0, 0, 0),
                            new Vector3Int(1, 0, 0)
                        }
                    }
                }
            },

            trainData = new TrainManagerSaveData
            {
                nextID = 11,
                selectedTrainID = 5,
                trains = new List<TrainSaveData>
                {
                    new TrainSaveData
                    {
                        ID = 5,
                        assignedLineID = 3,
                        speedLevel = 2,
                        dir = 1,
                        currentTileIndex = 1,
                        headDistance = 1.25f,
                        atStation = false,
                        isOperational = true,
                        isBroken = false
                    }
                }
            },

            financeData = new FinanceSystemSaveData
            {
                balance = 250f,
                lastBalanceChange = 25f,
                dayBalance = 40f,
                currentDay = 6
            },

            timeData = new TimeManagerSaveData
            {
                timeMultiplier = 2f,
                previousTimeMultiplier = 1f,
                secondsPerHour = 1f,
                accumulatedSeconds = 0.5f,
                dayCounter = 4,
                hourCounter = 9
            }
        };

        string json = JsonUtility.ToJson(saveData, true);
        SubsystemSaveData restored = JsonUtility.FromJson<SubsystemSaveData>(json);

        Assert.IsNotNull(restored);

        Assert.AreEqual(7, restored.railData.nextID);
        Assert.AreEqual(3, restored.railData.selectedLineID);
        Assert.AreEqual(1, restored.railData.lines.Count);
        Assert.AreEqual(new Vector3Int(1, 0, 0), restored.railData.lines[0].cells[1]);

        Assert.AreEqual(11, restored.trainData.nextID);
        Assert.AreEqual(5, restored.trainData.selectedTrainID);
        Assert.AreEqual(1, restored.trainData.trains.Count);
        Assert.AreEqual(3, restored.trainData.trains[0].assignedLineID);

        Assert.AreEqual(250f, restored.financeData.balance);
        Assert.AreEqual(6, restored.financeData.currentDay);

        Assert.AreEqual(4, restored.timeData.dayCounter);
        Assert.AreEqual(9, restored.timeData.hourCounter);
    }
}
