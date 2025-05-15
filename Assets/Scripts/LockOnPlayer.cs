using UnityEngine;

public class LockOnPlayer : MonoBehaviour
{
    Transform player;
    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            return;
        }
        transform.position = player.position;
    }
}
