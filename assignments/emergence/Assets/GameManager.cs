using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject thingPrefab;

    thingScript[,] grid;
    float spacing = 1.1f;

    float simulationTimer;
    float simulationRate = 0.1f;
    bool isSimulating = false;
    bool gameEnded = false;

    // the size of the grid
    const int gridSize = 100; 
    const int heartSize = 10; // size of each heart
    const int numHearts = 25;  // # of hearts to scatter

    void Start()
    {
        simulationTimer = simulationRate;

        grid = new thingScript[gridSize, gridSize];
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 pos = transform.position;
                pos.x += x * spacing;
                pos.z += y * spacing;
                GameObject thing = Instantiate(thingPrefab, pos, Quaternion.identity);
                grid[x,y] = thing.GetComponent<thingScript>();
                grid[x,y].alive = false; // initialize all cells as dead
                grid[x,y].xIndex = x;
                grid[x,y].yIndex = y;
            }
        }

        // multiple scattered hearts
        for (int i = 0; i < numHearts; i++)
        {
            CreateHeart();
        }

        // colors for all cells
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x,y].SetColor();
            }
        }

        gameEnded = false;
    }

    void CreateHeart()
    {
        // heart shape
        int[,] heart = new int[,]
        {
            {0,0,1,1,0,0,0,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1},
            {0,1,1,1,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,1,1,1,0,0},
            {0,0,0,1,1,1,1,1,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0},
            {0,0,0,0,0,1,0,0,0,0,0}
        };

        // random position for the heart
        int startX, startY;
        do
        {
            startX = Random.Range(0, gridSize - heartSize);
            startY = Random.Range(0, gridSize - heartSize);
        } while (!IsAreaClear(startX, startY, heartSize, heartSize));

        // set
        for (int x = 0; x < heartSize; x++)
        {
            for (int y = 0; y < heartSize; y++)
            {
                grid[startX + x, startY + y].alive = (heart[x, y] == 1);
            }
        }
    }

    bool IsAreaClear(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (grid[x, y].alive)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public int CountNeighbors(int xIndex, int yIndex)
    {
        int count = 0;

        for (int x = xIndex - 1; x <= xIndex + 1; x++)
        {
            for (int y = yIndex - 1; y <= yIndex + 1; y++)
            {
                if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                {
                    if (!(x == xIndex && y == yIndex))
                    {
                        if (grid[x,y].alive)
                        {
                            count++;
                        }
                    }
                }
            }
        }

        return count;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gameEnded)
            {
                InitializeGrid();
                isSimulating = true;
            }
            else
            {
                isSimulating = !isSimulating;
            }
        }

        if (isSimulating)
        {
            simulationTimer -= Time.deltaTime;
            if (simulationTimer < 0)
            {
                Simulate();
                simulationTimer = simulationRate;
            }
        }
    }

    void Simulate()
    {
        bool[,] nextAlive = new bool[gridSize, gridSize];
        bool hasChanged = false;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int neighborCount = CountNeighbors(x, y);
                if (grid[x,y].alive && neighborCount < 2) {
                    nextAlive[x,y] = false;
                } else if (grid[x,y].alive && (neighborCount == 2 || neighborCount == 3)) {
                    nextAlive[x,y] = true;
                } else if (grid[x,y].alive && neighborCount > 3) {
                    nextAlive[x,y] = false;
                } else if (!grid[x,y].alive && neighborCount == 3) {
                    nextAlive[x,y] = true;
                } else {
                    nextAlive[x,y] = grid[x,y].alive;
                }

                if (nextAlive[x,y] != grid[x,y].alive)
                {
                    hasChanged = true;
                }
            }
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x,y].alive = nextAlive[x,y];
                grid[x,y].SetColor();
            }
        }

        if (!hasChanged)
        {
            gameEnded = true;
            isSimulating = false;
        }
    }
}