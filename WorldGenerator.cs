using UnityEngine;
using System.Collections.Generic;

namespace YourGameNamespace
{
    public class WorldGenerator : MonoBehaviour
    {
        public GameObject rockPrefab;
        public GameObject copperPrefab;
        public GameObject ironPrefab;
        public GameObject woodPrefab;
        public GameObject herbPrefab;
        public GameObject waterPrefab;
        public GameObject tinPrefab;
        public GameObject clayPrefab;
        public GameObject wheatPrefab;
        public GameObject carrotPrefab;
        public GameObject oilPrefab;
        public GameObject scrapPrefab;
        public GameObject titaniumPrefab;
        public GameObject uraniumPrefab;
        public GameObject lavaPrefab;

        public BiomeManager biomeManager;

        public int worldSizeX = 200;
        public int worldSizeY = 200;
        public float cellSize = 8f;
        public int minVeinSize = 10;
        public int maxVeinSize = 100;
        public int numberOfVeins = 50;

        private Dictionary<Vector2Int, GameObject> worldGrid = new Dictionary<Vector2Int, GameObject>();

        private const string WaterLayerName = "Water";
        private const string MountainLayerName = "Mountain";

        void Start()
        {
            biomeManager = FindObjectOfType<BiomeManager>();
            ClearExistingResources();
        }

        void ClearExistingResources()
        {
            GameObject[] existingResources = GameObject.FindGameObjectsWithTag("Resource");
            foreach (GameObject resource in existingResources)
            {
                Destroy(resource);
            }
        }

        public void GenerateWorld(Vector2 playerPosition)
        {
            Vector2Int centerTile = new Vector2Int(
                Mathf.RoundToInt(playerPosition.x / cellSize),
                Mathf.RoundToInt(playerPosition.y / cellSize)
            );
            
            // Generate water and mountains first
            GenerateWaterAndMountains(centerTile);
            
            // Then generate resource veins
            for (int i = 0; i < numberOfVeins; i++)
            {
                GenerateResourceVein(centerTile);
            }
            Debug.Log($"Generated {worldGrid.Count} resources");
        }

        private void GenerateWaterAndMountains(Vector2Int centerTile)
        {
            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    Vector2Int worldPos = new Vector2Int(
                        centerTile.x - worldSizeX / 2 + x,
                        centerTile.y - worldSizeY / 2 + y
                    );
                    
                    if (biomeManager.IsWaterAt(worldPos))
                    {
                        PlaceResource(worldPos, waterPrefab);
                    }
                    else if (biomeManager.IsMountainAt(worldPos))
                    {
                        PlaceResource(worldPos, rockPrefab); // Use rock prefab for mountains
                    }
                }
            }
        }

        void GenerateResourceVein(Vector2Int centerTile)
        {
            Vector2Int startPos;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                startPos = new Vector2Int(
                    Random.Range(centerTile.x - worldSizeX / 2, centerTile.x + worldSizeX / 2),
                    Random.Range(centerTile.y - worldSizeY / 2, centerTile.y + worldSizeY / 2)
                );
                attempts++;
                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("Failed to find a suitable position for resource vein after " + maxAttempts + " attempts.");
                    return;
                }
            } while (IsWaterOrMountainAt(startPos) || worldGrid.ContainsKey(startPos));

            int veinSize = Random.Range(minVeinSize, maxVeinSize + 1);
            GameObject resourcePrefab = ChooseRandomResource(startPos);

            List<Vector2Int> veinTiles = new List<Vector2Int>();
            veinTiles.Add(startPos);

            for (int i = 1; i < veinSize; i++)
            {
                Vector2Int lastTile = veinTiles[veinTiles.Count - 1];
                List<Vector2Int> possibleNextTiles = GetAdjacentTiles(lastTile);
                possibleNextTiles = possibleNextTiles.FindAll(tile => 
                    !worldGrid.ContainsKey(tile) && 
                    IsInWorldBounds(tile) && 
                    !IsWaterOrMountainAt(tile) &&
                    biomeManager.GetBiomeAt(tile.x, tile.y) == biomeManager.GetBiomeAt(startPos.x, startPos.y));

                if (possibleNextTiles.Count == 0) break;

                Vector2Int nextTile = possibleNextTiles[Random.Range(0, possibleNextTiles.Count)];
                veinTiles.Add(nextTile);
            }

            foreach (Vector2Int tile in veinTiles)
            {
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

        GameObject ChooseRandomResource(Vector2Int position)
        {
            BiomeType biomeType = biomeManager.GetBiomeAt(position.x, position.y);
            float random = Random.value;

            switch (biomeType)
            {
                case BiomeType.Forest:
                    if (random < 0.3f) return woodPrefab;
                    else if (random < 0.5f) return rockPrefab;
                    else if (random < 0.8f) return herbPrefab;
                    else return waterPrefab;
                case BiomeType.Desert:
                    if (random < 0.3f) return copperPrefab;
                    else if (random < 0.5f) return ironPrefab;
                    else if (random < 0.7f) return tinPrefab;
                    else if (random < 0.8f) return clayPrefab;
                    else if (random < 0.9f) return wheatPrefab;
                    else return carrotPrefab;
                case BiomeType.IndustrialWasteland:
                    if (random < 0.5f) return oilPrefab;
                    else return scrapPrefab;
                case BiomeType.AlienArea:
                    if (random < 0.5f) return titaniumPrefab;
                    else return uraniumPrefab;
                case BiomeType.Lava:
                    return lavaPrefab;
                default:
                    return rockPrefab;
            }
        }

        public Vector2 GetForestCenter()
        {
            float forestRadius = Mathf.Sqrt(worldSizeX * worldSizeX + worldSizeY * worldSizeY) * 0.15f / 2f;
            Vector2 worldCenter = GetWorldCenter();
            return new Vector2(worldCenter.x, worldCenter.y + forestRadius / 2f);
        }

        void PlaceResource(Vector2Int tile, GameObject prefab)
        {
            Vector3 worldPosition = new Vector3(tile.x * cellSize, tile.y * cellSize, -0.1f);
            GameObject resource = Instantiate(prefab, worldPosition, Quaternion.identity);
            resource.transform.SetParent(transform);
            resource.transform.localScale = new Vector3(0.2f*cellSize, 0.2f*cellSize, 1f);
            resource.tag = "Resource";
            worldGrid[tile] = resource;
        }

        public GameObject GetResourceAt(Vector2Int position)
        {
            if (worldGrid.TryGetValue(position, out GameObject resource))
            {
                return resource;
            }
            return null;
        }

        public bool IsWaterOrMountainAt(Vector2Int position)
        {
            return biomeManager.IsWaterAt(position) || biomeManager.IsMountainAt(position);
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
}