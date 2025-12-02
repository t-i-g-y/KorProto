using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Train : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float speedUnitsPerSec = 4f;
    [SerializeField] private float arriveSnap = 0.02f;

    [Header("Capacity")]
    [SerializeField] private int capacity = 6;
    public bool onlyLoadRequested = false;

    [Header("Timing")]
    public GameConfig config;

    [SerializeField] private List<Vector3> worldPts;
    [SerializeField] private List<Vector3Int> cells;
    private int idx = 0;
    private int dir = 1;
    private bool dwelling = false;

    [SerializeField] private ResourceAmount[] cargo = new ResourceAmount[]
    {
        new ResourceAmount(ResourceType.Circle),
        new ResourceAmount(ResourceType.Triangle),
        new ResourceAmount(ResourceType.Square) 
    };

    private int CargoCount() => cargo[(int)ResourceType.Circle].Amount + cargo[(int)ResourceType.Triangle].Amount + cargo[(int)ResourceType.Square].Amount;

    public void SetPath(List<Vector3> ptsWorld, List<Vector3Int> ptsCells)
    {
        worldPts = ptsWorld;
        cells = ptsCells;
        if (worldPts == null || worldPts.Count < 2)
        {
            Destroy(gameObject);
            return;
        }
        idx = 0;
        dir = 1;
        transform.position = worldPts[0];
    }

    void Update()
    {
        if (dwelling || worldPts == null || worldPts.Count < 2)
            return;

        var target = worldPts[idx + dir];
        var step = speedUnitsPerSec * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
        float snap = ((target == worldPts[0]) || (target == worldPts[^1])) ? arriveSnap : 0f;
        if (Vector3.Distance(transform.position, target) <= snap)
        {
            idx += dir;

            if ((idx == 0 || idx == cells.Count - 1) & StationRegistry.TryGet(cells[idx], out var station))
            {
                StartCoroutine(DwellAtStation(station));
                return;
            }

            if (idx == worldPts.Count - 1 || idx == 0)
                dir = -dir;
        }
    }

    IEnumerator DwellAtStation(Station s)
    {
        dwelling = true;

        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            if (!s.Consumes(t))
                continue;
            int canUnload = Mathf.Min(cargo[(int)t].Amount, s.demand[(int)t].Amount);
            for (int i = 0; i < canUnload; i++)
            {
                yield return new WaitForSeconds(config.TimePerUnloadSec);
                cargo[(int)t]--;
                s.demand[(int)t]--;
                GlobalDemand.Outstanding[(int)t].Amount = Mathf.Max(0, GlobalDemand.Outstanding[(int)t].Amount);
            }
        }

        int free = capacity - CargoCount();
        if (free > 0)
        {
            foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            {
                if (!s.Produces(t))
                    continue;
                if (onlyLoadRequested && GlobalDemand.Outstanding[(int)t].Amount <= 0)
                    continue;

                while (free > 0 && s.supply[(int)t].Amount > 0) {
                    yield return new WaitForSeconds(config.TimePerLoadSec);
                    s.supply[(int)t]--;
                    cargo[(int)t]++;
                    free--;
                }
            }
        }

        if (idx == worldPts.Count - 1 || idx == 0)
            dir = -dir;
        dwelling = false;
    }

    public void UpgradeCapacity(int delta) => capacity = Mathf.Max(0, capacity + delta);
    public void UpgradeSpeed(float mul) => speedUnitsPerSec *= Mathf.Max(0.1f, mul);
    public ResourceAmount[] Manifest() => cargo;

    private void OnDestroy()
    {
        Debug.Log("Train destroyed");
    }
}
