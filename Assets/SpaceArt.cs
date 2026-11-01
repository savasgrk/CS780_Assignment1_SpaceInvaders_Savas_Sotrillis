using UnityEngine;

// Small procedural sprites keep the effects independent of external art assets.
public static class SpaceArt
{
    public static Sprite CreateGlow()
    {
        const int size = 32;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float radius = new Vector2((x + 0.5f) / size - 0.5f, (y + 0.5f) / size - 0.5f).magnitude;
            float alpha = Mathf.Exp(-radius * radius * 28f) * Mathf.Clamp01((0.5f - radius) * 10f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        return MakeSprite(size, pixels);
    }

    public static Sprite CreateShieldRing()
    {
        const int size = 128;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float radius = new Vector2((x + 0.5f) / size - 0.5f, (y + 0.5f) / size - 0.5f).magnitude;
            float ring = Mathf.Exp(-Mathf.Pow((radius - 0.44f) / 0.016f, 2f));
            float glow = 0.2f * Mathf.Exp(-Mathf.Pow((radius - 0.44f) / 0.045f, 2f));
            pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(ring + glow));
        }
        return MakeSprite(size, pixels);
    }

    public static Sprite CreatePickupIcon()
    {
        const int size = 64;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float px = (x + 0.5f) / size - 0.5f;
            float py = (y + 0.5f) / size - 0.5f;
            float distance = Mathf.Sqrt(px * px + py * py);
            bool cross = (Mathf.Abs(px) < 0.055f && Mathf.Abs(py) < 0.21f)
                || (Mathf.Abs(py) < 0.055f && Mathf.Abs(px) < 0.21f);
            if (cross) pixels[y * size + x] = Color.white;
            else if (distance < 0.44f)
                pixels[y * size + x] = distance > 0.39f
                    ? new Color(0.2f, 0.6f, 1f) : new Color(0.025f, 0.08f, 0.3f, 0.95f);
        }
        return MakeSprite(size, pixels);
    }

    public static Sprite MakeSprite(int size, Color[] pixels)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
    }
}
