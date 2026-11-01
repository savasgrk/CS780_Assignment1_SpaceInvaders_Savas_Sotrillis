using UnityEngine;

// Replace placeholder meshes while preserving the prefab's physics and gameplay.
public class ArcadeSpriteVisual : MonoBehaviour
{
    [SerializeField] private Texture2D artwork;
    [SerializeField] private Vector2 localSize = Vector2.one;
    [SerializeField] private Vector3 localOffset;
    [SerializeField] private Color tint = Color.white;
    private Sprite sprite;

    void Awake()
    {
        if (artwork == null) return;
        foreach (MeshRenderer placeholder in GetComponentsInChildren<MeshRenderer>())
            placeholder.enabled = false;

        sprite = Sprite.Create(artwork, new Rect(0, 0, artwork.width, artwork.height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        GameObject visual = new GameObject("Artwork", typeof(SpriteRenderer));
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = localOffset;
        visual.transform.localScale = new Vector3(localSize.x / sprite.bounds.size.x,
            localSize.y / sprite.bounds.size.y, 1f);
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
    }

    void OnDestroy()
    {
        if (sprite != null) Destroy(sprite);
    }
}
