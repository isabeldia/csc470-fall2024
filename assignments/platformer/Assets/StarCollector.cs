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
            ScoreDisplay.AddPoints(starPoints);
            
            Destroy(other.gameObject);
        }
    }
}