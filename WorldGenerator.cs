using UnityEngine;
using System.Collections.Generic;

namespace YourGameNamespace
{
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
        private BiomeManager biomeManager;

        void Start()
        {
            biomeManager = FindObjectOfType<BiomeManager>();
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
            Vector2Int startPos;
            do
            {
                startPos = new Vector2Int(Random.Range(0, worldSizeX), Random.Range(0, worldSizeY));
            } while (biomeManager.IsWaterAt(startPos) || biomeManager.IsMountainAt(startPos));

            int veinSize = Random.Range(minVeinSize, maxVeinSize + 1);
            GameObject resourcePrefab = ChooseRandomResource();

            List<Vector2Int> veinTiles = new List<Vector2Int>();
            veinTiles.Add(startPos);

            for (int i = 1; i < veinSize; i++)
            {
                Vector2Int lastTile = veinTiles[veinTiles.Count - 1];
                List<Vector2Int> possibleNextTiles = GetAdjacentTiles(lastTile);
                possibleNextTiles = possibleNextTiles.FindAll(tile => !worldGrid.ContainsKey(tile) && IsInWorldBounds(tile) && !biomeManager.IsWaterAt(tile) && !biomeManager.IsMountainAt(tile));

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
            resource.transform.localScale = new Vector3(0.225f*cellSize, 0.225f*cellSize, 1f); // Adjust the scale to match the cell size
            worldGrid[tile] = resource;
        }

        public GameObject GetResourceAt(Vector2Int position)
        {
            worldGrid.TryGetValue(position, out GameObject resource);
            return resource;
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