using UnityEngine;
using UnityEngine.UI;

namespace YourGameNamespace
{
    public class Minimap : MonoBehaviour
    {
        [Header("Map References")]
        public RawImage minimapImage;
        public RawImage fullMapImage;    // Reference to the larger map image
        public GameObject minimapPanel;   // The small map panel
        public GameObject fullMapPanel;   // The large map panel
        private Vector2 defaultMinimapSize;
        private Vector2 fullMapSize = new Vector2(800, 600);
        public int mapWidth = 300; 
        public int mapHeight = 200;

        public Color groundColor = new Color(0.76f, 0.70f, 0.50f); // Desert color
        public Color copperColor = new Color(1f, 0.65f, 0f); // Orange
        public Color ironColor = new Color(0.75f, 0.75f, 0.8f); // Silver (grey/blueish)
        public Color stoneColor = new Color(0.5f, 0.5f, 0.5f); // Grey/brown
        public Color waterColor = Color.blue; // Blue for water
        public Color mountainColor = Color.gray; // Gray for mountains
        public Color lavaColor = Color.red; // Red for lava
        public Color woodColor = new Color(0.54f, 0.27f, 0.07f);
        public Color herbColor = new Color(0.13f, 0.55f, 0.13f);
        public Color tinColor = new Color(0.8f, 0.8f, 0.8f);
        public Color clayColor = new Color(0.76f, 0.7f, 0.5f);
        public Color wheatColor = new Color(0.93f, 0.86f, 0.51f);
        public Color carrotColor = new Color(1f, 0.55f, 0f);
        public Color oilColor = new Color(0.1f, 0.1f, 0.1f);
        public Color scrapColor = new Color(0.5f, 0.5f, 0.5f);
        public Color titaniumColor = new Color(0.75f, 0.75f, 0.75f);
        public Color uraniumColor = new Color(0.2f, 0.8f, 0.2f);
        public Color playerColor = Color.green;
        public Color rockColor = new Color(0.5f, 0.5f, 0.5f);
        public Color defaultResourceColor = new Color(0.7f, 0.7f, 0.7f);
        public Color defaultBiomeColor = new Color(0.8f, 0.8f, 0.8f);

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
    // Store the default minimap size
    defaultMinimapSize = minimapImage.rectTransform.sizeDelta;
    
    // Initialize the full map
    if (fullMapPanel != null)
    {
        fullMapPanel.SetActive(false);
        
        // Set the full map panel to fill the screen
        RectTransform panelRect = fullMapPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        if (fullMapImage != null)
        {
            // Make sure the RawImage component is enabled but the GameObject is hidden
            fullMapImage.enabled = true;
            fullMapImage.gameObject.SetActive(false);
            
            // Set the full map image to fill its parent panel
            RectTransform imageRect = fullMapImage.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.1f, 0.1f);
            imageRect.anchorMax = new Vector2(0.9f, 0.9f);
            imageRect.sizeDelta = Vector2.zero;
            imageRect.anchoredPosition = Vector2.zero;
        }
    }

    // Rest of your existing Start code
    worldGenerator = FindObjectOfType<WorldGenerator>();
    biomeManager = FindObjectOfType<BiomeManager>();
    playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    lastPlayerPosition = playerTransform.position;

    minimapTexture = new Texture2D(mapWidth, mapHeight);
    minimapImage.texture = minimapTexture;

    UpdateMinimap();
}

void Update()
{
    UpdateMinimap();
    // If full map is active, update it too
    if (fullMapPanel != null && fullMapPanel.activeSelf && fullMapImage != null && minimapTexture != null)
    {
        fullMapImage.texture = minimapTexture;
    }
}

public void ToggleFullMap()
{
    if (fullMapPanel != null && minimapTexture != null)
    {
        bool isFullMapActive = !fullMapPanel.activeSelf;
        fullMapPanel.SetActive(isFullMapActive);
        minimapPanel.SetActive(!isFullMapActive);
        
        if (fullMapImage != null)
        {
            // Toggle the GameObject instead of the RawImage component
            fullMapImage.gameObject.SetActive(isFullMapActive);
            if (isFullMapActive)
            {
                fullMapImage.texture = minimapTexture;
            }
        }
    }
}

