using UnityEngine;
using System.Collections;

public class Lighting : MonoBehaviour
{
    [SerializeField] private float TTL_Seconds;
    [SerializeField] GameObject lightningStrikePrefab;  // Prefab for the ligtning hit visual


    public void Initialize(Vector3 endPosition)
    {
        gameObject.SetActive(true);
        Instantiate(lightningStrikePrefab, endPosition, Quaternion.identity);
        StartCoroutine(DashCooldown());
    }


    private IEnumerator DashCooldown()
    {        
        yield return new WaitForSeconds(TTL_Seconds);
        Destroy(gameObject);
    }
}
