using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thingScript : MonoBehaviour
{
    public Renderer cubeRenderer;

    public bool alive = false;

    public int aliveCount = 0;

    public int xIndex = -1;
    public int yIndex = -1;

    public Color aliveColor;
    public Color deadColor;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        SetColor();

        GameObject gmObj = GameObject.Find("GameManagerObject");
        gameManager = gmObj.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        alive = !alive;
        SetColor();

        // Count my neighbors!
        int neighborCount = gameManager.CountNeighbors(xIndex, yIndex);
        Debug.Log("(" + xIndex + "," + yIndex + "): " + neighborCount);
    }
    public void SetColor() {
        if (alive) {
            cubeRenderer.material.color = aliveColor;
        } else {
            cubeRenderer.material.color = deadColor;
        }
        // cubeRenderer.material.color = Color.HSVToRGB(aliveCount / 100f, 0.6f, 1f);
    }
}