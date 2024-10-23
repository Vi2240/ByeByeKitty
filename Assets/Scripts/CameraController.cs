using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject target;

    

    void Update()
    { 
        if(target != null)
        {
            this.gameObject.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10);
        }
    }
}
