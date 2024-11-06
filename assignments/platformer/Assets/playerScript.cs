using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class PlayerScript : MonoBehaviour
{
    [Header("Player Movement")]
    public Transform cameraTransform;
    public CharacterController cc;
    // float rotateSpeed = 90;
    float moveSpeed = 100f;
    float jumpVelocity = 50;

    [Header("Death and Respawn")]
    public Vector3 respawnPoint = new Vector3(970.6f, 1.2f, 425.6f);
    public int deathPenalty = 30;
    public string deathTag = "Death";

    [Header("Star Collection")]
    private int starsCollected = 0;
    public int totalStars = 3;
    public TextMeshProUGUI starCountText; 
    public TextMeshProUGUI winText;
    public string starTag = "Star";

    // Movement variables
    float yVelocity = 0;
    float gravity = -11.1f;
    float dashAmount = 8;
    float dashVelocity = 0;
    float friction = -2.8f;
    float fallingTime = 5;
    float coyoteTime = 0.5f;

    GameObject movingPlatform;
    Vector3 previousMovingPlatformPosition;

    void Start()
    {
        if (cc == null)
        {
            cc = GetComponent<CharacterController>();
        }

        if (starCountText != null)
        {
            UpdateStarCountUI();
        }
        
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void UpdateStarCountUI()
    {
        if (starCountText != null)
        {
            starCountText.text = $"Stars: {starsCollected}/{totalStars}";
        }
    }

    void CheckWinCondition()
    {
        if (starsCollected >= totalStars && winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = "You Win!";
        }
    }

    void HandleMovement()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashVelocity = dashAmount;
        }
        dashVelocity += friction * Time.deltaTime;
        dashVelocity = Mathf.Clamp(dashVelocity, 0, 10000);

        if (!cc.isGrounded)
        {
            fallingTime += Time.deltaTime;
            if (fallingTime < coyoteTime && Input.GetKeyDown(KeyCode.Space))
            {
                yVelocity = jumpVelocity;
            }

            if (yVelocity > 0 && Input.GetKeyUp(KeyCode.Space))
            {
                yVelocity = 0;
            }

            yVelocity += gravity * Time.deltaTime;
        }
        else
        {
            yVelocity = -2;
            fallingTime = 0;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                yVelocity = jumpVelocity;
            }
        }

        Vector3 flatCameraForward = cameraTransform.forward;
        flatCameraForward.y = 0;
        Vector3 amountToMove = flatCameraForward.normalized * moveSpeed * vAxis;
        amountToMove += cameraTransform.right * moveSpeed * hAxis;
        amountToMove += transform.forward * dashVelocity;
        amountToMove.y += yVelocity;
        amountToMove *= Time.deltaTime;

        if (movingPlatform != null)
        {
            Vector3 amountPlatformMoved = movingPlatform.transform.position - previousMovingPlatformPosition;
            amountToMove += amountPlatformMoved;
            previousMovingPlatformPosition = movingPlatform.transform.position;
        }

        cc.Move(amountToMove);

        amountToMove.y = 0;
        if (amountToMove.magnitude > 0)
        {
            transform.forward = amountToMove.normalized;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag(deathTag))
        {
            HandleDeath();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovingPlatform"))
        {
            movingPlatform = other.gameObject;
            previousMovingPlatformPosition = movingPlatform.transform.position;
        }
        else if (other.CompareTag(deathTag))
        {
            HandleDeath();
        }
        else if (other.CompareTag(starTag))
        {
            CollectStar(other.gameObject);
        }
    }

    void CollectStar(GameObject star)
    {
        starsCollected++;
        UpdateStarCountUI();
        CheckWinCondition();
        Destroy(star);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MovingPlatform"))
        {
            movingPlatform = null;
        }
    }

    void HandleDeath()
    {
        ScoreDisplay.AddPoints(-deathPenalty);
        
        cc.enabled = false;
        
        transform.position = respawnPoint;
        
        yVelocity = 0;
        dashVelocity = 0;
        
        starsCollected = 0;
        UpdateStarCountUI();
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
        
        // Reactivate all stars
        GameObject[] stars = GameObject.FindGameObjectsWithTag(starTag);
        foreach (GameObject star in stars)
        {
            star.SetActive(true);
        }
        
        cc.enabled = true;
        
        Debug.Log("Player died and respawned at: " + respawnPoint);
    }
}