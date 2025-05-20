using UnityEngine;
using System.Collections; // IEnumerator
using System.Collections.Generic; // List

public class EnemyStopFire : MonoBehaviour
{
    class Zone{
        public bool isInZone;
        public GameObject targetGameObject;
        public bool onCooldown = false;
        public bool initialCooldownDone = false;

        public Zone(bool isInZone, GameObject targetGameObject)
        {
            this.isInZone = isInZone;
            this.targetGameObject = targetGameObject;
        }
        
        public IEnumerator startCooldown(float extinguishTime, float initialTime )
        {
            onCooldown = true;

            if (!initialCooldownDone)
            {
                if (initialTime > 0)
                {
                    yield return new WaitForSeconds(initialTime);
                }
                initialCooldownDone = true;
            }

            yield return new WaitForSeconds(extinguishTime);
            onCooldown = false;
        }        
    }

    List<Zone> zones = new List<Zone>();
    [SerializeField] float fireStoppingPower;
    [SerializeField] float extinguishCooldownTime;
    [SerializeField] float initialCooldownTime;
    
    void FixedUpdate() { ExtinguishFire(); }
    
    public void SetInObjectiveZone(bool isInZone, GameObject targetGameObject){
        foreach (Zone zone in zones)
            if (zone.targetGameObject == targetGameObject) { zone.isInZone = isInZone; zone.initialCooldownDone = false; break; }            
        zones.Add(new Zone(isInZone, targetGameObject));            
    }
    
    private void ExtinguishFire()
    {
        foreach (Zone zone in zones)
            if (zone.isInZone && !zone.onCooldown)
            {
                zone.targetGameObject.GetComponent<Objective>().FireExtinguish(fireStoppingPower);
                StartCoroutine(zone.startCooldown(extinguishCooldownTime, initialCooldownTime));
            }
    }
}
