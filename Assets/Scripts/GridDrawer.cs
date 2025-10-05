using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;
    public Vector3 origin = new Vector3(-2f, -2f, 0f);
    public Color gridColor = Color.white;
    public float lineWidth = 0.05f;

    private void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
{
    GameObject gridParent = new GameObject("GridLines");

    Vector3 gridOrigin = new Vector3(-2.5f, -2.5f, 0f); 

    
    for (int x = 0; x <= width; x++)
    {
        Vector3 start = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y, 0);
        Vector3 end = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y + height * cellSize, 0);
        CreateLine(start, end, gridParent.transform);
    }

    
    for (int y = 0; y <= height; y++)
    {
        Vector3 start = new Vector3(gridOrigin.x, gridOrigin.y + y * cellSize, 0);
        Vector3 end = new Vector3(gridOrigin.x + width * cellSize, gridOrigin.y + y * cellSize, 0);
        CreateLine(start, end, gridParent.transform);
    }
}


    void CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = parent;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.sortingOrder = 0; 
    }
}
