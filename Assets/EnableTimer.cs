using System.Collections; // Added this for IEnumerator
using System.Collections.Generic;
using UnityEngine;

public class EnableTimer : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToEnable;
    [SerializeField] float timer = 2f;

    void Start()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        StartCoroutine(EnableAfter(timer));
    }

    IEnumerator EnableAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}