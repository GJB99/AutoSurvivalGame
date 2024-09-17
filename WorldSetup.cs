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
    public float tileSize = 4f;

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

            Vector2 forestCenter = worldGenerator.GetForestCenter();
            player.transform.position = new Vector3(forestCenter.x, forestCenter.y, player.transform.position.z);
            
            // Generate biomes first
            biomeManager.GenerateBiomes(new Vector2Int(Mathf.RoundToInt(forestCenter.x), Mathf.RoundToInt(forestCenter.y)));
            
            // Then generate the world
            worldGenerator.GenerateWorld(forestCenter);
            CreateWorldBackground();

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

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 1f);
                GameObject tile = new GameObject($"BackgroundTile_{x}_{y}");
                tile.transform.position = position;
                tile.transform.SetParent(transform);

                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                BiomeType biomeType = worldGenerator.biomeManager.GetBiomeAt(x, y);
                spriteRenderer.sprite = GetBiomeSprite(biomeType);
                spriteRenderer.sortingOrder = -1;
            }
        }
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