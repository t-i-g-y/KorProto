using UnityEngine;
using System.Collections.Generic;

public class Train : MonoBehaviour
{
    [Header("Motion")]
    public float Speed = 4f;
    public float ArriveSnap = 0.02f;

    List<Vector3> waypoints;
    int idx = 0;
    int dir = 1;
    bool pingPong;

    public void SetPath(List<Vector3> worldPoints, bool pingPong = false)
    {
        this.pingPong = pingPong;
        waypoints = worldPoints;

        if (waypoints == null || waypoints.Count < 2)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = waypoints[0];
        idx = 0; dir = +1;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        var target = waypoints[idx + dir];
        var step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) <= ArriveSnap)
        {
            idx += dir;

            if (idx == waypoints.Count - 1 || idx == 0)
            {
                if (pingPong)
                    dir = -dir;
                else if (idx == waypoints.Count - 1)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
