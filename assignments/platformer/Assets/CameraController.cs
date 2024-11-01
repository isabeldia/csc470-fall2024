using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;                // The player to follow
    public float distanceFromTarget = 10f;  // How far to stay from player
    public float heightOffset = 5f;         // How high above the player
    
    [Header("Camera Control")]
    public float rotationSpeed = 2f;        // How fast camera rotates around player
    public float smoothSpeed = 10f;         // How smoothly the camera follows
    public float minVerticalAngle = -30f;   // Lowest angle camera can go
    public float maxVerticalAngle = 60f;    // Highest angle camera can go
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        // Initialize rotation to current camera rotation
        rotationY = transform.eulerAngles.y;
        rotationX = transform.eulerAngles.x;
        
        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Calculate rotation
        rotationY += mouseX;
        rotationX -= mouseY; // Inverted for natural feel
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Calculate camera position
        Vector3 targetRotation = new Vector3(rotationX, rotationY);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref smoothVelocity, 1f / smoothSpeed);

        transform.eulerAngles = currentRotation;

        // Calculate desired position using spherical coordinates
        float horizontalDistance = distanceFromTarget * Mathf.Cos(rotationX * Mathf.Deg2Rad);
        float verticalDistance = distanceFromTarget * Mathf.Sin(rotationX * Mathf.Deg2Rad);

        Vector3 desiredPosition = target.position;
        desiredPosition -= transform.forward * horizontalDistance;
        desiredPosition += Vector3.up * (heightOffset + verticalDistance);

        // Set position and look at target
        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * heightOffset);
    }

    // Call this if you need to unlock the cursor (e.g., for menus)
    public void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}