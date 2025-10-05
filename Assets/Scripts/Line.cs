using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public Node startNode = null;
    public Node endNode = null;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void UpdateRenderer(List<Vector2> worldPositions)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        
        lr.positionCount = worldPositions.Count;
        for (int i = 0; i < worldPositions.Count; i++)
            lr.SetPosition(i, new Vector3(worldPositions[i].x, worldPositions[i].y, 0));
    }

    public void FinalizeLine(Node a, Node b)
    {
        startNode = a;
        endNode = b;
        if (startNode != null) startNode.SetOwningLine(this);
        if (endNode != null) endNode.SetOwningLine(this);
        
        
        if (lr != null && a != null)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && a.colorId < gm.colors.Length)
            {
                lr.startColor = gm.colors[a.colorId];
                lr.endColor = gm.colors[a.colorId];
            }
        }
    }

    public void DestroyLine()
    {
        if (startNode != null) startNode.ClearOwningLine();
        if (endNode != null) endNode.ClearOwningLine();
        
        
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            foreach (var cell in cells)
            {
                gm.ClearOccupancy(cell);
            }
        }
        
        Destroy(gameObject);
    }
}