using UnityEngine;
using UnityEngine.UI;

namespace YourGameNamespace
{
    public class Minimap : MonoBehaviour
    {
        public RawImage minimapImage;
        public int mapSize = 200;
        public Color groundColor = new Color(0.76f, 0.70f, 0.50f); // Desert color
        public Color copperColor = new Color(1f, 0.65f, 0f); // Orange
        public Color ironColor = new Color(0.75f, 0.75f, 0.8f); // Silver (grey/blueish)
        public Color stoneColor = new Color(0.5f, 0.5f, 0.5f); // Grey/brown
        public Color waterColor = Color.blue; // Blue for water
        public Color mountainColor = Color.gray; // Gray for mountains
        public Color lavaColor = Color.red; // Red for lava
        public Color playerColor = Color.green;

        public Color forestColor = new Color(0.1f, 0.8f, 0.1f);
        public Color desertColor = new Color(0.9f, 0.9f, 0.5f);
        public Color industrialColor = new Color(0.5f, 0.5f, 0.5f);
        public Color alienColor = new Color(0.7f, 0.1f, 0.7f);

        private Texture2D minimapTexture;
        private WorldGenerator worldGenerator;
        private BiomeManager biomeManager;
        private Transform playerTransform;
        private Vector3 lastPlayerPosition;

        void Start()
        {
            worldGenerator = FindObjectOfType<WorldGenerator>();
            biomeManager = FindObjectOfType<BiomeManager>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            lastPlayerPosition = playerTransform.position;
            
            minimapTexture = new Texture2D(mapSize, mapSize);
            minimapImage.texture = minimapTexture;

            UpdateMinimap();
        }

        void Update()
        {
            UpdateMinimap();
        }

        void UpdateMinimap()
        {
            int playerX = Mathf.RoundToInt(playerTransform.position.x / worldGenerator.cellSize);
            int playerY = Mathf.RoundToInt(playerTransform.position.y / worldGenerator.cellSize);

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    int worldX = playerX + x - mapSize / 2;
                    int worldY = playerY + y - mapSize / 2;

                    Color pixelColor = GetBiomeColor(biomeManager.GetBiomeAt(worldX, worldY));

                    if (biomeManager.IsWaterAt(new Vector2Int(worldX, worldY)))
                    {
                        pixelColor = waterColor;
                    }
                    else if (biomeManager.IsMountainAt(new Vector2Int(worldX, worldY)))
                    {
                        pixelColor = mountainColor;
                    }
                    else
                    {
                        GameObject resource = worldGenerator.GetResourceAt(new Vector2Int(worldX, worldY));
                        if (resource != null)
                        {
                            pixelColor = GetResourceColor(resource);
                        }
                    }

                    minimapTexture.SetPixel(x, y, pixelColor);
                }
            }

            DrawPlayerIcon();
            minimapTexture.Apply();
        }

        Color GetResourceColor(GameObject resource)
        {
            string resourceName = resource.name.ToLower();
            if (resourceName.Contains("copper")) return copperColor;
            if (resourceName.Contains("iron")) return ironColor;
            if (resourceName.Contains("rock") || resourceName.Contains("stone")) return stoneColor;
            // Add more resource types as needed
            return Color.white; // Default color for unknown resources
        }

        void DrawPlayerIcon()
        {
            int iconSize = 10;
            int centerX = mapSize / 2;
            int centerY = mapSize / 2;

            Vector3 playerDirection = (playerTransform.position - lastPlayerPosition).normalized;
            float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

            // Draw black outline
            for (int y = -iconSize - 1; y <= iconSize + 1; y++)
            {
                for (int x = -iconSize - 1; x <= iconSize + 1; x++)
                {
                    if (IsPointInArrow(new Vector2(x, y), angle, iconSize + 1))
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        if (pixelX >= 0 && pixelX < mapSize && pixelY >= 0 && pixelY < mapSize)
                        {
                            minimapTexture.SetPixel(pixelX, pixelY, Color.black);
                        }
                    }
                }
            }

            // Draw green arrow
            for (int y = -iconSize; y <= iconSize; y++)
            {
                for (int x = -iconSize; x <= iconSize; x++)
                {
                    if (IsPointInArrow(new Vector2(x, y), angle, iconSize))
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        if (pixelX >= 0 && pixelX < mapSize && pixelY >= 0 && pixelY < mapSize)
                        {
                            minimapTexture.SetPixel(pixelX, pixelY, playerColor);
                        }
                    }
                }
            }

            lastPlayerPosition = playerTransform.position;
        }

        bool IsPointInArrow(Vector2 point, float angle, float size)
        {
            Vector2 rotatedPoint = RotatePoint(point, -angle);
            float halfSize = size / 2f;

            // Define a more pronounced arrow shape
            return rotatedPoint.y <= halfSize && rotatedPoint.y >= -halfSize / 2f &&
                Mathf.Abs(rotatedPoint.x) <= (halfSize - rotatedPoint.y) / 2f;
        }

        Vector2 RotatePoint(Vector2 point, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos
            );
        }

        void UpdateMinimapTexture()
        {
            if (minimapTexture == null)
            {
                minimapTexture = new Texture2D(mapSize, mapSize);
            }

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Vector2Int worldPos = new Vector2Int(x, y);
                    BiomeType biomeType = worldGenerator.biomeManager.GetBiomeAt(x, y);
                    Color biomeColor = GetBiomeColor(biomeType);

                    if (worldGenerator.IsWaterOrMountainAt(worldPos))
                    {
                        minimapTexture.SetPixel(x, y, worldGenerator.biomeManager.IsWaterAt(worldPos) ? waterColor : mountainColor);
                    }
                    else
                    {
                        GameObject resource = worldGenerator.GetResourceAt(worldPos);
                        if (resource != null)
                        {
                            string resourceName = resource.name.ToLower();
                            if (resourceName.Contains("copper"))
                                minimapTexture.SetPixel(x, y, copperColor);
                            else if (resourceName.Contains("iron"))
                                minimapTexture.SetPixel(x, y, ironColor);
                            else if (resourceName.Contains("stone"))
                                minimapTexture.SetPixel(x, y, stoneColor);
                            else
                                minimapTexture.SetPixel(x, y, biomeColor);
                        }
                        else
                        {
                            minimapTexture.SetPixel(x, y, biomeColor);
                        }
                    }
                }
            }

            minimapTexture.Apply();
            minimapImage.texture = minimapTexture;
        }

        Color GetBiomeColor(BiomeType biomeType)
        {
            switch (biomeType)
            {
                case BiomeType.Forest:
                    return forestColor;
                case BiomeType.Desert:
                    return desertColor;
                case BiomeType.IndustrialWasteland:
                    return industrialColor;
                case BiomeType.AlienArea:
                    return alienColor;
                case BiomeType.Lava:
                    return lavaColor;
                default:
                    return groundColor;
            }
        }

        public void InitializeMinimap()
        {
            worldGenerator = FindObjectOfType<WorldGenerator>();
            biomeManager = FindObjectOfType<BiomeManager>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            lastPlayerPosition = playerTransform.position;
            
            minimapTexture = new Texture2D(mapSize, mapSize);
            minimapImage.texture = minimapTexture;

            UpdateMinimap();
        }

    }
    
}