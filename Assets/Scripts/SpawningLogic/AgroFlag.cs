using UnityEngine;

public class AgroFlag : MonoBehaviour
{
    [SerializeField] bool initialValue = false;
    public Wrapper<bool> agroFlag;
    
    void Start(){
        agroFlag = new Wrapper<bool>(initialValue);
    }    
}
