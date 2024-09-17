using UnityEngine;

namespace YourGameNamespace
{
    public class BiomeManager : MonoBehaviour
    {
        public int worldSizeX = 200;
        public int worldSizeY = 200;
        public float noiseScale = 20f;
        public float waterThreshold = 0.3f;
        public float mountainThreshold = 0.7f;

        public GameObject waterPrefab;
        public GameObject mountainPrefab;

        private WorldGenerator worldGenerator;
        private float[,] noiseMap;

        private const string WaterLayerName = "Water";
        private const string MountainLayerName = "Mountain";

        void Start()
        {
            worldGenerator = FindObjectOfType<WorldGenerator>();
            CreateLayers();
            GenerateBiomes();
        }

        void CreateLayers()
        {
            CreateLayerIfNotExists(WaterLayerName);
            CreateLayerIfNotExists(MountainLayerName);
        }

        void CreateLayerIfNotExists(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist. Please create it in the Unity Editor.");
            }
        }

        void GenerateBiomes()
        {
            noiseMap = new float[worldSizeX, worldSizeY];
            for (int x = 0; x < worldSizeX; x++)
            {
                for (int y = 0; y < worldSizeY; y++)
                {
                    float noiseValue = Mathf.PerlinNoise(x / noiseScale, y / noiseScale);
                    noiseMap[x, y] = noiseValue;
                    Vector2Int position = new Vector2Int(x, y);

                    if (noiseValue < waterThreshold)
                    {
                        GameObject water = Instantiate(waterPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        water.transform.SetParent(transform);
                        AddCollider(water);
                        water.layer = LayerMask.NameToLayer(WaterLayerName);
                        Debug.Log($"Water generated at {x}, {y}");
                    }
                    else if (noiseValue > mountainThreshold)
                    {
                        GameObject mountain = Instantiate(mountainPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        mountain.transform.SetParent(transform);
                        AddCollider(mountain);
                        mountain.layer = LayerMask.NameToLayer(MountainLayerName);
                        Debug.Log($"Mountain generated at {x}, {y}");
                    }
                }
            }
        }

    void AddCollider(GameObject obj)
    {
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = false;
        collider.radius = 0.5f; // Adjust this value to match half your cell size
    }

    public bool IsWaterAt(Vector2Int position)
        {
            if (position.x >= 0 && position.x < worldSizeX && position.y >= 0 && position.y < worldSizeY)
            {
                return noiseMap[position.x, position.y] < waterThreshold;
            }
            return false;
        }

    public bool IsMountainAt(Vector2Int position)
        {
            if (position.x >= 0 && position.x < worldSizeX && position.y >= 0 && position.y < worldSizeY)
            {
                return noiseMap[position.x, position.y] > mountainThreshold;
            }
            return false;
        }
    }
}