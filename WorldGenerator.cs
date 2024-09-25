using UnityEngine;
using System.Collections.Generic;

namespace YourGameNamespace
{
    public class WorldGenerator : MonoBehaviour
    {
        public GameObject rockPrefab;
        public GameObject copperPrefab;
        public GameObject ironPrefab;
        public GameObject waterPrefab;
        public GameObject woodPrefab;
        public GameObject herbPrefab;
        public GameObject tinPrefab;
        public GameObject clayPrefab;
        public GameObject wheatPrefab;
        public GameObject carrotPrefab;
        public GameObject oilPrefab;
        public GameObject scrapPrefab;
        public GameObject titaniumPrefab;
        public GameObject uraniumPrefab;

        public BiomeManager biomeManager;

        public int worldSizeX = 200;
        public int worldSizeY = 200;
        public float cellSize = 1f;
        public int minVeinSize = 10;
        public int maxVeinSize = 100;
        public int numberOfVeins = 100;

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
            
            // Generate water border
            GenerateWaterBorder(centerTile);
            
            // Generate water and mountains
            GenerateWaterAndMountains(centerTile);
            
            // Generate resources
            GenerateResources(centerTile);
            
            Debug.Log($"Generated {worldGrid.Count} resources");
        }

        void GenerateResources(Vector2Int centerTile)
        {
            // Generate specific resources near borders
            GenerateBorderResources(centerTile, herbPrefab, BiomeType.Forest, 0.2f);
            GenerateBorderResources(centerTile, clayPrefab, BiomeType.Desert, 0.2f);

            // Generate other resources
            GenerateSpecificResourceVein(centerTile, woodPrefab, BiomeType.Forest);
            GenerateSpecificResourceVein(centerTile, herbPrefab, BiomeType.Forest);
            GenerateSpecificResourceVein(centerTile, rockPrefab, BiomeType.Forest);
            GenerateSpecificResourceVein(centerTile, copperPrefab, BiomeType.Desert);
            GenerateSpecificResourceVein(centerTile, ironPrefab, BiomeType.Desert);
            GenerateSpecificResourceVein(centerTile, tinPrefab, BiomeType.Desert);
            GenerateSpecificResourceVein(centerTile, wheatPrefab, BiomeType.Desert);
            GenerateSpecificResourceVein(centerTile, carrotPrefab, BiomeType.Desert);
            GenerateSpecificResourceVein(centerTile, oilPrefab, BiomeType.IndustrialWasteland);
            GenerateSpecificResourceVein(centerTile, scrapPrefab, BiomeType.IndustrialWasteland);
            GenerateSpecificResourceVein(centerTile, titaniumPrefab, BiomeType.AlienArea);
            GenerateSpecificResourceVein(centerTile, uraniumPrefab, BiomeType.AlienArea);

            // Generate additional random resource veins
            for (int i = 0; i < numberOfVeins; i++)
            {
                GenerateResourceVein(centerTile);
            }
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
                        // Place water tile (this should be handled by your terrain generation system)
                        // Do not add it to the worldGrid as it's not a minable resource
                    }
                }
            }
        }

        void GenerateWaterBorder(Vector2Int centerTile)
        {
            float forestRadius = Mathf.Sqrt(worldSizeX * worldSizeX + worldSizeY * worldSizeY) * 0.15f;
            Vector2 worldCenter = GetWorldCenter();

            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    Vector2Int worldPos = new Vector2Int(
                        centerTile.x - worldSizeX / 2 + x,
                        centerTile.y - worldSizeY / 2 + y
                    );

                    float distanceFromCenter = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), worldCenter / cellSize);

                    if (Mathf.Abs(distanceFromCenter - forestRadius / cellSize) < 2)
                    {
                        biomeManager.SetWaterAt(worldPos, true);
                        PlaceResource(worldPos, waterPrefab);
                    }
                }
            }
        }

        void GenerateBorderResources(Vector2Int centerTile, GameObject resourcePrefab, BiomeType biomeType, float spawnChance)
        {
            float forestRadius = Mathf.Sqrt(worldSizeX * worldSizeX + worldSizeY * worldSizeY) * 0.15f;
            Vector2 worldCenter = GetWorldCenter();

            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    Vector2Int worldPos = new Vector2Int(
                        centerTile.x - worldSizeX / 2 + x,
                        centerTile.y - worldSizeY / 2 + y
                    );

                    float distanceFromCenter = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), worldCenter / cellSize);

                    if (Mathf.Abs(distanceFromCenter - forestRadius / cellSize) < 4 &&
                        biomeManager.GetBiomeAt(worldPos.x, worldPos.y) == biomeType &&
                        !biomeManager.IsWaterAt(worldPos) &&
                        !worldGrid.ContainsKey(worldPos) &&
                        Random.value < spawnChance)
                    {
                        PlaceResource(worldPos, resourcePrefab);
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

            int veinSize = Random.Range(minVeinSize * 2, maxVeinSize * 2 + 1);            
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

        void GenerateSpecificResourceVein(Vector2Int centerTile, GameObject resourcePrefab, BiomeType biomeType)
        {
            int attempts = 0;
            const int maxAttempts = 1000;

            while (attempts < maxAttempts)
            {
                Vector2Int startPos = new Vector2Int(
                    Random.Range(centerTile.x - worldSizeX / 2, centerTile.x + worldSizeX / 2),
                    Random.Range(centerTile.y - worldSizeY / 2, centerTile.y + worldSizeY / 2)
                );

                if (!IsWaterOrMountainAt(startPos) && !worldGrid.ContainsKey(startPos) && biomeManager.GetBiomeAt(startPos.x, startPos.y) == biomeType)
                {
                    GenerateResourceVeinAt(startPos, resourcePrefab);
                    return;
                }

                attempts++;
            }

            Debug.LogWarning($"Failed to place {resourcePrefab.name} in {biomeType} biome after {maxAttempts} attempts.");
        }

        void GenerateResourceVeinAt(Vector2Int startPos, GameObject resourcePrefab)
        {
            int veinSize = Random.Range(minVeinSize, maxVeinSize + 1);
            List<Vector2Int> veinTiles = new List<Vector2Int> { startPos };

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
                    else if (random < 0.6f) return rockPrefab;
                    else if (random < 0.9f) return herbPrefab;
                    else return waterPrefab;
                case BiomeType.Desert:
                    if (random < 0.25f) return copperPrefab;
                    else if (random < 0.5f) return ironPrefab;
                    else if (random < 0.7f) return tinPrefab;
                    else if (random < 0.85f) return clayPrefab;
                    else if (random < 0.95f) return wheatPrefab;
                    else return carrotPrefab;
                case BiomeType.IndustrialWasteland:
                    if (random < 0.5f) return oilPrefab;
                    else return scrapPrefab;
                case BiomeType.AlienArea:
                    if (random < 0.5f) return titaniumPrefab;
                    else return uraniumPrefab;
                default:
                    return rockPrefab;
            }
        }

        public Vector2 GetForestCenter()
        {
            float forestRadius = Mathf.Sqrt(worldSizeX * worldSizeX + worldSizeY * worldSizeY) * 0.15f / 2f;
            Vector2 worldCenter = GetWorldCenter();
            return new Vector2(worldCenter.x, worldCenter.y + forestRadius);
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