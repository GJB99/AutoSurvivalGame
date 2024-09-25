using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class Resource : MonoBehaviour
{
    public string resourceName;
    public int initialQuantity;
    public int currentQuantity;
    public float miningRange = 3f;
    public float cellSize = 1f;
    public bool isFood = false;

    void Start()
    {
        SetResourceName();
        initialQuantity = Random.Range(10, 51); // Adjust range as needed
        currentQuantity = initialQuantity;
    }

    public bool IsInMiningRange(Vector3 playerPosition)
    {
        // Use the center of the cell for distance calculation
        Vector3 resourceCenter = transform.position + new Vector3(cellSize / 4, cellSize / 4, 0);
        float distance = Vector2.Distance(resourceCenter, playerPosition);
        
        // Adjust the range check for better accuracy
        return distance <= miningRange + cellSize / 2f;
    }

    public void Mine()
    {
        if (currentQuantity > 0 && gameObject.tag != "Water" && gameObject.tag != "Mountain")
        {
            currentQuantity--;
            if (currentQuantity <= 0)
            {
                StartCoroutine(DestroyResource());
            }
        }
    }

    void SetResourceName()
    {
        resourceName = gameObject.name.Replace("(Clone)", "").Trim();
    }

    public bool IsDepletedResource()
    {
        return currentQuantity <= 0;
    }

    IEnumerator DestroyResource()
    {
        yield return new WaitForSeconds(0.2f); // Short delay before destroying
        // Perform any necessary cleanup here
        Destroy(gameObject);
    }

    // Remove or comment out the OnMouseDown, OnTriggerEnter2D, OnCollisionEnter2D, and OnTriggerExit2D methods
    // as they're not necessary for the current implementation and contain references to the non-existent player variable
}