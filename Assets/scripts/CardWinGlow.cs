// CardWinGlow.cs  — single green overlay (no outline clones)
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CardWinGlow : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private Color overlayColor = new Color(0f, 1f, 0f, 0.45f); // green with alpha
    [SerializeField] private bool pulse = true;          // pulse alpha over time
    [SerializeField] private float duration = 0.6f;      // seconds to show/pulse
    [SerializeField] private bool easeInOut = true;

    [Header("Placement")]
    [Tooltip("If true, force overlay to draw on top of everything (recommended). If false, use relativeOrder.")]
    [SerializeField] private bool forceTopmost = true;
    [Tooltip("Used only when forceTopmost=false. Negative = behind the card, positive = in front.")]
    [SerializeField] private int relativeOrder = 1;

    [Header("Topmost Settings")]
    [SerializeField] private int topSortingBoost = 10000; // big boost over the card
    [SerializeField] private int topRenderQueue = 4000;   // Overlay queue

    private SpriteRenderer baseSR;
    private Coroutine running;

    void Awake()
    {
        baseSR = GetComponent<SpriteRenderer>();
    }

    public void PlayGlow(float seconds = -1f)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(OverlayRoutine(seconds > 0f ? seconds : duration));
    }

    private IEnumerator OverlayRoutine(float seconds)
    {
        if (!baseSR || !baseSR.sprite) yield break;

        // Create one child with the same sprite, tinted green
        var go = new GameObject("GlowOverlay");
        go.transform.SetParent(transform, false); // same position/rotation/scale
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = baseSR.sprite;
        sr.flipX = baseSR.flipX;
        sr.flipY = baseSR.flipY;
        sr.sortingLayerID = baseSR.sortingLayerID;

        if (forceTopmost)
        {
            sr.sortingOrder = baseSR.sortingOrder + topSortingBoost;
            var mat = sr.material;    // instanced material for this SR
            mat.renderQueue = topRenderQueue; // Overlay
            sr.material = mat;
        }
        else
        {
            sr.sortingOrder = baseSR.sortingOrder + relativeOrder;
        }

        // start transparent; we’ll animate the alpha
        sr.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            float alpha = pulse
                ? (easeInOut ? Mathf.Sin(a * Mathf.PI) : a * (1f - a) * 4f) // pulse up & down
                : 1f;                                                       // constant

            float useA = overlayColor.a * alpha;
            sr.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, useA);
            yield return null;
        }

        if (sr) Destroy(sr.gameObject);
        running = null;
    }
}
