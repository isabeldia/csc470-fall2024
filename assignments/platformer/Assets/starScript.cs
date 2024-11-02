using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class starScript : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Floating Settings")]
    [SerializeField] private float floatSpeed = 100f;
    [SerializeField] private float floatHeight = 10000f;
    
    [Header("Collection Settings")]
    [SerializeField] private string collectorTag = "Player"; 
    [SerializeField] private bool destroyOnCollect = true;  
    [SerializeField] private ParticleSystem collectEffect;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }