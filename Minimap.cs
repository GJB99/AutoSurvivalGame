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
        public Color playerColor = Color.red;

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
            int playerX = Mathf.RoundToInt(playerTransform.position.x);
            int playerY = Mathf.RoundToInt(playerTransform.position.y);

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    int worldX = playerX + x - mapSize / 2;
                    int worldY = playerY + y - mapSize / 2;

                    Color pixelColor = groundColor;

                    GameObject resource = worldGenerator.GetResourceAt(new Vector2Int(worldX, worldY));
                    if (resource != null)
                    {
                        string resourceName = resource.name.ToLower();
                        if (resourceName.Contains("copper"))
                            pixelColor = copperColor;
                        else if (resourceName.Contains("iron"))
                            pixelColor = ironColor;
                        else if (resourceName.Contains("rock"))
                            pixelColor = stoneColor;
                    }

                    // Check for water and mountain biomes
                    if (biomeManager.IsWaterAt(new Vector2Int(worldX, worldY)))
                    {
                        pixelColor = waterColor;
                    }
                    else if (biomeManager.IsMountainAt(new Vector2Int(worldX, worldY)))
                    {
                        pixelColor = mountainColor;
                    }

                    minimapTexture.SetPixel(x, y, pixelColor);
                }
            }

            DrawPlayerIcon();

            minimapTexture.Apply();
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
    }
}