using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceBackdrop : MonoBehaviour
{
    private Camera gameplayCamera;
    private Sprite backgroundSprite;
    private Transform background;
    private Canvas lineCanvas;
    private RectTransform defeatLine;

    void Start()
    {
        gameplayCamera = Camera.main;
        if (gameplayCamera == null) return;
        gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
        gameplayCamera.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
        backgroundSprite = CreateStarfield();
        background = MakeVisual("Nebula and stars", backgroundSprite, -100);
        GameObject overlay = new GameObject("Defeat line overlay", typeof(RectTransform), typeof(Canvas));
        lineCanvas = overlay.GetComponent<Canvas>();
        lineCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        lineCanvas.sortingOrder = 50;
        GameObject line = new GameObject("Alien defeat line", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(overlay.transform, false);
        defeatLine = line.GetComponent<RectTransform>();
        Image lineImage = line.GetComponent<Image>();
        lineImage.color = new Color(1f, 0.3f, 0.35f, 0.9f);
        lineImage.raycastTarget = false;
        GameObject label = new GameObject("Defeat line label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(line.transform, false);
        TextMeshProUGUI lineLabel = label.GetComponent<TextMeshProUGUI>();
        lineLabel.text = "DEFEND THIS LINE";
        lineLabel.fontSize = 16f;
        lineLabel.alignment = TextAlignmentOptions.TopLeft;
        lineLabel.color = new Color(1f, 0.5f, 0.55f);
        lineLabel.raycastTarget = false;
        lineLabel.rectTransform.anchorMin = lineLabel.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        lineLabel.rectTransform.pivot = new Vector2(0f, 1f);
        lineLabel.rectTransform.anchoredPosition = new Vector2(12f, -8f);
        lineLabel.rectTransform.sizeDelta = new Vector2(300f, 28f);
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
        if (GameSession.Instance == null || defeatLine == null) return;
        float screenY = gameplayCamera.WorldToScreenPoint(new Vector3(0f, GameSession.Instance.DefeatLineY, 0f)).y;
        float anchorY = screenY / Mathf.Max(1, Screen.height);
        defeatLine.anchorMin = new Vector2(gameplayCamera.rect.xMin, anchorY);
        defeatLine.anchorMax = new Vector2(gameplayCamera.rect.xMax, anchorY);
        defeatLine.anchoredPosition = Vector2.zero;
        defeatLine.sizeDelta = new Vector2(0f, Mathf.Max(3f, Screen.height * 0.002f));
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
        if (lineCanvas != null) Destroy(lineCanvas.gameObject);
    }
}
