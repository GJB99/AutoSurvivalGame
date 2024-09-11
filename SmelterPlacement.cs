using UnityEngine;

public class SmelterPlacement : MonoBehaviour
{
    public GameObject smelterPrefab;
    private PlayerInventory playerInventory;
    private GameObject placementPreview;

    void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && playerInventory.GetItemCount("Smelter") > 0)
        {
            PlaceSmelter();
        }

        UpdatePlacementPreview();
    }

    void UpdatePlacementPreview()
    {
        if (playerInventory.GetItemCount("Smelter") > 0)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            Vector3 snappedPosition = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0);

            if (placementPreview == null)
            {
                placementPreview = Instantiate(smelterPrefab, snappedPosition, Quaternion.identity);
                placementPreview.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
                Destroy(placementPreview.GetComponent<Collider2D>());
            }
            else
            {
                placementPreview.transform.position = snappedPosition;
            }
        }
        else if (placementPreview != null)
        {
            Destroy(placementPreview);
        }
    }

    void PlaceSmelter()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 snappedPosition = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0);

        if (!Physics2D.OverlapCircle(snappedPosition, 0.4f))
        {
            Instantiate(smelterPrefab, snappedPosition, Quaternion.identity);
            playerInventory.RemoveItems("Smelter", 1);
        }
    }
}