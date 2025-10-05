using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Grid")]
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero;

    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject linePrefab;

    [Header("Colors (set in inspector)")]
    public Color[] colors;

    
    private GameObject winPanel;
    private Button nextButton;
    private Button replayButton;
    private Text roundText;
    private Text winText;

    private Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    private Dictionary<Vector2Int, Line> occupancy = new Dictionary<Vector2Int, Line>();
    private List<Line> activeFinalLines = new List<Line>();

    [Header("Gameplay")]
    public int nodesPerColor = 2;
    public LineDrawer lineDrawer;

    [Header("Level System")]
    public int currentRound = 1;
    public int maxRounds = 5;
    private List<Vector2Int>[] roundNodePositions;
    private bool gameWon = false;

    void Start()
    {
        Instance = this;

        Debug.Log("=== GAME MANAGER START ===");
        SetupCamera();
        CreateUI(); 
        SetupUI();  

        if (colors == null || colors.Length == 0)
        {
            colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
        }

        if (lineDrawer == null)
        {
            GameObject ldObj = new GameObject("LineDrawer");
            lineDrawer = ldObj.AddComponent<LineDrawer>();
            lineDrawer.linePrefab = linePrefab;
            lineDrawer.gameManager = this;
        }

        
        PreGenerateRoundNodePositions();

        
        StartRound(currentRound);
    }

    void CreateUI()
    {
        Debug.Log("Creating UI automatically...");

        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Created new Canvas");
        }

        
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }

        
        winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(canvas.transform);
        winPanel.AddComponent<CanvasRenderer>();
        Image panelImage = winPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); 

        
        RectTransform panelRect = winPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        winPanel.SetActive(false);

        
        winText = CreateText("WinText", winPanel.transform, "Round Complete!", 36, TextAnchor.MiddleCenter, new Vector2(0, 50));

        
        GameObject nextButtonObj = CreateButton("NextButton", winPanel.transform, "Next Round", new Vector2(0, -30));
        nextButton = nextButtonObj.GetComponent<Button>();

        
        GameObject replayButtonObj = CreateButton("ReplayButton", canvas.transform, "Restart Round", new Vector2(0, -100));
        replayButton = replayButtonObj.GetComponent<Button>();

        
        RectTransform replayRect = replayButtonObj.GetComponent<RectTransform>();
        replayRect.anchorMin = new Vector2(0.5f, 0);
        replayRect.anchorMax = new Vector2(0.5f, 0);
        replayRect.anchoredPosition = new Vector2(0, 80);

        
        roundText = CreateText("RoundText", canvas.transform, $"Round: {currentRound}/{maxRounds}", 24, TextAnchor.UpperLeft, new Vector2(80, -50));

        
        RectTransform roundRect = roundText.GetComponent<RectTransform>();
        roundRect.anchorMin = new Vector2(0, 1);
        roundRect.anchorMax = new Vector2(0, 1);
        roundRect.anchoredPosition = new Vector2(100, -50);

        Debug.Log("UI creation complete!");

    }


    GameObject CreateButton(string name, Transform parent, string buttonText, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);

        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.6f, 1f); 

        
        Button button = buttonObj.AddComponent<Button>();

        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.6f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.8f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.9f, 1f);
        colors.selectedColor = new Color(0.3f, 0.3f, 0.7f, 1f);
        button.colors = colors;

        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;

        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(160, 50);
        buttonRect.anchoredPosition = position;

        Debug.Log($"Button '{name}' created at position {position}");
        return buttonObj;
    }
    void TestButtonClicks()
    {
        Debug.Log("=== TESTING BUTTON CLICKS ===");

        if (nextButton != null)
        {
            Debug.Log("Next button exists - adding test listener");
            nextButton.onClick.AddListener(() => Debug.Log("NEXT BUTTON CLICKED!"));
        }
        else
        {
            Debug.LogError("Next button is null!");
        }

        if (replayButton != null)
        {
            Debug.Log("Replay button exists - adding test listener");
            replayButton.onClick.AddListener(() => Debug.Log("REPLAY BUTTON CLICKED!"));
        }
        else
        {
            Debug.LogError("Replay button is null!");
        }
    }

    Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = alignment;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);
        rect.anchoredPosition = position;

        return textComp;
    }

    void SetupUI()
    {
        Debug.Log("Setting up UI button listeners...");

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextRound);
            nextButton.gameObject.SetActive(false);
            Debug.Log("Next button listener added");
        }

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(RestartRound);
            Debug.Log("Replay button listener added");
        }

        UpdateRoundText();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("=== MANUAL WIN TRIGGERED ===");
            ShowWinUI();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("=== MANUAL WIN CHECK ===");
            CheckWinCondition();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("=== CHECK NODE CONNECTIONS ===");
            DebugWinCondition();
        }

        
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"=== NODE COUNT: {nodes.Count} ===");
            if (nodes.Count != 8)
            {
                Debug.LogError($"WRONG NODE COUNT! Expected 8, got {nodes.Count}");
            }
        }
    }

    void PreGenerateRoundNodePositions()
    {
        roundNodePositions = new List<Vector2Int>[maxRounds];

        
        roundNodePositions[0] = new List<Vector2Int>
    {
        new Vector2Int(1, 4),  
        new Vector2Int(2, 2),  
        new Vector2Int(3, 3),  
        new Vector2Int(4, 0),  
        new Vector2Int(1, 1),  
        new Vector2Int(4, 4),  
        new Vector2Int(4, 1),  
        new Vector2Int(4, 3)
    };

        
        roundNodePositions[1] = new List<Vector2Int>
    {
        new Vector2Int(0, 4),
        new Vector2Int(1, 0),
        new Vector2Int(1, 4),
        new Vector2Int(4, 4),
        new Vector2Int(1, 3),
        new Vector2Int(2, 0),  
        new Vector2Int(1, 1),  
        new Vector2Int(3, 2)
    };

        
        roundNodePositions[2] = new List<Vector2Int>
    {
        new Vector2Int(0, 4),
        new Vector2Int(1, 2),
        new Vector2Int(1, 3),
        new Vector2Int(4, 1),
        new Vector2Int(1, 1),
        new Vector2Int(3, 3),  
        new Vector2Int(3, 4),  
        new Vector2Int(4, 2)
    };

        roundNodePositions[3] = new List<Vector2Int>
    {
        new Vector2Int(1, 0),
        new Vector2Int(4, 2),
        new Vector2Int(1, 1),
        new Vector2Int(2, 2),
        new Vector2Int(1, 3),
        new Vector2Int(4, 1),  
        new Vector2Int(2, 1),  
        new Vector2Int(4, 0)
    };

        roundNodePositions[4] = new List<Vector2Int>
    {
        new Vector2Int(0, 4),
        new Vector2Int(4, 2),
        new Vector2Int(1, 4),
        new Vector2Int(2, 1),
        new Vector2Int(3, 4),
        new Vector2Int(3, 1),  
        new Vector2Int(3, 3),  
        new Vector2Int(4, 4)
    };



        Debug.Log($"Manual node positions set for {maxRounds} rounds");
    }

    void StartRound(int roundNumber)
    {
        gameWon = false;
        currentRound = roundNumber;

        Debug.Log($"=== STARTING ROUND {roundNumber} ===");

        
        ClearBoard();

        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
            Debug.Log("Win panel hidden");
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            Debug.Log("Next button hidden");
        }

        
        SpawnRoundNodes(roundNumber - 1);

        UpdateRoundText();

        Debug.Log($"Round {roundNumber} setup complete with {nodes.Count} nodes");
    }

    void SpawnRoundNodes(int roundIndex)
    {
        if (roundIndex < 0 || roundIndex >= maxRounds)
        {
            Debug.LogError($"Invalid round index: {roundIndex}");
            return;
        }

        List<Vector2Int> roundPositions = roundNodePositions[roundIndex];
        int colorId = 0;
        int nodesPlacedForCurrentColor = 0;

        Debug.Log($"Spawning {roundPositions.Count} nodes for round {roundIndex + 1}");

        foreach (Vector2Int cell in roundPositions)
        {
            SpawnNodeAtCell(cell, colorId);
            nodesPlacedForCurrentColor++;

            if (nodesPlacedForCurrentColor >= nodesPerColor)
            {
                colorId++;
                nodesPlacedForCurrentColor = 0;

                if (colorId >= colors.Length)
                    colorId = colors.Length - 1;
            }
        }

        Debug.Log($"Spawned {nodes.Count} nodes for round {roundIndex + 1}");
    }

    void SpawnNodeAtCell(Vector2Int cell, int colorId)
    {
        Vector2 worldPos = GridToWorld(cell);
        GameObject nodeObj = Instantiate(nodePrefab, new Vector3(worldPos.x, worldPos.y, 0), Quaternion.identity);
        nodeObj.name = $"Node_{cell.x}_{cell.y}_Color{colorId}_Round{currentRound}";

        Node node = nodeObj.GetComponent<Node>();
        if (node != null)
        {
            node.colorId = colorId;

            SpriteRenderer sr = nodeObj.GetComponent<SpriteRenderer>();
            if (sr != null && colorId < colors.Length)
            {
                sr.color = colors[colorId];
            }

            node.SetColor(colors[colorId]);
            nodes[cell] = node;
        }
        else
        {
            Debug.LogError("No Node component found!");
            Destroy(nodeObj);
        }
    }

    void ClearBoard()
    {
        Debug.Log("Clearing board...");

        
        int nodeCount = 0;
        foreach (var kv in nodes)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value.gameObject);
                nodeCount++;
            }
        }
        nodes.Clear();
        Debug.Log($"Destroyed {nodeCount} nodes");

        
        int lineCount = 0;
        foreach (var line in activeFinalLines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
                lineCount++;
            }
        }
        activeFinalLines.Clear();
        occupancy.Clear();
        Debug.Log($"Destroyed {lineCount} lines");

        
        if (lineDrawer != null)
            lineDrawer.ClearCurrentLine();
    }

    public void RestartRound()
    {
        Debug.Log($"=== RESTARTING ROUND {currentRound} ===");
        StartRound(currentRound);
    }

    public void NextRound()
    {
        Debug.Log($"=== NEXT ROUND FROM {currentRound} ===");
        if (currentRound < maxRounds)
        {
            currentRound++;
            StartRound(currentRound);
        }
        else
        {
            Debug.Log("All rounds completed!");
            if (winText != null)
                winText.text = "Game Completed!";
        }
    }

    void CheckAllNodeConnections()
    {
        Debug.Log("=== CHECKING ALL NODE CONNECTIONS ===");
        int connected = 0;
        int total = nodes.Count;

        foreach (var kv in nodes)
        {
            Node n = kv.Value;
            if (n != null)
            {
                string status = n.owningLine != null ? "CONNECTED" : "NOT CONNECTED";
                Debug.Log($"Node at {kv.Key}: {status}");
                if (n.owningLine != null) connected++;
            }
        }

        Debug.Log($"Result: {connected}/{total} nodes connected");

        if (connected == total && total > 0)
        {
            Debug.Log("ALL NODES CONNECTED - CALLING WIN!");
            ShowWinUI();
        }
    }

    void CheckWinCondition()
    {
        if (gameWon)
        {
            Debug.Log("Game already won, skipping win check");
            return;
        }

        DebugWinCondition(); 
    }

    void DebugWinCondition()
    {
        Debug.Log("=== DEBUG WIN CONDITION ===");
        int connectedCount = 0;
        int totalCount = 0;

        foreach (var kv in nodes)
        {
            totalCount++;
            Node node = kv.Value;
            if (node != null)
            {
                if (node.owningLine != null)
                {
                    connectedCount++;
                    Debug.Log($"✅ Node at {kv.Key} - CONNECTED (Color: {node.colorId})");
                }
                else
                {
                    Debug.Log($"❌ Node at {kv.Key} - NOT CONNECTED (Color: {node.colorId})");
                }
            }
        }

        Debug.Log($"=== RESULT: {connectedCount}/{totalCount} nodes connected ===");

        if (connectedCount == totalCount && totalCount > 0)
        {
            Debug.Log("🎉 ALL NODES CONNECTED - WIN CONDITION MET!");
            ShowWinUI();
        }
        else
        {
            Debug.Log("⏳ Not all nodes connected yet...");
        }
    }

    void ShowWinUI()
    {
        if (gameWon) return;

        gameWon = true;

        Debug.Log("=== SHOW WIN UI ===");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("Win panel activated successfully!");
        }
        else
        {
            Debug.LogError("Win panel is null!");
            return;
        }

        if (nextButton != null)
        {
            bool showNext = currentRound < maxRounds;
            nextButton.gameObject.SetActive(showNext);
            Debug.Log($"Next button set to: {showNext} (round {currentRound}/{maxRounds})");
        }

        if (winText != null)
        {
            winText.text = $"Round {currentRound} Complete!";
        }

        Debug.Log($"Round {currentRound} completed - UI should be visible!");
    }

    void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = $"Round: {currentRound}/{maxRounds}";
            Debug.Log($"Round text updated to: {currentRound}/{maxRounds}");
        }
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        float centerX = origin.x + (width - 1) * cellSize / 2f;
        float centerY = origin.y + (height - 1) * cellSize / 2f;
        mainCamera.transform.position = new Vector3(centerX, centerY, -10f);
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        float size = Mathf.Max(width, height) * cellSize * 0.6f;
        mainCamera.orthographicSize = size;
        mainCamera.backgroundColor = Color.black;
    }

    
    public Vector2 GridToWorld(Vector2Int cell)
    {
        return origin + new Vector2(cell.x * cellSize, cell.y * cellSize);
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 rel = worldPos - origin;
        int x = Mathf.RoundToInt(rel.x / cellSize);
        int y = Mathf.RoundToInt(rel.y / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsCellInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    public Node GetNodeAtCell(Vector2Int cell)
    {
        nodes.TryGetValue(cell, out Node node);
        return node;
    }

    public Line GetLineAtCell(Vector2Int cell)
    {
        occupancy.TryGetValue(cell, out Line line);
        return line;
    }

    public void RemoveLineAtCell(Vector2Int cell)
    {
        Line line = GetLineAtCell(cell);
        if (line != null)
        {
            RemoveLine(line);
        }
    }

    public void SetOccupancy(Vector2Int cell, Line line)
    {
        if (occupancy.ContainsKey(cell) && occupancy[cell] != line)
        {
            RemoveLine(occupancy[cell]);
        }
        occupancy[cell] = line;
    }

    public void ClearOccupancy(Vector2Int cell)
    {
        if (occupancy.ContainsKey(cell))
            occupancy.Remove(cell);
    }

    public void RegisterFinalLine(Line line)
    {
        if (line == null || gameWon) return;

        Debug.Log($"Registering final line with {line.cells.Count} cells");

        foreach (var cell in line.cells)
        {
            Line existingLine = GetLineAtCell(cell);
            if (existingLine != null && existingLine != line)
            {
                if (line.startNode != null && existingLine.startNode != null &&
                    line.startNode.colorId != existingLine.startNode.colorId)
                {
                    RemoveLine(existingLine);
                }
            }
        }

        activeFinalLines.Add(line);
        foreach (var cell in line.cells)
        {
            SetOccupancy(cell, line);
        }

        CheckWinCondition();
    }

    public void RemoveLine(Line line)
    {
        if (line == null) return;

        Debug.Log($"Removing line with {line.cells.Count} cells");

        foreach (var c in line.cells)
        {
            if (occupancy.TryGetValue(c, out Line occ))
            {
                if (occ == line)
                    occupancy.Remove(c);
            }
        }

        if (line.startNode != null) line.startNode.ClearOwningLine();
        if (line.endNode != null) line.endNode.ClearOwningLine();

        activeFinalLines.Remove(line);
        Destroy(line.gameObject);

        CheckWinCondition();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 world = GridToWorld(new Vector2Int(x, y));
                Gizmos.DrawWireCube(world, Vector3.one * (cellSize * 0.9f));
            }
        }
    }
}