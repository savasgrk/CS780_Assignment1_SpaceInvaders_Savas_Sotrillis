using TMPro;
using UnityEngine;

public class SpaceBackdrop : MonoBehaviour
{
    private Camera gameplayCamera;
    private Sprite backgroundSprite;
    private Sprite lineSprite;
    private Transform background;
    private Transform defeatLine;
    private TextMeshPro lineLabel;

    void Start()
    {
        gameplayCamera = Camera.main;
        if (gameplayCamera == null) return;
        gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
        gameplayCamera.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
        backgroundSprite = CreateStarfield();
        background = MakeVisual("Nebula and stars", backgroundSprite, -100);
        lineSprite = SpaceArt.MakeSprite(1, new[] { Color.white });
        defeatLine = MakeVisual("Alien defeat line", lineSprite, -1);
        defeatLine.GetComponent<SpriteRenderer>().color = new Color(1f, 0.3f, 0.35f, 0.75f);
        GameObject label = new GameObject("Defeat line label", typeof(TextMeshPro));
        label.transform.SetParent(transform, false);
        lineLabel = label.GetComponent<TextMeshPro>();
        lineLabel.text = "DEFEND THIS LINE";
        lineLabel.fontSize = 2.3f;
        lineLabel.alignment = TextAlignmentOptions.Center;
        lineLabel.color = new Color(1f, 0.5f, 0.55f);
        lineLabel.rectTransform.sizeDelta = new Vector2(8f, 0.6f);
        lineLabel.GetComponent<MeshRenderer>().sortingOrder = -1;
        LateUpdate();
    }

    private Transform MakeVisual(string name, Sprite sprite, int order)
    {
        GameObject obj = new GameObject(name, typeof(SpriteRenderer));
        obj.transform.SetParent(transform, false);
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        return obj.transform;
    }

    void LateUpdate()
    {
        if (gameplayCamera == null || background == null) return;
        float depth = gameplayCamera.WorldToViewportPoint(Vector3.zero).z;
        Vector3 left = gameplayCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
        Vector3 right = gameplayCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
        Vector3 bottom = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
        Vector3 top = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
        background.position = new Vector3((left.x + right.x) * 0.5f, (bottom.y + top.y) * 0.5f, 5f);
        background.localScale = new Vector3(right.x - left.x, top.y - bottom.y, 1f);
        float lineY = GameSession.Instance.DefeatLineY;
        defeatLine.position = new Vector3((left.x + right.x) * 0.5f, lineY, 0.5f);
        defeatLine.localScale = new Vector3(right.x - left.x, 0.035f, 1f);
        lineLabel.transform.position = new Vector3(left.x + 4f, lineY - 0.4f, 0.5f);
    }

    private static Sprite CreateStarfield()
    {
        const int size = 512;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (float)x / size;
            float v = (float)y / size;
            float purple = Mathf.Exp(-((u - 0.2f) * (u - 0.2f) * 8f + (v - 0.75f) * (v - 0.75f) * 18f));
            float blue = Mathf.Exp(-((u - 0.8f) * (u - 0.8f) * 12f + (v - 0.35f) * (v - 0.35f) * 12f));
            float wisps = 0.6f + 0.4f * Mathf.PerlinNoise(u * 5f, v * 7f);
            pixels[y * size + x] = new Color(0.012f + purple * wisps * 0.065f,
                0.018f + blue * wisps * 0.07f, 0.045f + (purple + blue) * wisps * 0.11f);
        }
        // A separate random generator leaves gameplay randomness untouched.
        System.Random random = new System.Random(731);
        for (int i = 0; i < 280; i++)
        {
            int x = random.Next(2, size - 2);
            int y = random.Next(2, size - 2);
            float brightness = 0.3f + (float)random.NextDouble() * 0.65f;
            pixels[y * size + x] = new Color(brightness * 0.8f, brightness * 0.9f, brightness);
            if (i % 12 == 0)
            {
                Color glow = new Color(0.12f, 0.16f, 0.25f);
                pixels[y * size + x - 1] += glow;
                pixels[y * size + x + 1] += glow;
                pixels[(y - 1) * size + x] += glow;
                pixels[(y + 1) * size + x] += glow;
            }
        }
        return SpaceArt.MakeSprite(size, pixels);
    }

    void OnDestroy()
    {
        if (backgroundSprite != null) { Destroy(backgroundSprite.texture); Destroy(backgroundSprite); }
        if (lineSprite != null) { Destroy(lineSprite.texture); Destroy(lineSprite); }
    }
}
