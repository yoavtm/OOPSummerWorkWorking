using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CardHoverHighlighter : MonoBehaviour
{
    [Header("Pixel-perfect hover")]
    public int liftPixels = 4;
    public int pixelsPerUnit = 100;
    public float liftSpeedPixelsPerSec = 240f;

    [Header("Sorting")]
    public int sortingBoost = 1000;

    [Header("Filter")]
    public bool onlyIfOwnerIsPlayer1 = false;

    private static CardHoverHighlighter currentHighlighted;

    SpriteRenderer _sr;
    Collider2D _col;
    int _origOrder;

    bool _hovering;
    float _currentLiftPixels;
    float _lastAppliedLiftUnits;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _origOrder = _sr.sortingOrder;
        _currentLiftPixels = 0f;
        _lastAppliedLiftUnits = 0f;
    }

    void Update()
    {
        // Pause hover while hand layout animates
        if (GameManager.Instance != null && GameManager.Instance.IsAnimatingHand)
        {
            ResetHover();
            if (Mathf.Abs(_lastAppliedLiftUnits) > 0.00001f)
            {
                var pos = transform.position; // <-- renamed from 'p' to avoid CS0136
                transform.position = new Vector3(pos.x, pos.y - _lastAppliedLiftUnits, pos.z);
                _lastAppliedLiftUnits = 0f;
                _currentLiftPixels = 0f;
            }
            return;
        }

        if (Camera.main == null || _col == null) return;

        if (onlyIfOwnerIsPlayer1)
        {
            var cv = GetComponent<CardView>();
            if (cv != null && cv.Owner != null && cv.Owner.GetName() != "Player 1")
            {
                ResetHover();
                return;
            }
        }

        // New Input System only (no legacy Input.mousePosition)
        if (Mouse.current == null && Pointer.current == null)
        {
            ResetHover();
            return;
        }

        Vector2 screenPos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Pointer.current.position.ReadValue();

        float z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        Vector2 point = new Vector2(world.x, world.y);

        bool over = _col.OverlapPoint(point);

        if (over && !_hovering && currentHighlighted == null)
        {
            _hovering = true;
            currentHighlighted = this;
            _sr.sortingOrder = _origOrder + sortingBoost;
        }
        else if ((!over && _hovering) || (currentHighlighted != this))
        {
            ResetHover();
        }

        float targetLiftPixels = _hovering ? liftPixels : 0f;
        float delta = liftSpeedPixelsPerSec * Time.deltaTime;
        _currentLiftPixels = Mathf.MoveTowards(_currentLiftPixels, targetLiftPixels, delta);

        float unitsPerPixel = 1f / Mathf.Max(1, pixelsPerUnit);
        float baseY = transform.position.y - _lastAppliedLiftUnits;
        float baseYSnapped = Mathf.Round(baseY / unitsPerPixel) * unitsPerPixel;

        float liftUnitsNow = _currentLiftPixels * unitsPerPixel;
        float newY = baseYSnapped + liftUnitsNow;

        var pnow = transform.position;
        transform.position = new Vector3(pnow.x, newY, pnow.z);

        _lastAppliedLiftUnits = liftUnitsNow;
    }

    void ResetHover()
    {
        _hovering = false;
        if (currentHighlighted == this) currentHighlighted = null;
        _sr.sortingOrder = _origOrder;
    }
}
