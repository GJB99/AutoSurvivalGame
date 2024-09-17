using UnityEngine;
using YourGameNamespace;

public class DesertSetup : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject player;
    public Sprite desertTileSprite;
    public float tileSize = 4f; // Size of each tile in world units

    void Start()
    {
        // Generate the world
        if (worldGenerator != null)
        {
            worldGenerator.GenerateWorld();
            CreateDesertBackground();

            // Set player position to the center of the world
            if (player != null)
            {
                Vector2 worldCenter = worldGenerator.GetWorldCenter();
                player.transform.position = new Vector3(worldCenter.x, worldCenter.y, player.transform.position.z);
            }
            else
            {
                Debug.LogError("Player not assigned to DesertSetup!");
            }
        }
        else
        {
            Debug.LogError("WorldGenerator not assigned to DesertSetup!");
        }
    }

    void CreateDesertBackground()
    {
        Vector2 worldSize = worldGenerator.GetWorldSize();
        int tilesX = Mathf.CeilToInt(worldSize.x / tileSize);
        int tilesY = Mathf.CeilToInt(worldSize.y / tileSize);

        GameObject backgroundParent = new GameObject("DesertBackground");

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                GameObject tile = new GameObject($"DesertTile_{x}_{y}");
                tile.transform.SetParent(backgroundParent.transform);

                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = desertTileSprite;
                spriteRenderer.sortingOrder = -1; // Ensure it's behind other objects

                float posX = x * tileSize;
                float posY = y * tileSize;
                tile.transform.position = new Vector3(posX, posY, 0);

                // Adjust the scale to match the tileSize
                float scaleX = tileSize / spriteRenderer.sprite.bounds.size.x;
                float scaleY = tileSize / spriteRenderer.sprite.bounds.size.y;
                tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        // Center the background
        Vector2 worldCenter = worldGenerator.GetWorldCenter();
        backgroundParent.transform.position = new Vector3(worldCenter.x - worldSize.x / 2, worldCenter.y - worldSize.y / 2, 0);
    }
}