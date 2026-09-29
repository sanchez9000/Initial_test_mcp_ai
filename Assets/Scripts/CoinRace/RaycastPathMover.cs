using UnityEngine;

/// <summary>
/// Moves toward a destination with short forward raycasts for simple avoidance.
/// Uses Rigidbody2D.MovePosition so trigger pickups register.
/// </summary>
public class RaycastPathMover : MonoBehaviour
{
    public float speed = 6f;
    public float arriveDistance = 0.15f;
    public float avoidDistance = 0.7f;
    public float arenaHalfExtent = 9f;
    public LineRenderer pathLine;
    public LayerMask avoidMask = ~0;

    Rigidbody2D _rb;
    Vector2 _destination;
    bool _hasDestination;
    bool _frozen;

    public bool HasDestination => _hasDestination;
    public Vector2 Destination => _destination;
    public bool IsMoving => _hasDestination && !_frozen;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void SetFrozen(bool frozen)
    {
        _frozen = frozen;
        if (frozen)
            ClearDestination();
    }

    public void SetDestination(Vector2 worldPos)
    {
        if (_frozen) return;
        _destination = ClampToArena(worldPos);
        _hasDestination = true;
        UpdatePathLine();
    }

    public void ClearDestination()
    {
        _hasDestination = false;
        if (pathLine != null)
            pathLine.enabled = false;
    }

    void FixedUpdate()
    {
        if (_frozen || !_hasDestination) return;

        Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 toTarget = _destination - pos;
        float dist = toTarget.magnitude;
        if (dist <= arriveDistance)
        {
            ClearDestination();
            return;
        }

        Vector2 dir = toTarget / dist;
        RaycastHit2D hit = Physics2D.CircleCast(pos, 0.25f, dir, avoidDistance, avoidMask);
        if (hit.collider != null
            && hit.collider.gameObject != gameObject
            && hit.collider.GetComponent<CoinPickup>() == null
            && hit.collider.GetComponent<ClickToMoveAgent>() == null
            && hit.collider.GetComponent<CoinRaceAI>() == null
            && hit.collider.gameObject.name != "Ground")
        {
            Vector2 slide = Vector2.Perpendicular(hit.normal);
            if (Vector2.Dot(slide, dir) < 0f) slide = -slide;
            dir = (dir + slide * 1.25f).normalized;
        }

        Vector2 next = ClampToArena(pos + dir * speed * Time.fixedDeltaTime);
        if (_rb != null)
            _rb.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, 0f);
        UpdatePathLine();
    }

    Vector2 ClampToArena(Vector2 p)
    {
        float e = arenaHalfExtent;
        return new Vector2(Mathf.Clamp(p.x, -e, e), Mathf.Clamp(p.y, -e, e));
    }

    void UpdatePathLine()
    {
        if (pathLine == null) return;
        if (!_hasDestination)
        {
            pathLine.enabled = false;
            return;
        }
        pathLine.enabled = true;
        pathLine.positionCount = 2;
        pathLine.SetPosition(0, transform.position);
        pathLine.SetPosition(1, new Vector3(_destination.x, _destination.y, 0f));
    }
}
