using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private float timer;
    private Coroutine movementCoroutine; // yields execution
    private bool isInitialized = false;
    
    private float minWanderDistance = 5f;
    private float maxWanderDistance = 15f;
    private float minWanderWaitTime = 2f;
    private float maxWanderWaitTime = 5f;
    
    [Header("Movement")]
    public float normalSpeed = 3.5f;
    public float sprintSpeed = 7f;
    public float chanceToSprint = 0.3f;
    
    private bool useRandomPatterns = true;
    private float circleRadius = 5f;
    private int circleSegments = 8;
    
    private float minX = -10f;
    private float maxX = 10f;
    private float minZ = -10f;
    private float maxZ = 10f;

    void Awake()
    {
        InitializeAgent();
    }

    private BoxCollider enemyCollider;
    
    private void InitializeAgent()
    {
        if (!isInitialized)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            
            enemyCollider = GetComponent<BoxCollider>();
            if (enemyCollider == null)
            {
                enemyCollider = gameObject.AddComponent<BoxCollider>();
            }
            enemyCollider.isTrigger = true;
            enemyCollider.size = new Vector3(1f, 1f, 1f);
            
            gameObject.tag = "enemies";
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            agent.speed = normalSpeed;
            agent.acceleration = normalSpeed * 2;
            agent.angularSpeed = 360f;
            isInitialized = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Triggered: {other.gameObject.name}, Tag: {other.tag}");
        if (other.CompareTag("unit"))
        {
            Debug.Log("Enemy triggered  collision with player! Game over!!");
            GameManager.Instance.GameOver();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision: {collision.gameObject.name}, Tag: {collision.gameObject.tag}"); 
        if (collision.gameObject.CompareTag("unit"))
        {
            Debug.Log("Enemy collided with player! Game over!! ");
            GameManager.Instance.GameOver();
        }
    }

    void OnEnable()
    {
        InitializeAgent();
        
        if (agent != null)
        {
            agent.enabled = true;
            StartCoroutine(DelayedStart());
        }
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        StartNewMovement();
    }
    
    // enemies would start moving then abruptly stop. this makes them active until end
    private void StartNewMovement()
    {
        if (!isInitialized || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name} not ready for movement");
            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        if (useRandomPatterns && Random.value < 0.3f)
        {
            if (Random.value < 0.5f)
            {
                movementCoroutine = StartCoroutine(CircleMovement());
            }
            else
            {
                movementCoroutine = StartCoroutine(ZigZagMovement());
            }
        }
        else
        {
            SetNewRandomDestination();
            timer = Random.Range(minWanderWaitTime, maxWanderWaitTime);
            agent.speed = (Random.value < chanceToSprint) ? sprintSpeed : normalSpeed;
        }
    }

    private void Update()
    {
        if (!isInitialized || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (!agent.hasPath || agent.remainingDistance < 0.1f || 
            (timer <= 0f && !useRandomPatterns))
        {
            StartNewMovement();
        }

        if (!useRandomPatterns)
        {
            timer -= Time.deltaTime;
        }
    }
    
    private IEnumerator CircleMovement()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            yield break;

        Vector3 centerPoint = transform.position;
        float angleStep = 360f / circleSegments;
        
        for (int i = 0; i <= circleSegments; i++)
        {
            if (!agent.enabled || !agent.isOnNavMesh)
                yield break;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 targetPos = centerPoint + new Vector3(
                Mathf.Cos(angle) * circleRadius,
                0,
                Mathf.Sin(angle) * circleRadius
            );
            
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX); // bounds
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ); // bounds
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 100f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                
                while (agent.pathStatus == NavMeshPathStatus.PathPartial ||
                       agent.remainingDistance > 0.1f)
                {
                    if (!agent.enabled || !agent.isOnNavMesh)
                        yield break;
                    yield return null;
                }
            }
        }
        
        StartNewMovement();
    }
    
    private IEnumerator ZigZagMovement()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            yield break;

        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y = 0;
        randomDir.Normalize();
        
        for (int i = 0; i < 5; i++)
        {
            if (!agent.enabled || !agent.isOnNavMesh)
                yield break;

            Vector3 targetPos = transform.position + randomDir * Random.Range(minWanderDistance/2, maxWanderDistance/2);
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 100f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                
                while (agent.pathStatus == NavMeshPathStatus.PathPartial ||
                       agent.remainingDistance > 0.1f)
                {
                    if (!agent.enabled || !agent.isOnNavMesh)
                        yield break;
                    yield return null;
                }
            }
            
            randomDir = Quaternion.Euler(0, Random.Range(120f, 240f), 0) * randomDir;
        }
        
        StartNewMovement();
    }
    
    private void SetNewRandomDestination()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minWanderDistance, maxWanderDistance);
        randomDirection += transform.position;
        
        randomDirection.x = Mathf.Clamp(randomDirection.x, minX, maxX);
        randomDirection.z = Mathf.Clamp(randomDirection.z, minZ, maxZ);
        randomDirection.y = transform.position.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 100f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmosSelected() //debug illustration
    {
        Gizmos.color = Color.yellow; //wireframe yellow
        Vector3 center = new Vector3((maxX + minX) / 2f, transform.position.y, (maxZ + minZ) / 2f); //mid point of movement
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ); // size of wireftame box
        Gizmos.DrawWireCube(center, size); // visual
    }
}