using System.Collections.Generic;
using UnityEngine;

public class RelayStopRegistry : MonoBehaviour
{
    public static RelayStopRegistry Instance { get; private set; }

    [SerializeField] private RelayStop relayPrefab;
    [SerializeField] private float defaultMaintenancePerDay = 1f;

    private readonly Dictionary<Vector3Int, RelayStop> relaysByCell = new();
    private int nextId = 0;

    public IEnumerable<RelayStop> AllRelays => relaysByCell.Values;

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
        RebuildRegistry();
    }

    public void RebuildRegistry()
    {
        relaysByCell.Clear();
        RelayStop[] relays = FindObjectsByType<RelayStop>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

        foreach (var relay in relays)
        {
            if (relay == null)
                continue;

            relaysByCell[relay.Cell] = relay;
            nextId = Mathf.Max(nextId, relay.ID + 1);
        }
    }

    public bool TryGet(Vector3Int cell, out RelayStop relay)
    {
        return relaysByCell.TryGetValue(cell, out relay);
    }

    public RelayStop GetOrCreate(Vector3Int cell, Vector3 worldCoord)
    {
        if (relaysByCell.TryGetValue(cell, out var existing))
            return existing;

        if (relayPrefab == null)
        {
            Debug.LogError("FreightRelayRegistry: relayPrefab  missin");
            return null;
        }

        RelayStop relay = Instantiate(relayPrefab, worldCoord, Quaternion.identity);
        relay.Initialize(nextId++, cell, defaultMaintenancePerDay);
        relaysByCell[cell] = relay;
        return relay;
    }

    public bool IsRelayCell(Vector3Int cell)
    {
        return relaysByCell.ContainsKey(cell);
    }

    public bool RemoveIfExists(Vector3Int cell)
    {
        if (!relaysByCell.TryGetValue(cell, out RelayStop relay) || relay == null)
            return false;

        relaysByCell.Remove(cell);
        Destroy(relay.gameObject);
        return true;
    }
}