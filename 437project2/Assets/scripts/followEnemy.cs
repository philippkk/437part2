using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class followEnemy : MonoBehaviour
{
    public bool isPatroller = false;
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;
    private Transform currentPatrolPoint;
    public bool isGuard = false;
    public Transform guardPoint;
    public GameObject player;
    public NavMeshAgent nav;
    public Material angryMaterial;
    public Material normalMaterial;
    public Material hitMaterial;
    public int health = 3;
    public int agroDistance = 5;
    public float wanderRadius = 5f;
    public float wanderTime = 5f;
    private float timer = 0f;
    private float hitTimer = 0.5f;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (isPatroller)
        {
            gameController gc = GameObject.Find("gameController").GetComponent<gameController>();
            patrolPoints = gc.patrolPoints;
            currentPatrolIndex = Random.Range(0, patrolPoints.Count);
        }
        if (isGuard)
        {
            gameController gc = GameObject.Find("gameController").GetComponent<gameController>();
            guardPoint = GameObject.Find("guardPoint").transform;
        }
        GetComponent<Renderer>().material = normalMaterial;
    }

    void Update()
    {
        hitTimer -= Time.deltaTime;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < agroDistance)
        {
            // GetComponent<Renderer>().material = angryMaterial;
            nav.SetDestination(player.transform.position);
        }
        else
        {
            if (isPatroller)
            {
                patrol();
            }
            else if (isGuard)
            {
                guard();
            }
            else
            {
                wander();
            }
        }

        if (health <= 0)
        {
            gameController gc = GameObject.Find("gameController").GetComponent<gameController>();
            gc.enemiesList.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    public void HitEnemy(int damage)
    {
        if (hitTimer < 0)
        {
            Debug.Log("Hit");
            hitTimer = 0.5f;
            health -= damage;
            GetComponent<Renderer>().material = hitMaterial;
            Invoke("ResetMaterial", 0.5f);
        }

    }

    void ResetMaterial()
    {
        if (health > 0)
        {
            GetComponent<Renderer>().material = normalMaterial;
        }
    }

    void wander()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GetComponent<Renderer>().material = normalMaterial;
            timer = wanderTime;
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);
            nav.SetDestination(hit.position);
        }
    }

    void patrol()
    {
        if (patrolPoints.Count > 0)
        {
            currentPatrolPoint = patrolPoints[currentPatrolIndex];
            nav.SetDestination(currentPatrolPoint.position);
            if (Vector3.Distance(transform.position, currentPatrolPoint.position) < 1f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            }
        }
    }

    void guard()
    {
        if (guardPoint != null)
        {
            nav.SetDestination(guardPoint.position);
        }
        //toodo: always look at the player
        Vector3 direction = player.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }
}
