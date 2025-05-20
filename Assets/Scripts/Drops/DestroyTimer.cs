using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    [SerializeField, Tooltip("Match this to particle effect's duration.")] float destroyDelay = 1f;
    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
