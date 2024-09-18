using UnityEngine;
using YourGameNamespace;

public class WorldSetup : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject player;
    public Sprite forestTileSprite;
    public Sprite desertTileSprite;
    public Sprite industrialWastelandTileSprite;
    public Sprite alienAreaTileSprite;
    public Sprite lavaTileSprite;
    public float tileSize = 8f; 
    public int visibleTilesX = 25; // Adjust these values to control how many tiles are visible
    public int visibleTilesY = 25;
    public int worldSizeX = 200;
    public int worldSizeY = 200;

    void Start()
    {
        if (worldGenerator != null && player != null)
        {
            BiomeManager biomeManager = FindObjectOfType<BiomeManager>();
            if (biomeManager == null)
            {
                Debug.LogError("BiomeManager not found in the scene!");
                return;
            }

            // Initialize biome map
            biomeManager.InitializeBiomeMap();

            Vector2 worldCenter = worldGenerator.GetWorldCenter();
            player.transform.position = new Vector3(worldCenter.x, worldCenter.y, player.transform.position.z);
            
            // Generate world background (which includes biomes)
            CreateWorldBackground();
            
            // Generate resources
            worldGenerator.GenerateWorld(worldCenter);

            Minimap minimap = FindObjectOfType<Minimap>();
            if (minimap != null)
            {
                minimap.InitializeMinimap();
            }
            else
            {
                Debug.LogError("Minimap not found in the scene!");
            }
        }
        else
        {
            Debug.LogError("WorldGenerator or Player not assigned to WorldSetup!");
        }
    }

    void CreateWorldBackground()
    {
        Vector2 worldSize = worldGenerator.GetWorldSize();
        int tilesX = Mathf.CeilToInt(worldSize.x / tileSize);
        int tilesY = Mathf.CeilToInt(worldSize.y / tileSize);

        for (int x = 0; x < visibleTilesX; x++)
        {
            for (int y = 0; y < visibleTilesY; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 1f);
                BiomeType biomeType = worldGenerator.biomeManager.GetBiomeAt(x, y);
                CreateBiomeTile(position, biomeType);
            }
        }
    }

    private BiomeType DetermineBiomeType(int x, int y)
    {
        float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(worldSizeX / 2, worldSizeY / 2));
        float maxDistance = Mathf.Sqrt(worldSizeX * worldSizeX + worldSizeY * worldSizeY) / 2;

        if (distanceFromCenter < maxDistance * 0.15f)
            return BiomeType.Forest;
        else if (distanceFromCenter < maxDistance * 0.35f)
            return BiomeType.Desert;
        else if (distanceFromCenter < maxDistance * 0.55f)
            return BiomeType.IndustrialWasteland;
        else if (distanceFromCenter < maxDistance * 0.75f)
            return BiomeType.AlienArea;
        else
            return BiomeType.Lava;
    }

    void CreateBiomeTile(Vector3 position, BiomeType biomeType)
    {
        GameObject tile = new GameObject($"BiomeTile_{position.x}_{position.y}");
        tile.transform.position = position;
        tile.transform.SetParent(transform);

        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetBiomeSprite(biomeType);
        spriteRenderer.sortingOrder = -1;

        tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
    }

    Sprite GetBiomeSprite(BiomeType biomeType)
    {
        switch (biomeType)
        {
            case BiomeType.Forest:
                return forestTileSprite;
            case BiomeType.Desert:
                return desertTileSprite;
            case BiomeType.IndustrialWasteland:
                return industrialWastelandTileSprite;
            case BiomeType.AlienArea:
                return alienAreaTileSprite;
            case BiomeType.Lava:
                return lavaTileSprite;
            default:
                return forestTileSprite;
        }
    }
}