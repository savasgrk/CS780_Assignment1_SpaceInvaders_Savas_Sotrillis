using UnityEngine;

// Visual-only effects: no colliders, forces, or changes to gameplay randomness.
public class CollisionBurst : MonoBehaviour
{
    private Sprite ringSprite;
    private Sprite glowSprite;
    private SpriteRenderer ring;
    private SpriteRenderer flash;
    private SpriteRenderer[] sparks;
    private Vector3[] directions;
    private Color color;
    private float size;
    private float duration;
    private float age;

    public static void Spawn(Vector3 position, Color color, float size = 1f, float duration = 0.45f)
    {
        GameObject root = new GameObject("Collision burst");
        root.transform.position = new Vector3(position.x, position.y, -1f);
        CollisionBurst burst = root.AddComponent<CollisionBurst>();
        burst.color = color;
        burst.size = size;
        burst.duration = duration;
        burst.Build();
    }

    private void Build()
    {
        ringSprite = SpaceArt.CreateShieldRing();
        glowSprite = SpaceArt.CreateGlow();
        ring = MakeVisual("Shockwave", ringSprite);
        flash = MakeVisual("Flash", glowSprite);
        sparks = new SpriteRenderer[10];
        directions = new Vector3[sparks.Length];
        for (int i = 0; i < sparks.Length; i++)
        {
            float angle = i * 2.39996f;
            directions[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            sparks[i] = MakeVisual("Spark", glowSprite);
            sparks[i].transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
        }
        Animate(0f);
    }

    private SpriteRenderer MakeVisual(string name, Sprite sprite)
    {
        GameObject obj = new GameObject(name, typeof(SpriteRenderer));
        obj.transform.SetParent(transform, false);
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 20;
        return renderer;
    }

    void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.State == GameSession.RoundState.Paused) return;
        // Keep final-hit effects moving even after the round freezes gameplay.
        age += Time.unscaledDeltaTime;
        if (age >= duration)
        {
            Destroy(gameObject);
            return;
        }
        Animate(age / duration);
    }

    private void Animate(float progress)
    {
        float fade = (1f - progress) * (1f - progress);
        float expansion = 1f - Mathf.Pow(1f - progress, 3f);
        ring.transform.localScale = Vector3.one * size * Mathf.Lerp(0.15f, 1.7f, expansion);
        ring.color = new Color(color.r, color.g, color.b, fade * 0.8f);
        flash.transform.localScale = Vector3.one * size * Mathf.Lerp(0.9f, 0.2f, progress);
        flash.color = Color.Lerp(Color.white, color, Mathf.Min(1f, progress * 5f));
        Color flashColor = flash.color;
        flashColor.a = Mathf.Clamp01(1f - progress * 3f);
        flash.color = flashColor;
        for (int i = 0; i < sparks.Length; i++)
        {
            float distance = size * expansion * (0.5f + (i % 3) * 0.2f);
            sparks[i].transform.localPosition = directions[i] * distance;
            sparks[i].transform.localScale = new Vector3(0.3f, 0.1f, 1f) * size * (1f - progress);
            sparks[i].color = new Color(color.r, color.g, color.b, fade);
        }
    }

    void OnDestroy()
    {
        if (ringSprite != null) { Destroy(ringSprite.texture); Destroy(ringSprite); }
        if (glowSprite != null) { Destroy(glowSprite.texture); Destroy(glowSprite); }
    }
}