void UpdateMinimap()
{
    int playerX = Mathf.RoundToInt(playerTransform.position.x / worldGenerator.cellSize);
    int playerY = Mathf.RoundToInt(playerTransform.position.y / worldGenerator.cellSize);

    for (int y = 0; y < mapHeight; y++)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            int worldX = playerX + x - mapWidth / 2;
            int worldY = playerY + y - mapHeight / 2;
            Vector2Int worldPos = new Vector2Int(worldX, worldY);

            Color pixelColor = GetBiomeColor(biomeManager.GetBiomeAt(worldX, worldY));

            if (biomeManager.IsWaterAt(worldPos))
            {
                pixelColor = waterColor;
            }
            else if (biomeManager.IsMountainAt(worldPos))
            {
                pixelColor = mountainColor;
            }
            else
            {
                GameObject resource = worldGenerator.GetResourceAt(worldPos);
                if (resource != null)
                {
                    string resourceName = resource.name.Replace("(Clone)", "").Trim();
                    switch (resourceName)
                    {
                        case "Rock": pixelColor = rockColor; break;
                        case "Copper": pixelColor = copperColor; break;
                        case "Iron": pixelColor = ironColor; break;
                        case "Wood": pixelColor = woodColor; break;
                        case "Herb": pixelColor = herbColor; break;
                        case "Tin": pixelColor = tinColor; break;
                        case "Clay": pixelColor = clayColor; break;
                        case "Wheat": pixelColor = wheatColor; break;
                        case "Carrot": pixelColor = carrotColor; break;
                        case "Oil": pixelColor = oilColor; break;
                        case "Scrap": pixelColor = scrapColor; break;
                        case "Titanium": pixelColor = titaniumColor; break;
                        case "Uranium": pixelColor = uraniumColor; break;
                    }
                }
            }

            minimapTexture.SetPixel(x, y, pixelColor);
        }
    }

    DrawPlayerIcon();
    minimapTexture.Apply();
}

private Color GetResourceColor(Vector2Int position)
{
    if (worldGenerator.biomeManager.IsWaterAt(position))
        return waterColor;
    if (worldGenerator.biomeManager.IsMountainAt(position))
        return mountainColor;

    GameObject resource = worldGenerator.GetResourceAt(position);
    if (resource != null)
    {
        string resourceName = resource.name.Replace("(Clone)", "").Trim();
        switch (resourceName)
        {
            case "Rock": return rockColor;
            case "Copper": return copperColor;
            case "Iron": return ironColor;
            case "Wood": return woodColor;
            case "Herb": return herbColor;
            case "Tin": return tinColor;
            case "Clay": return clayColor;
            case "Wheat": return wheatColor;
            case "Carrot": return carrotColor;
            case "Oil": return oilColor;
            case "Scrap": return scrapColor;
            case "Titanium": return titaniumColor;
            case "Uranium": return uraniumColor;
            default: return defaultResourceColor;
        }
    }

    // Return biome color if no resource
    BiomeType biomeType = worldGenerator.biomeManager.GetBiomeAt(position.x, position.y);
    switch (biomeType)
    {
        case BiomeType.Forest: return forestColor;
        case BiomeType.Desert: return desertColor;
        case BiomeType.IndustrialWasteland: return industrialColor;
        case BiomeType.AlienArea: return alienColor;
        case BiomeType.Lava: return lavaColor;
        default: return defaultBiomeColor;
    }
}

        void DrawPlayerIcon()
        {
            int iconSize = 10;
            int centerX = mapWidth / 2;
            int centerY = mapHeight / 2;

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
                        if (pixelX >= 0 && pixelX < mapWidth && pixelY >= 0 && pixelY < mapHeight)
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
                        if (pixelX >= 0 && pixelX < mapWidth && pixelY >= 0 && pixelY < mapHeight)
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
        minimapTexture = new Texture2D(mapWidth, mapHeight);
    }

    for (int x = 0; x < mapWidth; x++)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            Vector2Int worldPos = new Vector2Int(x, y);
            BiomeType biomeType = worldGenerator.biomeManager.GetBiomeAt(x, y);
            Color biomeColor = GetBiomeColor(biomeType);

            if (biomeManager.IsWaterAt(worldPos))
            {
                minimapTexture.SetPixel(x, y, waterColor);
            }
            else if (biomeManager.IsMountainAt(worldPos))
            {
                minimapTexture.SetPixel(x, y, mountainColor);
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

            minimapTexture = new Texture2D(mapWidth, mapHeight);
            minimapImage.texture = minimapTexture;

            UpdateMinimap();
        }

    }
    
}