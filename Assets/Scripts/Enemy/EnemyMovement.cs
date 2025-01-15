using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Serialize fields
    [SerializeField] float speed = 2f;
    //[SerializeField] float maxDistance = 3;
    [SerializeField] float movePauseMin = 2;
    [SerializeField] float movePauseMax = 6;
    [SerializeField] int checkIfStuckFrequency = 5;
    [SerializeField] float stopDistanceFromPlayer = 5;

    [SerializeField, Tooltip("0 = Nothing, 1 = Player, 2 = Objective")] 
    int targetRestriction = 0;

    [SerializeField, Tooltip("0 = Melee, 1 = Ranged")] 
    int attackType = 0;

    [SerializeField, Tooltip("Radius around to check if near objective or player.")] 
    float nearObjectCheckRadius = 5;

    [SerializeField, Tooltip("Wander around before becomign agressive.")] 
    bool randomWander = false;

    //[SerializeField] bool followPlayer = false;
    [SerializeField, Tooltip("If it's able to attack players.")] 
    bool canAttackPlayers = true; // Should be private later

    // Private variables 
    float randomWanderPrecision = 1f; // How close it has to be to its random wander target before walking somewhere else.
    bool canMove = true; // Needed for random wander
    bool objectiveIsBurning = false; // Used to make enemies instantly move after extinguising an objective
    bool isNearPlayer = false;
    bool isNearObjective;

    //Stuck stuff
    float timer = 0f;
    Vector2[] savedPositions;
    int arrayCount = 0;

    // Positions
    GameObject nearestPlayer;
    GameObject nearestObjective;
    Vector3 randomDestination;
    Vector2 target;
    NavMeshAgent agent;
    GameObject walkableArea;

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
        agent.speed = speed;
        savedPositions = new Vector2[checkIfStuckFrequency];
        walkableArea = GameObject.FindGameObjectWithTag("WalkableArea");

        // If it's a melee enemy it should go all the way to the player
        if (attackType == 0)
        {
            stopDistanceFromPlayer = 0.2f;
        }
        
        // Can't attack players if restrcted to objectives
        if (targetRestriction == 2)
        {
            canAttackPlayers = false;
        }

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
        nearestPlayer = FindNearestObject("Player");
        nearestObjective = FindNearestObject("Objective");

        NearObjectChecks();

        CheckIfStuck();

        if (BehaviourChecks()) { return; };
        
        RandomWanderCheck();
    }

    bool BehaviourChecks() // Returns true if the update function should return to skip code below
    {
        // If it's not resticted to players, it should prioritize the objective over the player if both are in range.
        if (isNearObjective && targetRestriction != 1)
        {
            if (nearestObjective == null) { return true; }

            Temporary script = nearestObjective.GetComponent<Temporary>();

            if (DistanceTo(nearestObjective) <= 5f)
            {
                agent.isStopped = true;
                //print("Stopped");
                // Do fire extinguish stuff
            }
            return true; // Return true so it doesn't access the code below.
        }

        // Go to objective if it's not near a player and not restricted to players, or if it's restricted to objectves. Only works if the objective is under attack
        if ((!isNearPlayer && targetRestriction != 1) || (targetRestriction == 2))
        {
            if (nearestObjective != null)
            {
                MoveToDestination(nearestObjective.transform.position);
            }
        }

        if (canAttackPlayers && DistanceTo(nearestPlayer) <= stopDistanceFromPlayer) // If the distance is closer than the stop distance to the closest player
        {
            agent.isStopped = true;
            // Attack logic here
            return true;
        }
        else if ((canAttackPlayers && isNearPlayer) || (targetRestriction == 1))
        {
            MoveToDestination(nearestPlayer.transform.position); // FindNearestlayerPosition returns a GameObject, which the enemy moves to.
            return true;
        }

        return false;
    }

    void CheckIfStuck()
    {
        timer += Time.deltaTime;

        if (timer >= 1f) // 1 second delay
        {
            // Save the current position in the array
            savedPositions[arrayCount] = gameObject.transform.position;
            arrayCount++;
            timer = 0f;

            if (arrayCount < checkIfStuckFrequency) // 0 to 9 (10 elements)
            {
                return; // Return if 10 positions haven't been saved yet
            }

            // Calculate the average position after saving 10 positions
            Vector2 averagePosition = Vector2.zero;

            // Add all saved positions together
            for (int i = 0; i < savedPositions.Length; i++)
            {
                averagePosition += savedPositions[i];
            }

            // Divide by the array length to get the average
            averagePosition /= savedPositions.Length;

            // Compare the average position with the current position
            if (Vector2.Distance(averagePosition, gameObject.transform.position) < 0.1f)
            {
                print("Enemy is stuck! Setting a new destination.");
                SetNewRandomDestination();
                MoveToDestination(randomDestination);
            }

            arrayCount = 0; // Reset the position tracking
        }
    }

    void NearObjectChecks()
    {
        if (DistanceTo(nearestPlayer) <= nearObjectCheckRadius) { isNearPlayer = true; }
        else
        {
            isNearPlayer = false;
            if (targetRestriction != 1) // Find objective if not restricted to only players.
            {
                GameObject objective = nearestObjective;
                if (objective != null)
                {
                    MoveToDestination(objective.transform.position);
                }
            }
        }

        if (nearestObjective != null)
        {
            if (DistanceTo(nearestObjective) <= nearObjectCheckRadius) { isNearObjective = true; }
            else
            {
                isNearObjective = false;
            }
        }
    }

    void RandomWanderCheck()
    {
        if (!randomWander) // For testing when you want it to follow the mouse
        {
            // Follow mouse stuff for testing
            //Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            //target = new Vector2(worldPosition.x, worldPosition.y);
            //MoveToDestination(target);
            return;
        }

        // If it's close enough to its target, it will set a new target. 
        if (agent.remainingDistance <= randomWanderPrecision && canMove)
        {
            canMove = false;
            float waitTime = Random.Range(movePauseMin, movePauseMax);
            //print($"Should move in {waitTime} seconds");
            StartCoroutine(WaitThenMove(waitTime));
            return;
        }
    }

    /// <summary>
    /// Takes a string of a tag and finds the nearest object with that tag and returns it.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public GameObject FindNearestObject(string tag)
    {
        List<GameObject> objects = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag)); // Have to do this check every time in case a player disconnects.

        if (tag == "Objective") // Check each objective if it's under attack or not. If it's not under attack, the enemy should not go there.
        {
            for (int i = objects.Count - 1; i >= 0; i--) // Can't use foreach here, might cause errors when removing objects from the list.
            {
                GameObject _object = objects[i];
                Objective script = _object.GetComponent<Objective>(); // Change to objective class later

                if (!script.GetIsBurning())
                {
                    objects.Remove(_object);
                }
            }

            if (objects.Count <= 0) // If there are no burning bjectives
            {
                if (randomWander && objectiveIsBurning && canMove)
                {
                    StartCoroutine(WaitThenMove(1));
                    objectiveIsBurning = false;
                    canMove = false;
                }
                return null;
            }
            else
            {
                objectiveIsBurning = true;
            }
        }

        GameObject nearestGameObject = null; // Need to set it to null so it has a value, otherwise it doesn't want to return it.
        float smallestDistance = Mathf.Infinity;
        Vector2 currentPosition = transform.position; // The position of this object (enemy).

        foreach (GameObject _object in objects) // Iterate over the list of players.
        {
            float distanceToPlayer = Vector2.Distance(currentPosition, _object.transform.position); // Get distance away from this player.

            if (distanceToPlayer < smallestDistance)
            {
                smallestDistance = distanceToPlayer; // Update the closest distance to keep track of which was closest.
                nearestGameObject = _object; // If it is closest, save it.
            }
        }

        return nearestGameObject;
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
        if (destination != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
    }

    IEnumerator WaitThenMove(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        SetNewRandomDestination();
        MoveToDestination(randomDestination);
        canMove = true;
    }

    public float DistanceTo(GameObject _object) // Public so it can be used in other scripts
    {
        return Vector2.Distance(gameObject.transform.position, _object.transform.position);
    }
}