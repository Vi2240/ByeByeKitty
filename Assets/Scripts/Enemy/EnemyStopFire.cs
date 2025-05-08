using UnityEngine;
using System.Collections; // IEnumerator
using System.Collections.Generic; // List

public class EnemyStopFire : MonoBehaviour
{
    class Zone{
        public bool isInZone;
        public GameObject gameObject;
        public bool onCooldown = false;
        public bool initialCooldownDone = false;
        public Zone(bool isInZone, GameObject gameObject){ this.isInZone = isInZone; this.gameObject = gameObject; }
        public IEnumerator startCooldown(float extinguishCooldownTime, float initialCooldownTime) 
        {
            onCooldown = true;
            if (!initialCooldownDone)
                { yield return new WaitForSeconds(extinguishCooldownTime); initialCooldownDone = true; }                    

            yield return new WaitForSeconds(extinguishCooldownTime); 
            onCooldown = false; 
        }        
    }
    List<Zone> zones = new List<Zone>();
    [SerializeField] float fireStoppingPower;
    [SerializeField] float extinguishCooldownTime;
    [SerializeField] float initialCooldownTime;
    
    void FixedUpdate() { StopFire(); }
    
    public void SetInObjectiveZone(bool isInZone, GameObject gameObject){    
        foreach (Zone zone in zones)
            if (zone.gameObject == gameObject) { zone.isInZone = isInZone; zone.initialCooldownDone = false; break; }            
        zones.Add(new Zone(isInZone, gameObject));            
    }

    private void StopFire(){
        foreach (Zone zone in zones)
            if (zone.isInZone && !zone.onCooldown) {
                zone.gameObject.GetComponent<Objective>().StopFire(fireStoppingPower); 
                StartCoroutine(zone.startCooldown(extinguishCooldownTime, initialCooldownTime));
            }
    }
}
