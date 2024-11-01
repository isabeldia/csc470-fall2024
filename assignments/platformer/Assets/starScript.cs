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
    [SerializeField] private string collectorTag = "Player"; // Tag of object that can collect the star
    [SerializeField] private bool destroyOnCollect = true;   // Whether to destroy or just hide the star
    [SerializeField] private ParticleSystem collectEffect;   // Optional: particle effect for collection
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Rotate around Z-axis
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        
        // Gentle floating motion
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the correct tag
        if (other.CompareTag(collectorTag))
        {
            CollectStar();
        }
    }
    
    private void CollectStar()
    {
        // Play particle effect if one is assigned
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        // You can add sound effects here
        // AudioSource.PlayClipAtPoint(collectSound, transform.position);
        
        // You can add score or other game mechanics here
        // GameManager.Instance.AddScore(10);
        
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}