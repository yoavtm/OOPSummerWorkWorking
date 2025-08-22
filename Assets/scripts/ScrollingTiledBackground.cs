using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollingTiledBackground : MonoBehaviour
{
    [Header("Tiling")]
    public float tilesPerUnitX = 1f;
    public float tilesPerUnitY = 1f;

    [Header("Scroll (per second)")]
    public float scrollSpeedX = 0.05f;   // +X = right
    public float scrollSpeedY = -0.05f;  // -Y = down

    [Header("Render Order")]
    public bool forceBackgroundQueue = true;
    public int backgroundQueue = 1000;   // "Background" queue

    private Renderer rend;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (!rend) return;

        // Ensure this draws before everything else
        if (forceBackgroundQueue && rend.material != null)
            rend.material.renderQueue = backgroundQueue;

        // Repeat + crisp pixels
        if (rend.material != null && rend.material.mainTexture != null)
            rend.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void Update()
    {
        // Animate UV offset
        offset.x = (offset.x + scrollSpeedX * Time.deltaTime) % 1f;
        offset.y = (offset.y + scrollSpeedY * Time.deltaTime) % 1f;

        if (rend.material != null)
        {
            rend.material.mainTextureOffset = offset;

            // Tile based on world scale so stretching duplicates the pattern
            var s = transform.lossyScale;
            var tiling = new Vector2(
                Mathf.Max(0.0001f, s.x * tilesPerUnitX),
                Mathf.Max(0.0001f, s.y * tilesPerUnitY)
            );
            rend.material.mainTextureScale = tiling;
        }
    }
}
