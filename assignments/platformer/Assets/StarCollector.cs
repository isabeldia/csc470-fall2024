using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCollector : MonoBehaviour
{
    public int starPoints = 100;
    public string starTag = "Star";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(starTag))
        {
            // Add points using the static method from ScoreDisplay
            ScoreDisplay.AddPoints(starPoints);
            
            // Destroy the star
            Destroy(other.gameObject);
        }
    }
}