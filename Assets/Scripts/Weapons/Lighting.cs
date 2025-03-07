using UnityEngine;
using System.Collections;

public class Lighting : MonoBehaviour
{
    [SerializeField] private float TTL_Seconds;
    
    void Start()
    {
        StartCoroutine(DashCooldown());
    }
 
    private IEnumerator DashCooldown()
    {        
        yield return new WaitForSeconds(TTL_Seconds);
        Destroy(gameObject);
    }
}
