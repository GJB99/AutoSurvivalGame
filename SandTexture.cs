using UnityEngine;

public class SandTexture : MonoBehaviour
{
    public int textureSize = 512;
    public float noiseScale = 20f;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int octaves = 4;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GenerateSandTexture();
    }

    void GenerateSandTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] colorMap = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / noiseScale * frequency;
                    float sampleY = y / noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                float normalizedHeight = (noiseHeight + 1) / 2f;
                colorMap[y * textureSize + x] = new Color(normalizedHeight, normalizedHeight * 0.95f, normalizedHeight * 0.8f);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
}