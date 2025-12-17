using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomSpeed = 90f; 
    public float minZoom = 5f;
    public float maxZoom = 20f;

    private Camera cam;

    [Header("Pan Settings")]
    public float panSpeed = 20f;
    private Vector3 dragOrigin;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            Debug.LogError("[CameraController] Camera component not found on this GameObject!");
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            float newSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime, minZoom, maxZoom);

            Vector3 diff = mouseWorldPos - cam.transform.position;
            float sizeRatio = newSize / cam.orthographicSize;
            cam.transform.position += diff * (1 - sizeRatio);

            cam.orthographicSize = newSize;
        }
    }

    void HandlePan()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            cam.transform.position += difference;
        }
    }
}
