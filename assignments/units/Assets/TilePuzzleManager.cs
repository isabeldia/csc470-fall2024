using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileType
{
    Pink,   // No effect
    Red,    // Wall
    Yellow, // Forces back
    Green,  // Reset to start
    Orange, // Changes flavor to orange
    Purple, // Slides + changes flavor to lemon
    Blue    // Conditional based on adjacent tiles and flavor
}

public class TilePuzzleManager : MonoBehaviour
{
    public static TilePuzzleManager instance;
    
    private int gridWidth = 8;
    private int gridHeight = 8;
    private float tileSize = 1f;
    
    private GameObject tilePrefab;
    private Material[] tileMaterials;
    
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    
    private TileType[,] puzzleGrid;
    private GameObject[,] tileObjects;
    private PlayerController player;
    private string currentFlavor = "Normal";
    private Vector2Int lastPosition;
    private Vector3 startPosition;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
            
        puzzleGrid = new TileType[gridWidth, gridHeight];
        tileObjects = new GameObject[gridWidth, gridHeight];
    }
    
    void Start()
    {
        GeneratePuzzle();
        SetupExistingPlayer();
    }
    
    void GeneratePuzzle()
    {
        GameObject tilesParent = new GameObject("Tiles");
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                puzzleGrid[x, z] = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);
                
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, tilesParent.transform);
                tile.name = $"Tile_{x}_{z}";
                
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                renderer.material = tileMaterials[(int)puzzleGrid[x, z]];
                
                tileObjects[x, z] = tile;
            }
        }
    }
    
    void SetupExistingPlayer()
    {
        GameObject existingTurtle = GameObject.FindGameObjectWithTag("unit");
        if (existingTurtle != null)
        {
            // Initial position
            startPosition = new Vector3(-0.055921345949172977f, 0.15399999916553498f, -1.3643319606781006f);
            
            player = existingTurtle.AddComponent<PlayerController>();
            player.Initialize(this);
            
            float desiredY = 0.15399999916553498f;
            existingTurtle.transform.position = new Vector3(
                existingTurtle.transform.position.x,
                desiredY,
                existingTurtle.transform.position.z
            );
        }
    }
    
    public bool IsValidMove(Vector2Int currentPos, Vector2Int targetPos)
    {
        if (targetPos.x < 0 || targetPos.x >= gridWidth || 
            targetPos.y < 0 || targetPos.y >= gridHeight)
            return false;
            
        if (puzzleGrid[targetPos.x, targetPos.y] == TileType.Red)
            return false;
            
        return true;
    }
    
    public void ProcessTileEffect(Vector2Int position)
    {
        TileType currentTile = puzzleGrid[position.x, position.y];
        
        switch (currentTile)
        {
            case TileType.Yellow:
                HandleYellowTile();
                break;
                
            case TileType.Green:
                HandleGreenTile();
                break;
                
            case TileType.Orange:
                currentFlavor = "Orange";
                break;
                
            case TileType.Purple:
                currentFlavor = "Lemon";
                HandlePurpleTile();
                break;
                
            case TileType.Blue:
                HandleBlueTile(position);
                break;
        }
        
        lastPosition = position;
    }
    
    private void HandleYellowTile()
    {
        if (lastPosition != Vector2Int.zero)
        {
            player.ForceMoveToPosition(lastPosition);
        }
    }
    
    private void HandleGreenTile()
    {
        player.ResetToStart(startPosition);
    }
    
    private void HandlePurpleTile()
    {
        Vector2Int facing = player.GetFacingDirection();
        Vector2Int nextPosition = lastPosition + facing;
        
        if (IsValidMove(lastPosition, nextPosition))
        {
            player.ForceMoveToPosition(nextPosition);
        }
    }
    
    private void HandleBlueTile(Vector2Int position)
    {
        bool adjacentToYellow = CheckAdjacentTiles(position, TileType.Yellow);
        
        if (adjacentToYellow || currentFlavor == "Orange")
        {
            // Act like a yellow tile
            HandleYellowTile();
        }
        // Otherwise, acts like a pink tile (no effect)
    }
    
    private bool CheckAdjacentTiles(Vector2Int position, TileType typeToCheck)
    {
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x, position.y + 1),
            new Vector2Int(position.x, position.y - 1)
        };
        
        foreach (Vector2Int pos in adjacentPositions)
        {
            if (pos.x >= 0 && pos.x < gridWidth && 
                pos.y >= 0 && pos.y < gridHeight &&
                puzzleGrid[pos.x, pos.y] == typeToCheck)
            {
                return true;
            }
        }
        
        return false;
    }
}

public class PlayerController : MonoBehaviour
{
    private TilePuzzleManager puzzleManager;
    private Vector2Int facingDirection = new Vector2Int(1, 0);
    private bool isMoving = false;
    private float originalY;
    
    public void Initialize(TilePuzzleManager manager)
    {
        puzzleManager = manager;
        originalY = transform.position.y;
    }
    
    void Update()
    {
        if (isMoving) return;
        
        Vector2Int movement = Vector2Int.zero;
        
        if (Input.GetKeyDown(KeyCode.W)) movement.y = 1;
        else if (Input.GetKeyDown(KeyCode.S)) movement.y = -1;
        else if (Input.GetKeyDown(KeyCode.A)) movement.x = -1;
        else if (Input.GetKeyDown(KeyCode.D)) movement.x = 1;
        
        if (movement != Vector2Int.zero)
        {
            facingDirection = movement;
            TryMove(movement);
        }
    }
    
    private void TryMove(Vector2Int movement)
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        
        Vector2Int targetPos = currentPos + movement;
        
        if (puzzleManager.IsValidMove(currentPos, targetPos))
        {
            StartCoroutine(MoveToPosition(targetPos));
            puzzleManager.ProcessTileEffect(targetPos);
        }
    }
    
    public void ForceMoveToPosition(Vector2Int position)
    {
        StartCoroutine(MoveToPosition(position));
    }
    
    public void ResetToStart(Vector3 startPos)
    {
        StartCoroutine(MoveToWorldPosition(startPos));
    }
    
    private IEnumerator MoveToPosition(Vector2Int targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, originalY, targetPos.y);
        float moveTime = 0.2f;
        float elapsedTime = 0;
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        transform.position = endPos;
        isMoving = false;
    }
    
    private IEnumerator MoveToWorldPosition(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float moveTime = 0.2f;
        float elapsedTime = 0;
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        transform.position = targetPos;
        isMoving = false;
    }
    
    public Vector2Int GetFacingDirection()
    {
        return facingDirection;
    }
}