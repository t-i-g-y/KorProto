using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float zoomMin = 10f;
    [SerializeField] private float zoomMax = 60f;
    [SerializeField] private Vector2 cameraLimitMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 cameraLimitMax = new Vector2(50f, 50f);

    private Camera cam;
    private Vector2 moveInput;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        ConstrainCameraPosition();
    }

    private void HandleMovement()
    {
        moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1f : (Keyboard.current.aKey.isPressed ? -1f : 0f),
            Keyboard.current.wKey.isPressed ? 1f : (Keyboard.current.sKey.isPressed ? -1f : 0f)
        );

        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;

        if (Mouse.current.position.x.ReadValue() >= Screen.width - 10)
        {
            move.x = moveSpeed * Time.deltaTime;
        }
        else if (Mouse.current.position.x.ReadValue() <= 10)
        {
            move.x = -moveSpeed * Time.deltaTime;
        }

        if (Mouse.current.position.y.ReadValue() >= Screen.height - 10)
        {
            move.y = moveSpeed * Time.deltaTime;
        }
        else if (Mouse.current.position.y.ReadValue() <= 10)
        {
            move.y = -moveSpeed * Time.deltaTime;
        }

        transform.Translate(move);
    }

    private void HandleZoom()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (scrollInput != 0f)
        {
            float newZoom = cam.orthographicSize - scrollInput * scrollSpeed;
            cam.orthographicSize = Mathf.Clamp(newZoom, zoomMin, zoomMax);
        }
    }

    private void ConstrainCameraPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, cameraLimitMin.x, cameraLimitMax.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, cameraLimitMin.y, cameraLimitMax.y);
        transform.position = clampedPosition;
    }
}

