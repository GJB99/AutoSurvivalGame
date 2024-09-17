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

        void Start()
        {
            worldGenerator = FindObjectOfType<WorldGenerator>();
            GenerateBiomes();
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
                        AddCollider(water);
                        water.tag = "Water"; // Ensure water is not tagged as "Resource"
                        Debug.Log($"Water generated at {x}, {y}");
                    }
                    else if (noiseValue > mountainThreshold)
                    {
                        GameObject mountain = Instantiate(mountainPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        AddCollider(mountain);
                        mountain.tag = "Mountain"; // Ensure mountain is not tagged as "Resource"
                        Debug.Log($"Mountain generated at {x}, {y}");
                    }
                }
            }
        }

        void AddCollider(GameObject obj)
        {
            if (obj.GetComponent<Collider2D>() == null)
            {
                obj.AddComponent<BoxCollider2D>();
            }
            obj.GetComponent<Collider2D>().isTrigger = false; // Ensure the collider is not a trigger
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