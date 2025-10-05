using UnityEngine;

public class Node : MonoBehaviour
{
    public int colorId = 0;
    [HideInInspector] public Line owningLine = null;

    void Start()
    {
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.GetBuiltinResource<Sprite>("Circle");
        }
        
        if (sr.sprite == null)
        {
            sr.sprite = Resources.GetBuiltinResource<Sprite>("Circle");
        }
        
        Debug.Log($"Node {colorId} started with SR: {sr != null}, Sprite: {sr.sprite != null}");
    }

    public void SetColor(Color c)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = c;
            Debug.Log($"SET NODE COLOR TO: {c}");
        }
        else
        {
            Debug.LogError("No SpriteRenderer found!");
        }
    }

    public void SetOwningLine(Line line)
    {
        owningLine = line;
    }

    public void ClearOwningLine()
    {
        owningLine = null;
    }
}