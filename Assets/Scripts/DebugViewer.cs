using UnityEngine;

public class DebugViewer : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 20), "Nodes: " + FindObjectsOfType<Node>().Length);
        GUI.Label(new Rect(10, 30, 500, 20), "Lines: " + FindObjectsOfType<LineRenderer>().Length);
        GUI.Label(new Rect(10, 50, 500, 20), "Camera: " + Camera.main.name);
        
        if (Camera.main != null)
        {
            GUI.Label(new Rect(10, 70, 500, 20), "Camera Pos: " + Camera.main.transform.position);
            GUI.Label(new Rect(10, 90, 500, 20), "Camera Size: " + Camera.main.orthographicSize);
        }
    }
}