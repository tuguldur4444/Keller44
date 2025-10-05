using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    public GameObject linePrefab;
    public GameManager gameManager;
    public float lineWidth = 0.3f;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();
    private List<Vector2Int> gridCells = new List<Vector2Int>();
    private Node startNode;
    private bool isDrawing = false; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartLine();
        }
        else if (Input.GetMouseButton(0) && currentLine != null)
        {
            UpdateLine();
        }
        else if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            EndLine();
        }
    }

    void StartLine()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = gameManager.WorldToGrid(mouseWorldPos);
        Node nodeAtCell = gameManager.GetNodeAtCell(cell);

        
        if (nodeAtCell == null)
        {
            Debug.Log("Cannot start line — not on a node");
            return;
        }

        
        if (nodeAtCell.owningLine != null)
        {
            Debug.Log($"Node already connected — removing existing line for color {nodeAtCell.colorId}");
            gameManager.RemoveLine(nodeAtCell.owningLine);
        }

        
        startNode = nodeAtCell;
        GameObject newLineObj = Instantiate(linePrefab);
        newLineObj.name = $"Line_{startNode.colorId}";
        currentLine = newLineObj.GetComponent<LineRenderer>();

        
        currentLine.positionCount = 0;
        currentLine.startColor = gameManager.colors[startNode.colorId];
        currentLine.endColor = gameManager.colors[startNode.colorId];
        currentLine.widthMultiplier = 0.25f;
        currentLine.useWorldSpace = true;

        points.Clear();
        gridCells.Clear();

        
        Vector2 startWorld = gameManager.GridToWorld(cell);
        gridCells.Add(cell);
        points.Add(startWorld);
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, startWorld);

        isDrawing = true; 
        Debug.Log($"Started new line from node color {startNode.colorId} at {cell}");
    }

    public void ClearCurrentLine()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
        isDrawing = false; 
        startNode = null;
        points.Clear();
        gridCells.Clear();
    }

    void UpdateLine()
    {
        if (currentLine == null) return;

        Vector3 mousePos = GetMouseWorldPosition();
        Vector2Int currentCell = gameManager.WorldToGrid(mousePos);

        
        if (!gameManager.IsCellInsideGrid(currentCell))
            return;

        
        if (gridCells.Count > 0 && gridCells[gridCells.Count - 1] == currentCell)
            return;

        
        if (gridCells.Count > 1 && gridCells[gridCells.Count - 2] == currentCell)
        {
            gridCells.RemoveAt(gridCells.Count - 1);
            points.RemoveAt(points.Count - 1);
            currentLine.positionCount = points.Count;
            currentLine.SetPositions(points.ToArray());
            return;
        }

        
        if (gridCells.Count > 0)
        {
            Vector2Int prev = gridCells[gridCells.Count - 1];
            Vector2Int diff = currentCell - prev;

            
            bool isValidPlusMove =
                (Mathf.Abs(diff.x) == 1 && diff.y == 0) ||
                (Mathf.Abs(diff.y) == 1 && diff.x == 0);

            if (!isValidPlusMove)
            {
                Debug.Log("Invalid diagonal move — destroying line");
                DestroyCurrentLine();
                return;
            }
        }

        
        if (gridCells.Contains(currentCell))
        {
            Debug.Log("Self-overlap detected — destroying line");
            DestroyCurrentLine();
            return;
        }

        
        Node nodeAtCell = gameManager.GetNodeAtCell(currentCell);
        if (nodeAtCell != null)
        {
            if (nodeAtCell == startNode)
            {
                Debug.Log("Overlapped start node — destroying line");
                DestroyCurrentLine();
                return;
            }

            if (startNode == null || nodeAtCell.colorId != startNode.colorId)
            {
                Debug.Log($"Overlapped invalid node (different color) — destroying line");
                DestroyCurrentLine();
                return;
            }

            if (startNode != null && nodeAtCell != startNode && nodeAtCell.colorId == startNode.colorId)
            {
                Debug.Log("Reached matching end node — finalizing line");

                
                gridCells.Add(currentCell);
                points.Add(gameManager.GridToWorld(currentCell));
                currentLine.positionCount = points.Count;
                currentLine.SetPositions(points.ToArray());

                
                EndLine();
                return;
            }
        }

        
        Line existingLine = gameManager.GetLineAtCell(currentCell);
        Line myLineComp = currentLine.gameObject.GetComponent<Line>();
        if (existingLine != null && existingLine != myLineComp)
        {
            if (startNode != null && existingLine.startNode != null &&
                startNode.colorId != existingLine.startNode.colorId)
            {
                Debug.Log($"Overlap with different color line at {currentCell} — removing it");
                gameManager.RemoveLine(existingLine);
            }
            else
            {
                Debug.Log("Overlap with same-color existing line — destroying current line");
                DestroyCurrentLine();
                return;
            }
        }

        
        gridCells.Add(currentCell);
        Vector3 worldPos = gameManager.GridToWorld(currentCell);
        points.Add(worldPos);

        currentLine.positionCount = points.Count;
        currentLine.SetPositions(points.ToArray());

        if (startNode != null)
        {
            Color c = gameManager.colors[startNode.colorId];
            currentLine.startColor = c;
            currentLine.endColor = c;
        }
    }

    void DestroyCurrentLine()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
        }

        currentLine = null;
        points.Clear();
        gridCells.Clear();
        startNode = null;
        isDrawing = false; 
    }

    void CheckAndRemoveDifferentColorOverlaps(Vector2Int currentCell)
    {
        
        Line existingLine = gameManager.GetLineAtCell(currentCell);
        if (existingLine != null && existingLine != currentLine.GetComponent<Line>())
        {
            
            if (startNode != null && existingLine.startNode != null &&
                startNode.colorId != existingLine.startNode.colorId)
            {
                Debug.Log($"Overlap detected with different color line at {currentCell}, removing existing line");
                gameManager.RemoveLine(existingLine);
            }
            else
            {
                Debug.Log($"Cannot overlap with same color line at {currentCell}");
                
                
            }
        }
    }

    void EndLine()
    {
        if (currentLine == null) return;

        Debug.Log($"Line completed with {gridCells.Count} points");

        if (gridCells.Count >= 2)
        {
            Node endNode = gameManager.GetNodeAtCell(gridCells[gridCells.Count - 1]);

            
            if (startNode != null && endNode != null && startNode != endNode && startNode.colorId == endNode.colorId)
            {
                
                Line lineComponent = currentLine.gameObject.GetComponent<Line>();
                if (lineComponent == null)
                    lineComponent = currentLine.gameObject.AddComponent<Line>();

                lineComponent.cells = new List<Vector2Int>(gridCells);
                lineComponent.FinalizeLine(startNode, endNode);
                gameManager.RegisterFinalLine(lineComponent);

                Debug.Log($"Connected {startNode.colorId} colored nodes");
            }
            else
            {
                Debug.Log("Invalid connection - destroying line");
                Destroy(currentLine.gameObject);
            }
        }
        else
        {
            Debug.Log("Line too short - destroying");
            Destroy(currentLine.gameObject);
        }

        currentLine = null;
        points.Clear();
        gridCells.Clear();
        startNode = null;
        isDrawing = false; 
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}