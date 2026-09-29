using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Player control: mouse click raycasts onto the ground and sets a move destination.
/// </summary>
[RequireComponent(typeof(RaycastPathMover))]
public class ClickToMoveAgent : MonoBehaviour
{
    public Camera worldCamera;
    public LayerMask groundMask = ~0;

    RaycastPathMover _mover;
    bool _enabled = true;

    void Awake()
    {
        _mover = GetComponent<RaycastPathMover>();
    }

    public void SetInputEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    void Update()
    {
        if (!_enabled || worldCamera == null) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        Ray ray = worldCamera.ScreenPointToRay(screen);

        // Prefer 2D intersection against ground collider
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, 100f, groundMask);
        if (hit2D.collider != null)
        {
            _mover.SetDestination(hit2D.point);
            return;
        }

        // Fallback: project onto z=0 plane
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);
            _mover.SetDestination(point);
        }
    }
}
