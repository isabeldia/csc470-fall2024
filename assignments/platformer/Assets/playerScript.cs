using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerScript : MonoBehaviour
{
    public Transform cameraTransform;

    public CharacterController cc;
    float rotateSpeed = 90;
    float moveSpeed = 50f;
    float jumpVelocity = 50;

    // These will be used to simulate gravity, and for jumping
    float yVelocity = 0;
    float gravity = -9.8f;

    // These will be used to create a "dash"
    float dashAmount = 8;
    float dashVelocity = 0;
    float friction = -2.8f;

    GameObject movingPlatform;
    Vector3 previousMovingPlatformPosition;

    // This will keep track of how long we have been falling, we will use this 
    // for "coyote time" (keeping track of how long it has been since we have
    // started falling), and letting the player jump for a certain amount of time.
    float fallingTime = 0;
    float coyoteTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get WASD input
        float hAxis = Input.GetAxisRaw("Horizontal"); // A & D
        float vAxis = Input.GetAxisRaw("Vertical");   // W & S

        // --- ROTATION ---
        // Rotate on the y axis based on the hAxis value
        // NOTE: If the player isn't pressing left or right, hAxis will be 0 and there will be no rotation
        // transform.Rotate(0, rotateSpeed * hAxis * Time.deltaTime, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            dashVelocity = dashAmount;
        }
        // Slow the dash down, and keep it from going below zero (using clamp)
        dashVelocity += friction * Time.deltaTime;
        dashVelocity = Mathf.Clamp(dashVelocity, 0, 10000);

        if (!cc.isGrounded) 
        {
            // *** If we are in here, we are IN THE AIR ***

            fallingTime += Time.deltaTime;
            // Let the player jump if they have only been falling for a little bit
            if (fallingTime < coyoteTime && Input.GetKeyDown(KeyCode.Space)) {
                yVelocity = jumpVelocity;
            }
            
            // If we go in this block of code, cc.isGrounded is false, which means
            // the last time cc.Move was called, we did not try to enter the ground.

            // If the player releases space and the player is moving upwards, stop upward velocity
            // so that the player begins to fall.
            if (yVelocity > 0 && Input.GetKeyUp(KeyCode.Space))
            {
                yVelocity = 0;
            }

            // Apply gravity to the yVelocity
            yVelocity += gravity * Time.deltaTime;
        }
        else
        {
            // *** If we are in here, we are ON THE GROUND ***

            // Set velocity downward so that the CharacterController collides with the
            // ground again, and isGrounded is set to true.
            yVelocity = -2;

            fallingTime = 0;

            // Jump!
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                yVelocity = jumpVelocity;
            }
        }

        // --- TRANSLATION ---
        // Move the player based on camera direction and WASD input
        Vector3 flatCameraForward = cameraTransform.forward;
        flatCameraForward.y = 0;
        Vector3 amountToMove = flatCameraForward.normalized * moveSpeed * vAxis;
        amountToMove += cameraTransform.right * moveSpeed * hAxis;

        // Apply the dash (i.e. add the forward vector scaled by the forwardVelocity)
        amountToMove += transform.forward * dashVelocity;

        amountToMove.y += yVelocity;

        amountToMove *= Time.deltaTime;

        // Deal with moving platform. Add the amount that the platform moved to amountToMove
        if (movingPlatform != null) {
            Vector3 amountPlatformMoved = movingPlatform.transform.position - previousMovingPlatformPosition;
            amountToMove += amountPlatformMoved;
            previousMovingPlatformPosition = movingPlatform.transform.position;
        }

        // This will move the player according to the forward vector and the yVelocity using the
        // CharacterController.
        cc.Move(amountToMove);

        // Handle the rotation
        amountToMove.y = 0;
        transform.forward = amountToMove.normalized;
        // transform.rotation = Quaternion.LookRotation(amountToMove);
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("MovingPlatform")) {
            movingPlatform = other.gameObject;
            previousMovingPlatformPosition = movingPlatform.transform.position;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("MovingPlatform")) {
            movingPlatform = null;
        }
    }
}