using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public GameObject rockPrefab;
    public GameObject copperPrefab;
    public GameObject ironPrefab;

    public int worldSizeX = 200;
    public int worldSizeY = 200;
    public float cellSize = 1f;
    public int minVeinSize = 10;
    public int maxVeinSize = 100;
    public int numberOfVeins = 50;

    private Dictionary<Vector2Int, GameObject> worldGrid = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        ClearExistingResources();
        GenerateWorld();
    }

    void ClearExistingResources()
    {
        GameObject[] existingResources = GameObject.FindGameObjectsWithTag("Resource");
        foreach (GameObject resource in existingResources)
        {
            Destroy(resource);
        }
    }

    public void GenerateWorld()
    {
        for (int i = 0; i < numberOfVeins; i++)
        {
            GenerateResourceVein();
        }
    }

    void GenerateResourceVein()
    {
        // Get a starting position for the vein within the bounds of the world
        Vector2Int startPos = new Vector2Int(Random.Range(0, worldSizeX), Random.Range(0, worldSizeY));

        // Set the size of the vein
        int veinSize = Random.Range(minVeinSize, maxVeinSize + 1);

        // Choose a random resource prefab to spawn (iron, copper, rock, etc.)
        GameObject resourcePrefab = ChooseRandomResource();

        // List to keep track of the tiles where resources will be placed
        List<Vector2Int> veinTiles = new List<Vector2Int>();
        veinTiles.Add(startPos);

        // Loop through and generate the vein of resources
        for (int i = 1; i < veinSize; i++)
        {
            // Get the last placed tile in the vein
            Vector2Int lastTile = veinTiles[veinTiles.Count - 1];

            // Get adjacent tiles around the last tile, filter out invalid ones
            List<Vector2Int> possibleNextTiles = GetAdjacentTiles(lastTile);
            possibleNextTiles = possibleNextTiles.FindAll(tile => !worldGrid.ContainsKey(tile) && IsInWorldBounds(tile));

            // If no valid adjacent tiles are found, stop generating the vein
            if (possibleNextTiles.Count == 0) break;

            // Select a random valid adjacent tile for the next resource in the vein
            Vector2Int nextTile = possibleNextTiles[Random.Range(0, possibleNextTiles.Count)];
            veinTiles.Add(nextTile);
        }

        // Iterate over the valid vein tiles and place resources
        foreach (Vector2Int tile in veinTiles)
        {
            // Use a specific method for correct placement of the resource
            PlaceResource(tile, resourcePrefab);
        }
    }

    List<Vector2Int> GetAdjacentTiles(Vector2Int tile)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(tile.x + 1, tile.y),
            new Vector2Int(tile.x - 1, tile.y),
            new Vector2Int(tile.x, tile.y + 1),
            new Vector2Int(tile.x, tile.y - 1)
        };
    }

    bool IsInWorldBounds(Vector2Int tile)
    {
        return tile.x >= 0 && tile.x < worldSizeX && tile.y >= 0 && tile.y < worldSizeY;
    }

    GameObject ChooseRandomResource()
    {
        float random = Random.value;
        if (random < 0.4f)
            return rockPrefab;
        else if (random < 0.7f)
            return ironPrefab;
        else
            return copperPrefab;
    }

    void PlaceResource(Vector2Int tile, GameObject prefab)
    {
        Vector3 worldPosition = new Vector3(tile.x * cellSize, tile.y * cellSize, -1f);
        GameObject resource = Instantiate(prefab, worldPosition, Quaternion.identity);
        resource.transform.SetParent(transform);

        // Adjust the scale to fit the cellSize exactly
        SpriteRenderer spriteRenderer = resource.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            float scaleX = cellSize / spriteRenderer.sprite.bounds.size.x;
            float scaleY = cellSize / spriteRenderer.sprite.bounds.size.y;
            resource.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Adjust the collider size
        CircleCollider2D circleCollider = resource.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.radius = cellSize / 2f;
        }

        worldGrid[tile] = resource;
        Debug.Log($"Placed {prefab.name} at position {worldPosition} with scale {resource.transform.localScale}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        foreach (var kvp in worldGrid)
        {
            Vector3 worldPos = new Vector3(kvp.Key.x * cellSize, kvp.Key.y * cellSize, 0);
            Gizmos.color = kvp.Value.name.Contains("Rock") ? Color.gray : Color.red;
            Gizmos.DrawSphere(worldPos, 0.2f);
        }
    }

    public GameObject GetResourceAt(Vector2Int position)
    {
    if (worldGrid.TryGetValue(position, out GameObject resource))
    {
        return resource;
    }
    return null;
    }

    public Vector2 GetWorldCenter()
    {
        return new Vector2(worldSizeX * cellSize / 2f, worldSizeY * cellSize / 2f);
    }

    public Vector2 GetWorldSize()
    {
        return new Vector2(worldSizeX * cellSize, worldSizeY * cellSize);
    }
}