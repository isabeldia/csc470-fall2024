using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ovalMovement : MonoBehaviour
{
    [SerializeField] private float horizontalRadius = 5f;
    [SerializeField] private float verticalRadius = 3f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool moveVertically = false;
    [SerializeField] private bool clockwise = true;
    
    private Vector3 centerPosition;
    private float angle;

    void Start()
    {
        centerPosition = transform.position;
    }

    void Update()
    {
        float deltaAngle = speed * Time.deltaTime;
        angle += clockwise ? deltaAngle : -deltaAngle;

        Vector3 newPosition = centerPosition;
        
        if (moveVertically)
        {
            newPosition.y += verticalRadius * Mathf.Sin(angle);
            newPosition.z += horizontalRadius * Mathf.Cos(angle);
        }
        else
        {
            newPosition.x += horizontalRadius * Mathf.Cos(angle);
            newPosition.z += verticalRadius * Mathf.Sin(angle);
        }

        transform.position = newPosition;

        if (moveVertically)
        {
            transform.rotation = Quaternion.LookRotation(
                Vector3.forward * Mathf.Cos(angle),
                Vector3.up * Mathf.Sin(angle)
            );
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(
                new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle))
            );
        }
    }

    public void SetRadii(float horizontal, float vertical)
    {
        horizontalRadius = horizontal;
        verticalRadius = vertical;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDirection(bool isClockwise)
    {
        clockwise = isClockwise;
    }
}