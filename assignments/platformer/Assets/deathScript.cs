using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScript : MonoBehaviour 
{
    public int deathPenalty = 30;
    public Vector3 respawnPoint = new Vector3(970.6f, 1.2f, 425.6f);
    public string deathTag = "Death";

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("No CharacterController found on player!");
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)  // Add this method
    {
        if (hit.gameObject.CompareTag(deathTag))
        {
            ScoreDisplay.AddPoints(-deathPenalty);
            RespawnPlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Keep this as backup for trigger collisions
        if (other.CompareTag(deathTag))
        {
            ScoreDisplay.AddPoints(-deathPenalty);
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = respawnPoint;
            characterController.enabled = true;
        }
        else
        {
            transform.position = respawnPoint;
        }
        
        Debug.Log("Player respawned at: " + respawnPoint);
    }
}