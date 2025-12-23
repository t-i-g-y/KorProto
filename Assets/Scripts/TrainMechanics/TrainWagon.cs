using UnityEngine;

public class TrainWagon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private float followDistance = 1f;
    private Train ownerTrain;
    private int indexInTrain;

    public void Init(Train train, int index)
    {
        ownerTrain = train;
        indexInTrain = index;
    }

    private void Update()
    {
        if (ownerTrain == null) 
            return;

        Vector3 pos = ownerTrain.GetWagonPosition(indexInTrain, followDistance);
        transform.position = pos;

        Vector3 dir = ownerTrain.GetWagonDirection(indexInTrain, followDistance);
        if (dir.sqrMagnitude > 0.0001f)
        {
            var rot = Quaternion.LookRotation(Vector3.forward, dir);
            transform.rotation = rot;
        }
    }
}

