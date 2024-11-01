using UnityEngine;

namespace YourGameNamespace
{
    public enum BiomeType
    {
        Forest,
        Desert,
        IndustrialWasteland,
        AlienArea,
        Lava,
        Water
    }

    public class BiomeManager : MonoBehaviour
    {
        public int worldSizeX = 200;
        public int worldSizeY = 200;
        public float noiseScale = 20f;

        public GameObject forestPrefab;
        public GameObject desertPrefab;
        public GameObject industrialWastelandPrefab;
        public GameObject alienAreaPrefab;
        public GameObject waterPrefab;
        public GameObject mountainPrefab;
        public GameObject lavaPrefab;

        private BiomeType[,] biomeMap;

        private const string WaterLayerName = "Water";
        private const string MountainLayerName = "Mountain";

        private int waterLayer;
        private int mountainLayer;

        void Awake()
        {
            InitializeLayers();
            InitializeBiomeMap();
        }

        void InitializeLayers()
        {
            waterLayer = LayerMask.NameToLayer(WaterLayerName);
            mountainLayer = LayerMask.NameToLayer(MountainLayerName);

            if (waterLayer == -1 || mountainLayer == -1)
            {
                Debug.LogError("Water or Mountain layer not found. Please create these layers in Edit > Project Settings > Tags and Layers.");
            }
        }

        public void InitializeBiomeMap()
        {
            if (biomeMap == null)
            {
                biomeMap = new BiomeType[worldSizeX, worldSizeY];
                Vector2Int centerTile = new Vector2Int(worldSizeX / 2, worldSizeY / 2);
                GenerateBiomes(centerTile);
            }
        }

        private BiomeType DetermineBiomeType(int x, int y)
        {
            float noiseValue = Mathf.PerlinNoise(x / noiseScale, y / noiseScale);
            
            if (noiseValue < 0.2f) return BiomeType.Forest;
            if (noiseValue < 0.4f) return BiomeType.Desert;
            if (noiseValue < 0.6f) return BiomeType.IndustrialWasteland;
            if (noiseValue < 0.8f) return BiomeType.AlienArea;
            return BiomeType.Lava;
        }

        public void GenerateBiomes(Vector2Int centerTile)
        {
            biomeMap = new BiomeType[worldSizeX, worldSizeY];

            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    int worldX = centerTile.x - worldSizeX / 2 + x;
                    int worldY = centerTile.y - worldSizeY / 2 + y;
                    float noiseValue = Mathf.PerlinNoise(worldX / noiseScale, worldY / noiseScale);
                    biomeMap[x, y] = AssignBiome(worldX, worldY, noiseValue);
                    InstantiateBiomeTile(worldX, worldY, biomeMap[x, y]);
                }
            }
        }

        BiomeType AssignBiome(int x, int y, float noiseValue)
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

GameObject GetBiomePrefab(BiomeType biomeType)
{
    switch (biomeType)
    {
        case BiomeType.Forest:
            return forestPrefab;
        case BiomeType.Desert:
            return desertPrefab;
        case BiomeType.IndustrialWasteland:
            return industrialWastelandPrefab;
        case BiomeType.AlienArea:
            return alienAreaPrefab;
        case BiomeType.Lava:
            return lavaPrefab;
        case BiomeType.Water:
            return waterPrefab;
        default:
            return forestPrefab;
    }
}

        public BiomeType GetBiomeAt(int x, int y)
        {
            if (biomeMap == null)
            {
                Debug.LogError("BiomeMap is not initialized!");
                return BiomeType.Forest; // Default to Forest if map is not initialized
            }
            
            x = Mathf.Clamp(x, 0, worldSizeX - 1);
            y = Mathf.Clamp(y, 0, worldSizeY - 1);
            return biomeMap[x, y];
        }

void InstantiateBiomeTile(int x, int y, BiomeType biomeType)
{
    GameObject prefab = GetBiomePrefab(biomeType);
    Vector3 position = new Vector3(x, y, 0);
    GameObject biomeTile = Instantiate(prefab, position, Quaternion.identity);
    biomeTile.transform.SetParent(transform);

    if (IsWaterTile(x, y))
    {
        GameObject waterTile = Instantiate(waterPrefab, position, Quaternion.identity);
        waterTile.transform.SetParent(transform);
        waterTile.layer = waterLayer;
    }
    else if (biomeType == BiomeType.Desert && IsMountainTile(x, y))
    {
        GameObject mountainTile = Instantiate(mountainPrefab, position, Quaternion.identity);
        mountainTile.transform.SetParent(transform);
        mountainTile.layer = mountainLayer;
        // Destroy the original desert tile since we're replacing it with a mountain
        Destroy(biomeTile);
    }
}

bool IsWaterTile(int x, int y)
{
    if (x < 0 || x >= worldSizeX || y < 0 || y >= worldSizeY)
    {
        return false;
    }

    // Check if current tile is Forest
    if (biomeMap[x, y] != BiomeType.Forest)
    {
        return false;
    }

    // Check in a wider radius for Desert tiles
    for (int i = -2; i <= 2; i++)
    {
        for (int j = -2; j <= 2; j++)
        {
            if (i == 0 && j == 0) continue;
            
            int checkX = x + i;
            int checkY = y + j;

            if (checkX >= 0 && checkX < worldSizeX && checkY >= 0 && checkY < worldSizeY)
            {
                if (biomeMap[checkX, checkY] == BiomeType.Desert)
                {
                    return true; // This is a border tile
                }
            }
        }
    }
    return false;
}

bool IsMountainTile(int x, int y)
{
    if (x < 0 || x >= worldSizeX || y < 0 || y >= worldSizeY)
    {
        return false;
    }

    // Check if current tile is Desert
    if (biomeMap[x, y] != BiomeType.Desert)
    {
        return false;
    }

    // Check in a wider radius for Industrial Wasteland tiles
    for (int i = -2; i <= 2; i++)
    {
        for (int j = -2; j <= 2; j++)
        {
            if (i == 0 && j == 0) continue;
            
            int checkX = x + i;
            int checkY = y + j;

            if (checkX >= 0 && checkX < worldSizeX && checkY >= 0 && checkY < worldSizeY)
            {
                if (biomeMap[checkX, checkY] == BiomeType.IndustrialWasteland)
                {
                    return true; // This is a border tile
                }
            }
        }
    }
    return false;
}


        public bool IsWaterAt(Vector2Int position)
        {
            return IsWaterTile(position.x, position.y);
        }

        public bool IsMountainAt(Vector2Int position)
        {
            return IsMountainTile(position.x, position.y);
        }

        public void SetWaterAt(Vector2Int position, bool isWater)
        {
            if (position.x >= 0 && position.x < worldSizeX && position.y >= 0 && position.y < worldSizeY)
            {
                if (isWater)
                {
                    biomeMap[position.x, position.y] = BiomeType.Water;
                    InstantiateBiomeTile(position.x, position.y, BiomeType.Water);
                }
            }
        }
    }
}