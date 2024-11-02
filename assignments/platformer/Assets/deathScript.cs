using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deathScript : MonoBehaviour
{
    public int deathPenalty = 30;
    public Vector3 respawnPoint = new Vector3(970.6f, 1.2f, 425.6f);
    public string deathTag = "Death";

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider other)
    {
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
    }
}