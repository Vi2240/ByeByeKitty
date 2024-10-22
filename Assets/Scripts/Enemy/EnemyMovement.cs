using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] bool randomWander = false;
    [SerializeField] float speed = 0.8f;
    [SerializeField] float range = 0.01f;
    [SerializeField] float maxDistance = 3;
    [SerializeField] float movePauseMin = 2;
    [SerializeField] float movePauseMax = 6;
    [SerializeField] GameObject walkableArea;
    [SerializeField] bool followPlayer = false;

    float precision = 1f;
    bool canMove = true;
    bool isNearPlayer = false;
    [SerializeField] bool isAgressive = true;
    Vector3 randomDestination;
    Vector2 target;
    NavMeshAgent agent;

    // Constructor
    public EnemyMovement(bool wander)
    {
        this.randomWander = wander;
    }

    void Start()
    {
        canMove = true;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (randomWander && canMove)
        {
            SetNewRandomDestination();
            MoveToDestination(randomDestination);
        }
        else
        {
            MoveToDestination(target);
        }
    }

    void Update()
    {
        if (isNearPlayer && isAgressive)
        {
            MoveToDestination(FindNearestPlayerPosition()); // FindNearestlayerPosition returns a Vector2, which the enemy moves to.
            return; // Return so it doesn't access the code below when a player is nearby.
        }

        WanderCheck();
    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.CompareTag("Player")) { isNearPlayer = true; }
    }

    private void OnTriggerExit2D(Collider2D trigger)
    {
        if (trigger.CompareTag("Player")) 
        {
            isNearPlayer = false;
            if (isAgressive)
            {
                MoveToDestination(gameObject.transform.position);
            }
        }
    }

    void WanderCheck()
    {
        if (randomWander)
        {
            // If it's close enough to its target, it will set a new target. 
            if (!agent.pathPending && agent.remainingDistance <= precision)
            {
                if (canMove)
                {
                    canMove = false;
                    float waitTime = Random.Range(movePauseMin, movePauseMax);
                    print($"Should move in {waitTime} seconds");
                    StartCoroutine(WaitThenMove(waitTime));
                    return;
                }
            }
        }
        else
        {
            //Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            //target = new Vector2(worldPosition.x, worldPosition.y);
            ////print(target); // Test if it finds the mouse position
            //MoveToDestination(target);
        }
    }

    Vector2 FindNearestPlayerPosition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Have to do this check every time in case a player disconnects.

        GameObject nearestPlayer = null; // Need to set it to null so it has a value, otherwise it doesn't want to return it.
        float smallestDistance = Mathf.Infinity;
        Vector2 currentPosition = transform.position;  // The position of this object (enemy).

        foreach (GameObject player in players) // Iterate over the list of players.
        {
            float distanceToPlayer = Vector2.Distance(currentPosition, player.transform.position); // Get distance away from this player.

            if (distanceToPlayer < smallestDistance)
            {
                smallestDistance = distanceToPlayer; // Update the closest distance to keep track of which was closest.
                nearestPlayer = player; // If it is closest, save it.
            }
        }

        return nearestPlayer.transform.position;
    }

    void SetNewRandomDestination()
    {
        if (walkableArea != null)
        {
            // Get the bounds of the walkable area
            Bounds bounds = walkableArea.GetComponent<SpriteRenderer>().bounds;

            // Generate a random point within the bounds of the walkable area
            Vector2 randomPoint = (Vector2)bounds.center + Random.insideUnitCircle * bounds.extents;

            // Clamp the random point to stay within the bounds of the walkable area
            randomPoint.x = Mathf.Clamp(randomPoint.x, bounds.min.x, bounds.max.x);
            randomPoint.y = Mathf.Clamp(randomPoint.y, bounds.min.y, bounds.max.y);

            randomDestination = randomPoint;
        }
    }

    void MoveToDestination(Vector2 destination)
    {
        agent.SetDestination(destination);
    }

    IEnumerator WaitThenMove(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        SetNewRandomDestination();
        MoveToDestination(randomDestination);
        canMove = true;
    }
}