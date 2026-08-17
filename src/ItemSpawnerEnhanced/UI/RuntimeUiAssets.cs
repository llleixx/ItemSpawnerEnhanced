using UnityEngine;

namespace ItemSpawnerEnhanced.UI;

internal static class RuntimeUiAssets
{
    private static Texture2D? _heartTexture;
    private static Texture2D? _filterClearTexture;
    private static Texture2D? _searchClearTexture;
    private static Sprite? _roundedRectSprite;

    public static Texture2D HeartTexture => _heartTexture ??= CreateHeartTexture();
    public static Texture2D FilterClearTexture => _filterClearTexture ??= CreateFilterClearTexture();
    public static Texture2D SearchClearTexture => _searchClearTexture ??= CreateSearchClearTexture();
    public static Sprite RoundedRectSprite => _roundedRectSprite ??= CreateRoundedRectSprite();

    public static void Release()
    {
        if (_roundedRectSprite != null)
        {
            Texture2D roundedRectTexture = _roundedRectSprite.texture;
            Object.Destroy(_roundedRectSprite);
            Object.Destroy(roundedRectTexture);
            _roundedRectSprite = null;
        }

        DestroyTexture(ref _heartTexture);
        DestroyTexture(ref _filterClearTexture);
        DestroyTexture(ref _searchClearTexture);
    }

    private static Texture2D CreateHeartTexture()
    {
        const int size = 32;
        const int samplesPerAxis = 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = "ItemSpawnerEnhanced Heart",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        float pointX = x + (sampleX + 0.5f) / samplesPerAxis;
                        float pointY = y + (sampleY + 0.5f) / samplesPerAxis;
                        float normalizedX = (pointX - 16f) / 11f;
                        float normalizedY = (pointY - 14.5f) / 11f;
                        float sum = normalizedX * normalizedX + normalizedY * normalizedY - 1f;
                        if (sum * sum * sum - normalizedX * normalizedX * normalizedY * normalizedY * normalizedY <= 0f)
                            insideSamples++;
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private static Sprite CreateRoundedRectSprite()
    {
        const int size = 32;
        const float radius = 8f;
        const int samplesPerAxis = 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = "ItemSpawnerEnhanced Rounded Rectangle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        Vector2 center = new(size * 0.5f, size * 0.5f);
        float straightHalfExtent = size * 0.5f - radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        Vector2 point = new(
                            x + (sampleX + 0.5f) / samplesPerAxis,
                            y + (sampleY + 0.5f) / samplesPerAxis);
                        Vector2 distance = new(
                            Mathf.Abs(point.x - center.x) - straightHalfExtent,
                            Mathf.Abs(point.y - center.y) - straightHalfExtent);
                        Vector2 outside = new(Mathf.Max(distance.x, 0), Mathf.Max(distance.y, 0));
                        float signedDistance = outside.magnitude + Mathf.Min(Mathf.Max(distance.x, distance.y), 0) - radius;
                        if (signedDistance <= 0)
                            insideSamples++;
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 100f,
            extrude: 0,
            SpriteMeshType.FullRect,
            new Vector4(9, 9, 9, 9));
        sprite.name = "ItemSpawnerEnhanced Rounded Rectangle";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static Texture2D CreateFilterClearTexture() => CreateLineTexture(
        "ItemSpawnerEnhanced Clear Filters",
        new Vector2[]
        {
            new(4, 26), new(27, 26),
            new(4, 26), new(13, 16),
            new(27, 26), new(20, 18),
            new(13, 16), new(13, 5),
            new(13, 5), new(18, 8),
            new(18, 8), new(18, 14),
            new(20, 15), new(28, 7),
            new(28, 15), new(20, 7)
        });

    private static Texture2D CreateSearchClearTexture() => CreateLineTexture(
        "ItemSpawnerEnhanced Clear Search",
        new Vector2[]
        {
            new(7, 7), new(25, 25),
            new(25, 7), new(7, 25)
        });

    private static Texture2D CreateLineTexture(string name, Vector2[] segments)
    {
        const int size = 32;
        const int samplesPerAxis = 4;
        const float halfStroke = 1.35f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        Vector2 point = new(
                            x + (sampleX + 0.5f) / samplesPerAxis,
                            y + (sampleY + 0.5f) / samplesPerAxis);
                        for (int segment = 0; segment < segments.Length; segment += 2)
                        {
                            if (DistanceToSegment(point, segments[segment], segments[segment + 1]) <= halfStroke)
                            {
                                insideSamples++;
                                break;
                            }
                        }
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon)
            return Vector2.Distance(point, start);
        float position = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * position);
    }

    private static void DestroyTexture(ref Texture2D? texture)
    {
        if (texture != null)
            Object.Destroy(texture);
        texture = null;
    }
}
