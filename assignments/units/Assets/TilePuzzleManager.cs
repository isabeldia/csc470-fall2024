using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Pink tiles have no effect and can be walked on freely.
// Green tiles set the player (turtle) back to the starting position of (-0.055921345949172977f, 0.15399999916553498f, -1.3643319606781006f).
// Red tiles act as solid walls; players cannot move through/on them.
// Yellow tiles force the player back to the last tile they stepped on (the player's controls are locked in the meantime).
// Orange tiles change the player's flavor to "Orange".
// Purple tiles force the player to the next tile in the direction they are facing. Additionally, purple tiles change the player's flavor to "Lemon".
// Blue tiles vary in function depending on the following factors:
//     If the blue tile is adjacent to a yellow tile, the blue tile acts identically to a yellow tile, simulating electrified water.
//     If the player's flavor is "Orange", the blue tile also acts identically to a yellow tile, luring the piranhas out and forcing the player back to the previous tile.
//     If the player's flavor is "Lemon" and the tile is not adjacent to a yellow tile, the tile acts identically to a pink tile.
//     If neither of the above conditions is met, the blue tile acts identically to a pink tile.

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
    public int gridHeight { get; private set; } = 8;  // essentially a setter & getter
    private float tileSize = 1f;
    
    [Header("TILES")]
    public GameObject tilePrefab;
    public Material[] tileMaterials;
    
    [Header("Player Settings")]
    public GameObject turtle;
    private PlayerController player;
    public PlayerController Player => player;
    public float moveSpeed = 5f;
    
    private TileType[,] puzzleGrid;
    private GameObject[,] tileObjects;
    private string currentFlavor = "Normal";
    private Vector3 startPosition;
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    public bool isProcessingEffect { get; private set; } = false;
    
    public Vector3 StartPosition => startPosition;
    public Vector3 PreviousPosition => previousPosition;
    public Vector3 CurrentPosition => currentPosition;
    
    public void ResetEffectProcessing()
    {
        isProcessingEffect = false;
        Debug.Log("reset effect processing");
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        puzzleGrid = new TileType[gridWidth, gridHeight];
        tileObjects = new GameObject[gridWidth, gridHeight];
        startPosition = new Vector3(-0.055921345949172977f, 0.15399999916553498f, -1.3643319606781006f);
        previousPosition = startPosition;
        currentPosition = startPosition;
    }

    private LayerMask interactableLayerMask;
    private UnityEngine.AI.NavMeshAgent navAgent;
    void Start()
    {
        interactableLayerMask = LayerMask.GetMask("Default", "Ground", "Player", "Interactable");

        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }
        navAgent.enabled = false;
        
        GeneratePuzzle();
        SetupExistingPlayer();
    }
    
    void GeneratePuzzle()
    {
        GameObject tilesParent = new GameObject("Tiles");
        
        GameManager.Instance.tilesParent = tilesParent;
        
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
            turtle = existingTurtle;
            player = turtle.GetComponent<PlayerController>();
            if (player == null)
            {
                player = turtle.AddComponent<PlayerController>();
            }
            player.Initialize(this, interactableLayerMask);
            turtle.transform.position = startPosition;
            previousPosition = startPosition;
            currentPosition = startPosition;
        }
    }

    public void ProcessTileEffect(Vector2Int position, Vector3 worldPos)
    {
        isProcessingEffect = true;
        
        Vector3 positionBeforeEffect = previousPosition;
        
        previousPosition = currentPosition;
        currentPosition = worldPos;
        
        TileType currentTile = puzzleGrid[position.x, position.y];
        Debug.Log($"Current Tile: {currentTile}");
        // Debug.Log($"Previous: {previousPosition}, Current: {currentPosition}, Before Effect: {positionBeforeEffect}");
        
        switch (currentTile)
        {
            case TileType.Pink:
                isProcessingEffect = false;
                break;

            case TileType.Yellow:
                HandleYellowTile(positionBeforeEffect);
                break;
                
            case TileType.Green:
                HandleGreenTile();
                break;
                
            case TileType.Orange:
                HandleOrangeTile();
                break;
                
            case TileType.Purple:
                HandlePurpleTile(position);
                break;
                
            case TileType.Blue:
                HandleBlueTile(position, positionBeforeEffect);
                break;
        }
    }

    private void HandleYellowTile(Vector3 returnPosition)
    {
        Debug.Log($"Yellow Tile: Moving back to previous position {returnPosition}");
        currentPosition = returnPosition;
        player.MoveToWorldPosition(returnPosition, () => {
            Debug.Log("Yellow Tile effect completed");
            isProcessingEffect = false;
        });
    }
    
    private void HandleGreenTile()
    {
        Debug.Log($"Green Tile: Resetting to start position {startPosition}");
        currentPosition = startPosition;
        player.MoveToWorldPosition(startPosition, () => {
            Debug.Log("Green Tile effect completed");
            isProcessingEffect = false;
            previousPosition = startPosition;
        });
    }
    
    private void HandleOrangeTile()
    {
        currentFlavor = "Orange";
        isProcessingEffect = false;
    }
    
    private void HandlePurpleTile(Vector2Int position)
    {
        currentFlavor = "Lemon";
        Vector2Int facing = player.GetFacingDirection();
        Vector2Int nextPosition = position + facing;
        
        if (IsValidMove(position, nextPosition))
        {
            Vector3 nextWorldPos = new Vector3(nextPosition.x, startPosition.y, nextPosition.y);
            player.MoveToWorldPosition(nextWorldPos, () => {
                isProcessingEffect = false;
                ProcessTileEffect(nextPosition, nextWorldPos);
            });
        }
        else
        {
            isProcessingEffect = false;
        }
    }
    
    private void HandleBlueTile(Vector2Int position, Vector3 returnPosition)
    {
        bool adjacentToYellow = CheckAdjacentTiles(position, TileType.Yellow);
        
        Debug.Log($"Blue Tile: Adjacent to Yellow: {adjacentToYellow}, Current Flavor: {currentFlavor}");
        
        if (adjacentToYellow || currentFlavor == "Orange")
        {
            Debug.Log("Blue Tile pushing back b/c yellow adjacency or orange flavor");
            HandleYellowTile(returnPosition);
        }
        else
        {
            Debug.Log("Blue Tile has no effect");
            isProcessingEffect = false;
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.z / tileSize)
        );
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
    public LayerMask interactableLayerMask;
    public GameObject popUpWindow;
    public GameObject mainCamera;
    private TilePuzzleManager puzzleManager;
    private Vector2Int facingDirection = new Vector2Int(1, 0);
    private bool isMoving = false;
    private Vector3 moveTarget;
    private float moveSpeed = 5f;
    private System.Action onMoveComplete;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private bool isSelected = false;
    private bool isInvulnerable = false;

    void OnTriggerEnter(Collider other)
    {
        if (!isInvulnerable && other.CompareTag("enemies"))
        {
            GameManager.Instance.GameOver();
        }
    }

    public void Initialize(TilePuzzleManager manager, LayerMask interactableLayers)
    {
        puzzleManager = manager;
        moveSpeed = manager.moveSpeed;
        interactableLayerMask = interactableLayers;

        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }
        navAgent.enabled = false;
    }

    private void HandleMovement()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, moveTarget, step);
        
        if (Vector3.Distance(transform.position, moveTarget) < 0.001f)
        {
            transform.position = moveTarget;
            isMoving = false;
            onMoveComplete?.Invoke();
            onMoveComplete = null;
        }
    }

    public void MoveToWorldPosition(Vector3 targetPos, System.Action onComplete = null)
    {
        Debug.Log($"world position: {targetPos}");
        
        if (isMoving) 
        {
            Debug.Log("curretly moving cant move again");
            return;
        }
        
        moveTarget = targetPos;
        onMoveComplete = () => {
            Debug.Log("Move completed");
            onComplete?.Invoke();
        };
        isMoving = true;
    }

    private void TryMove(Vector2Int movement)
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        
        Vector2Int targetPos = currentPos + movement;
        
        Debug.Log($"Moving from {currentPos} to {targetPos}");
        
        if (puzzleManager.IsValidMove(currentPos, targetPos))
        {
            Vector3 targetWorldPos = new Vector3(targetPos.x, transform.position.y, targetPos.y);
            MoveToWorldPosition(targetWorldPos, () => {
                Debug.Log("Move successful, processing tile effect");
                puzzleManager.ProcessTileEffect(targetPos, transform.position);
            });
        }
        else
        {
            Debug.Log("Move not valid");
        }
    }

    public Vector2Int GetFacingDirection()
    {
        return facingDirection;
    }


    void Update()
    {
        GameState currentState = GameManager.Instance.CurrentGameState;
        
        if (currentState == GameState.RaycastMode)
        {
            HandleRaycastMovement();
        }
        else if (currentState == GameState.PuzzleMode)
        {
            if (isMoving)
            {
                HandleMovement();
            }
            else
            {
                HandleInput();
            }
        }
    }

    private void HandleInput()
    {
        Vector2Int movement = Vector2Int.zero;
        
        if (Input.GetKeyDown(KeyCode.W)) 
        {
            movement.y = 1;
            facingDirection = new Vector2Int(0, 1);
            
            Vector2Int currentPos = puzzleManager.WorldToGridPosition(transform.position);
            Debug.Log($"Current Position: {currentPos.y}, Grid Height: {puzzleManager.gridHeight}");
            
            if (currentPos.y >= puzzleManager.gridHeight - 1)
            {
                Debug.Log("Reached top row! Switching to raycast");
                // switches from w,a,s,d to mouse clicking
                puzzleManager.ResetEffectProcessing();
                GameManager.Instance.SwitchToRaycastMode();
                
                if (navAgent != null)
                {
                    navAgent.enabled = true;
                }
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.A)) 
        {
            movement.x = -1;
            facingDirection = new Vector2Int(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S)) 
        {
            movement.y = -1;
            facingDirection = new Vector2Int(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D)) 
        {
            movement.x = 1;
            facingDirection = new Vector2Int(1, 0);
        }
        
        if (movement != Vector2Int.zero)
        {
            TryMove(movement);
        }
    }

    private void HandleRaycastMovement()
    {
        if (GameManager.Instance.CurrentGameState == GameState.GameOver)
        {
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.ResetPath();
            }
            return;
        }

        if (!navAgent.enabled)
        {
            navAgent.enabled = true;
            navAgent.speed = moveSpeed;
            navAgent.acceleration = moveSpeed * 2;
            navAgent.angularSpeed = 360f;
            navAgent.stoppingDistance = 0.1f;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayerMask))
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name} at position {hit.point}");

                isSelected = true;

                if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out UnityEngine.AI.NavMeshHit navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    navAgent.SetDestination(navHit.position);
                    Debug.Log($"NavMesh destination: {navHit.position}");

                    if (hit.collider.CompareTag("flower"))
                    {
                        StartCoroutine(CheckFlowerCollection(hit.collider.gameObject));
                    }
                    else if (hit.collider.CompareTag("thing"))
                    {
                        StartCoroutine(CheckThingInteraction());
                    }
                }
                else
                {
                    Debug.LogWarning("Valid NavMesh position not fouind!");
                }
            }
            else
            {
                Debug.Log("Raycast hitting nothing :(");
            }
        }

        if (navAgent.hasPath)
        {
            Debug.Log($"Moving. Remaining distance: {navAgent.remainingDistance}");
        }
    }
    
    // interface currently not working :( 11/22
    private IEnumerator CheckFlowerCollection(GameObject flower)
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, flower.transform.position) < 1f)
            {
                GameManager.Instance.CollectFlower();
                Destroy(flower);
                break;
            }
            yield return null;
        }
    }

    private IEnumerator CheckThingInteraction()
    {
        while (true)
        {
            GameObject thing = GameObject.FindGameObjectWithTag("thing");
            if (thing != null && Vector3.Distance(transform.position, thing.transform.position) < 1f)
            {
                GameManager.Instance.TryWinGame();
                break;
            }
            yield return null;
        }
    }
}