using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum GameState
{
    Instructions,
    PuzzleMode,
    RaycastMode,
    GameWon,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Canvas instructionsCanvas;
    public Button startButton;
    public Button tryAgainButton;
    public Canvas winCanvas;
    public Canvas gameOverCanvas;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI flowerStatusText;
    public TextMeshProUGUI gameOverText;

    public GameObject tilesParent;
    public GameObject[] enemyObjects;

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
        
        foreach (GameObject enemy in enemyObjects)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
            }
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }

    private IEnumerator TransitionToRaycastMode()
    {
        Debug.Log("Starting transition to raycast mode...");
        
        if (tilesParent != null)
        {
            tilesParent.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log($"Activating {enemyObjects.Length} enemies");
        foreach (GameObject enemy in enemyObjects)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = true;
                }
                Debug.Log($"Activated enemy: {enemy.name}");
            }
        }

        Debug.Log("Transition complete!");
    }

    private void SetEnemiesActive(bool active)
    {
        foreach (GameObject enemy in enemyObjects)
        {
            if (enemy != null)
            {
                enemy.SetActive(active);
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
        
        Debug.Log($"Number of enemies found: {enemyObjects.Length}");
        foreach (GameObject enemy in enemyObjects)
        {
            Debug.Log($"Enemy: {enemy.name}, Active: {enemy.activeSelf}");
        }
    }

    void Start()
    {
        SetupUI();
        ShowInstructions();
        
        Debug.Log($"Initial enemy count: {enemyObjects.Length}");
        foreach (GameObject enemy in enemyObjects)
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
            "Rules (press button to close):\n" +
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

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(RestartGame);
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
        if (winCanvas != null)
        {
            winCanvas.gameObject.SetActive(false);
        }

        currentGameState = GameState.Instructions;
        hasFlower = false;
        ShowInstructions();

        GameObject player = GameObject.FindGameObjectWithTag("unit");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Vector3 startPos = new Vector3(-0.055921345949172977f, 0.15399999916553498f, -1.3643319606781006f);
                player.transform.position = startPos;
                UnityEngine.AI.NavMeshAgent playerAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (playerAgent != null)
                {
                    playerAgent.enabled = false;
                }
            }
        }

        if (tilesParent != null)
        {
            tilesParent.SetActive(true);
        }
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

    private bool isGameOver = false;

    public void GameOver()
    {
        if (isGameOver) return;
        
        Debug.Log("Game Over triggered!");
        isGameOver = true;
        currentGameState = GameState.GameOver;
        
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = "You lose :(\nTry again?";
            }
        }
        else
        {
            Debug.LogError("Game Over Canvas not assigned!");
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("unit");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                UnityEngine.AI.NavMeshAgent playerAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (playerAgent != null)
                {
                    playerAgent.enabled = false;
                }
            }
        }
        
        SetEnemiesActive(false);
    }

    public void SwitchToRaycastMode()
    {
        currentGameState = GameState.RaycastMode;
        StartCoroutine(TransitionToRaycastMode()); // switches from w,a,s,d control to raycast click
    }

    public void CollectFlower()
    {
        hasFlower = true;
        Debug.Log("Flower collected!");
        
        if (flowerStatusText != null)
        {
            flowerStatusText.gameObject.SetActive(true);
            flowerStatusText.text = "Flower Collected!";
            StartCoroutine(CoolFlowerText());
        }
    }

    // cool text effect! doesnt work rn but... hopefully will!!!!!
    private IEnumerator CoolFlowerText()
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
            Debug.Log("must collect flower first");
            if (flowerStatusText != null)
            {
                flowerStatusText.gameObject.SetActive(true);
                flowerStatusText.text = "Collect the flower first!";
                StartCoroutine(CoolFlowerText());
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