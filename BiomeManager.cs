using UnityEngine;

namespace YourGameNamespace
{
    public enum BiomeType
    {
        Forest,
        Desert,
        IndustrialWasteland,
        AlienArea,
        Lava
    }

    public class BiomeManager : MonoBehaviour
    {
        public int worldSizeX = 400;
        public int worldSizeY = 400;
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
            GenerateBiomes(new Vector2Int(worldSizeX / 2, worldSizeY / 2)); // Generate biomes centered on the world
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
                default:
                    return forestPrefab;
            }
        }

        public BiomeType GetBiomeAt(int x, int y)
        {
            if (x >= 0 && x < worldSizeX && y >= 0 && y < worldSizeY)
            {
                return biomeMap[x, y];
            }
            return BiomeType.Forest; // Default to Forest if out of bounds
        }

       void InstantiateBiomeTile(int x, int y, BiomeType biomeType)
        {
            GameObject prefab = GetBiomePrefab(biomeType);
            Vector3 position = new Vector3(x, y, 0);
            GameObject biomeTile = Instantiate(prefab, position, Quaternion.identity);
            biomeTile.transform.SetParent(transform);

            if (biomeType == BiomeType.Forest && IsWaterTile(x, y))
            {
                biomeTile.layer = waterLayer;
            }
            else if (biomeType == BiomeType.Desert && IsMountainTile(x, y))
            {
                biomeTile.layer = mountainLayer;
            }
        }

        bool IsWaterTile(int x, int y)
        {
            // Implement your water tile logic here
            return false; // Placeholder
        }

        bool IsMountainTile(int x, int y)
        {
            // Implement your mountain tile logic here
            return false; // Placeholder
        }

        public bool IsWaterAt(Vector2Int position)
        {
            return IsWaterTile(position.x, position.y);
        }

        public bool IsMountainAt(Vector2Int position)
        {
            return IsMountainTile(position.x, position.y);
        }
    }
}