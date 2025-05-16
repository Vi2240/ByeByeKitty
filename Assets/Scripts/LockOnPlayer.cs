using UnityEngine;

public class LockOnPlayer : MonoBehaviour
{
    Transform player;
    void Update()
    {
        if (player == null)
        {
            try
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            }
            catch {}
            return;
        }
        transform.position = player.position;
    }
}
