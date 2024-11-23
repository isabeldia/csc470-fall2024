using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum GameState
{
    Instructions,
    PuzzleMode,
    RaycastMode,
    GameWon
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public Canvas instructionsCanvas;
    public TextMeshProUGUI instructionsText;
    public Button startButton;
    public Canvas winCanvas;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI flowerStatusText;

    [Header("Game Objects")]
    public GameObject tilesParent;
    public GameObject[] enemies;

    private GameState currentGameState;
    private bool hasFlower = false;

    public GameState CurrentGameState
    {
        get { return currentGameState; }
        private set { currentGameState = value; }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        currentGameState = GameState.Instructions;
        enemies = GameObject.FindGameObjectsWithTag("enemies");
        SetEnemiesActive(false);
    }

    private IEnumerator TransitionToRaycastMode()
    {
        Debug.Log("Starting transition to raycast mode...");
        
        currentGameState = GameState.RaycastMode;
        
        if (tilesParent != null)
        {
            Debug.Log($"Deactivating tiles parent: {tilesParent.name}");
            tilesParent.SetActive(false);
        }
        else
        {
            Debug.LogError("TilesParent is null!");
        }

        yield return new WaitForSeconds(0.5f);

        enemies = GameObject.FindGameObjectsWithTag("enemies");
        Debug.Log($"Found {enemies.Length} enemies to activate");
        
        SetEnemiesActive(true);
        
        Debug.Log("Transition complete!");
    }

    private void SetEnemiesActive(bool active)
    {
        if (active)
        {
            enemies = GameObject.FindGameObjectsWithTag("enemies");
            Debug.Log($"Refreshed enemies array, found {enemies.Length} enemies");
        }
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(active);
                Debug.Log($"Set enemy {enemy.name} active: {active}");
                
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = active;
                }
            }
        }
    }

    public void DebugSceneSetup()
    {
        Debug.Log($"Tiles Parent null? {tilesParent == null}");
        if (tilesParent != null)
        {
            Debug.Log($"Tiles Parent active? {tilesParent.activeSelf}");
            Debug.Log($"Number of tile children: {tilesParent.transform.childCount}");
        }
        
        enemies = GameObject.FindGameObjectsWithTag("enemies");
        Debug.Log($"Number of enemies found: {enemies.Length}");
        foreach (GameObject enemy in enemies)
        {
            Debug.Log($"Enemy: {enemy.name}, Active: {enemy.activeSelf}");
        }
    }

    void Start()
    {
        SetupUI();
        ShowInstructions();
        
        enemies = GameObject.FindGameObjectsWithTag("enemies");
        Debug.Log($"Initial enemy count: {enemies.Length}");
        foreach (GameObject enemy in enemies)
        {
            Debug.Log($"Found enemy: {enemy.name}");
        }
        
        if (tilesParent == null)
        {
            Debug.LogError("Tiles Parent not assigned!");
        }
    }

    void SetupUI()
    {
        instructionsText.text = 
            "Pink tiles have no effect and can be walked on freely.\n" +
            "Green tiles set the player (turtle) back to the starting position.\n" +
            "Red tiles act as solid walls; players cannot move through/on them.\n" +
            "Yellow tiles force the player back to the last tile they stepped on\n" +
            "(the player's controls are locked in the meantime).\n" +
            "Orange tiles change the player's flavor to \"Orange\".\n" +
            "Purple tiles force the player to the next tile in the direction they are facing.\n" +
            "Additionally, purple tiles change the player's flavor to \"Lemon\".\n" +
            "Blue tiles vary in function depending on the following factors:\n" +
            "    If the blue tile is adjacent to a yellow tile, the blue tile acts identically to a yellow tile,\n" +
            "    simulating electrified water.\n" +
            "    If the player's flavor is \"Orange\", the blue tile also acts identically to a yellow tile,\n" +
            "    luring piranhas out and forcing the player back to the previous tile.\n" +
            "    If the player's flavor is \"Lemon\" and the tile is not adjacent to a yellow tile,\n" +
            "    the tile acts identically to a pink tile.\n" +
            "    If none of the above conditions is met, the blue tile acts identically to a pink tile.";

        winText.text = "Congratulations!\nYou Win!";
        
        startButton.onClick.AddListener(StartGame);
        winCanvas.gameObject.SetActive(false);
    }

    public void ShowInstructions()
    {
        currentGameState = GameState.Instructions;
        instructionsCanvas.gameObject.SetActive(true);
        winCanvas.gameObject.SetActive(false);
        SetEnemiesActive(false);
    }

    public void StartGame()
    {
        currentGameState = GameState.PuzzleMode;
        instructionsCanvas.gameObject.SetActive(false);
    }

    public void SwitchToRaycastMode()
    {
        currentGameState = GameState.RaycastMode;
        StartCoroutine(TransitionToRaycastMode());
    }

    public void CollectFlower()
    {
        hasFlower = true;
        Debug.Log("Flower collected!");
        
        if (flowerStatusText != null)
        {
            flowerStatusText.gameObject.SetActive(true);
            flowerStatusText.text = "Flower Collected!";
            StartCoroutine(FadeFlowerText());
        }
    }

    private IEnumerator FadeFlowerText()
    {
        yield return new WaitForSeconds(2f);
        
        if (flowerStatusText != null)
        {
            float alpha = 1f;
            Color textColor = flowerStatusText.color;
            
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime;
                textColor.a = alpha;
                flowerStatusText.color = textColor;
                yield return null;
            }
            
            flowerStatusText.gameObject.SetActive(false);
        }
    }

    public void TryWinGame()
    {
        if (hasFlower)
        {
            WinGame();
        }
        else
        {
            Debug.Log("Need to collect the flower!");
            if (flowerStatusText != null)
            {
                flowerStatusText.gameObject.SetActive(true);
                flowerStatusText.text = "Find the flower first!";
                StartCoroutine(FadeFlowerText());
            }
        }
    }

    private void WinGame()
    {
        currentGameState = GameState.GameWon;
        winCanvas.gameObject.SetActive(true);
        SetEnemiesActive(false);
    }

    public bool HasFlower()
    {
        return hasFlower;
    }
}