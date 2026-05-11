using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomePopupSpawner : MonoBehaviour
{
    public static IncomePopupSpawner Instance { get; private set; }

    [SerializeField] private IncomePopup popupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private float timeBetweenPopups = 0.12f;
    [SerializeField] private float verticalStackSpacing = 0.28f;

    private readonly Dictionary<Transform, Coroutine> activeRunners = new();
    private readonly Dictionary<Transform, Queue<PopupRequest>> queues = new();

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
    }

    public void QueueRailIncome(Transform anchor, Vector3 worldBasePosition, float amount)
    {
        if (amount <= 0f)
            return;

        Enqueue(anchor, new PopupRequest(worldBasePosition, $"+{amount:0.##}", 0));
    }

    public void QueueCargoSale(Transform anchor, Vector3 worldBasePosition, ResourceType resource, float amount, int stackIndex)
    {
        if (amount <= 0f)
            return;

        Enqueue(anchor, new PopupRequest(worldBasePosition, $"+{amount:0.##} {resource}", stackIndex));
    }

    private void Enqueue(Transform anchor, PopupRequest request)
    {
        if (anchor == null || popupPrefab == null)
            return;

        if (!queues.TryGetValue(anchor, out Queue<PopupRequest> queue))
        {
            queue = new Queue<PopupRequest>();
            queues[anchor] = queue;
        }

        queue.Enqueue(request);

        if (!activeRunners.ContainsKey(anchor))
            activeRunners[anchor] = StartCoroutine(RunQueue(anchor));
    }

    private IEnumerator RunQueue(Transform anchor)
    {
        while (anchor != null &&
               queues.TryGetValue(anchor, out Queue<PopupRequest> queue) &&
               queue.Count > 0)
        {
            PopupRequest request = queue.Dequeue();

            Vector3 spawnPos = request.WorldBasePosition + Vector3.up * (request.StackIndex * verticalStackSpacing);

            IncomePopup popup = Instantiate(
                popupPrefab,
                spawnPos,
                Quaternion.identity,
                popupParent
            );

            popup.Initialize(request.Text, anchor);

            yield return new WaitForSeconds(timeBetweenPopups);
        }

        activeRunners.Remove(anchor);
        queues.Remove(anchor);
    }

    private readonly struct PopupRequest
    {
        public readonly Vector3 WorldBasePosition;
        public readonly string Text;
        public readonly int StackIndex;

        public PopupRequest(Vector3 worldBasePosition, string text, int stackIndex)
        {
            WorldBasePosition = worldBasePosition;
            Text = text;
            StackIndex = stackIndex;
        }
    }
}