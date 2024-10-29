using UnityEngine;

public class GridPlacementSystem : MonoBehaviour
{
    public float gridSize = 1f;
    private Vector2 gridOffset = Vector2.zero;

    public Vector2 SnapToGrid(Vector2 position)
    {
        float x = Mathf.Round((position.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        float y = Mathf.Round((position.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
        return new Vector2(x, y);
    }

    public bool IsPositionOccupied(Vector2 position)
    {
        Vector2 snappedPos = SnapToGrid(position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(snappedPos, gridSize * 0.4f);
        return colliders.Length > 0;
    }

    public void ShowGridPreview(Vector2 position)
    {
        // You can implement grid visualization here if desired
        // For example, showing a temporary grid square where the item will be placed
    }
}