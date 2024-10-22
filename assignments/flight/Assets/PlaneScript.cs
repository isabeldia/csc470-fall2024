using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneScript : MonoBehaviour
{
    public GameObject cameraObject;
    float forwardSpeed = 0.25f;
    float xRotationSpeed = 0.2f;
    float yRotationSpeed = 0.2f;
    float decelerationRate = 0.001f; 
    float minSpeed = 0.01f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal"); // -1 if left is pressed, 1 if right is pressed, 0 if neither
        float vAxis = Input.GetAxis("Vertical"); // -1 if down is pressed, 1 if up is pressed, 0 if neither

        transform.Rotate(vAxis * xRotationSpeed, hAxis * yRotationSpeed, 0, Space.Self);
        forwardSpeed -= decelerationRate * Time.deltaTime;
        forwardSpeed = Mathf.Max(forwardSpeed, minSpeed);

        transform.position += transform.forward * forwardSpeed;

        //camera pos
        Vector3 cameraPosition = transform.position;
        cameraPosition += -transform.forward * 10f;
        cameraPosition += Vector3.up * 8f;
        cameraObject.transform.position = cameraPosition;
        cameraObject.transform.LookAt(transform.position);
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("collect"))
        {
            Destroy(other.gameObject);
            forwardSpeed += 0.05f;
        }
    }
}