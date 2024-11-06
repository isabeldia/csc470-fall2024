using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDown : MonoBehaviour
{
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private bool startAtRandom = false;

    private float startY;
    private float time;

    void Start()
    {
        startY = transform.position.y;
        
        if (startAtRandom)
        {
            time = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        float newY = startY + amplitude * Mathf.Sin(frequency * time);
        
        Vector3 newPosition = transform.position;
        newPosition.y = newY;
        transform.position = newPosition;
    }

    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }

    public void SetFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }
}