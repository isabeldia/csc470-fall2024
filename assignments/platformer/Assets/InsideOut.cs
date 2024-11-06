using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideOut : MonoBehaviour
{
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private bool startAtRandom = false;

    private float startZ;
    private float time;

    void Start()
    {
        startZ = transform.position.z;
        
        if (startAtRandom)
        {
            time = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        float newZ = startZ + amplitude * Mathf.Sin(frequency * time);
        
        Vector3 newPosition = transform.position;
        newPosition.z = newZ;
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