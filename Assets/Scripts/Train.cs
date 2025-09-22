using UnityEngine;
using System.Collections.Generic;

public class Train : MonoBehaviour
{
    [Header("Motion")]
    public float Speed = 4f;
    public float ArriveSnap = 0.02f;

    private RailLine attachedLine;
    private List<Vector3> cells;
    private int pos = 0;
    private int dir = 1;

    public void SetPath(List<Vector3> line)
    {
        //attachedLine = line;
        cells = line;

        if (cells == null || cells.Count < 2)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = cells[0];
        pos = 0;
        dir = 1;
    }

    void Update()
    {
        if (cells == null || cells.Count < 2)
            return;

        var target = cells[pos + dir];
        var step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) <= ArriveSnap)
        {
            pos += dir;

            if (pos == cells.Count - 1 || pos == 0)
            {
                dir = -dir;
            }
        }
    }
}
