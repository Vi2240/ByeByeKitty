using UnityEngine;
using System.Collections; // IEnumerator
using System.Collections.Generic; // List

public class EnemyStopFire : MonoBehaviour
{
    public class Zone
    {
        public bool isInZone;
        public Objective targetObjective;
        public bool onCooldown = false;
        public bool initialCooldownDone = false;
        public Wrapper<bool> isBurning;

        public Zone(bool isInZone, Objective targetObjective)
        {
            this.isInZone = isInZone;
            this.targetObjective = targetObjective;
        }

        public IEnumerator StartCooldown(float extinguishTime, float initialTime)
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

    public Zone zone = new Zone(false, null);
    public bool canExtinguish = true;
    [SerializeField] float fireStoppingPower;
    [SerializeField] float extinguishCooldownTime;
    [SerializeField] float initialCooldownTime;

    void FixedUpdate()
    {        
        ExtinguishFire();
    }

    public void SetInObjectiveZone(bool isInZone, GameObject targetObjectiveObj)
    {
        zone.isInZone = isInZone;
        zone.initialCooldownDone = false;
        try { zone.targetObjective = targetObjectiveObj.GetComponent<Objective>(); }
        catch { Debug.Log($"GameObject {targetObjectiveObj} dosn't contain any component \"Objective\""); }
        if (zone.targetObjective != null)
            zone.isBurning = zone.targetObjective.GetIsBurning();
    }

    private void ExtinguishFire()
    {
        if (!zone.isInZone || zone.onCooldown || zone.targetObjective == null || !canExtinguish) return;
        
        zone.targetObjective.FireExtinguish(fireStoppingPower);
        StartCoroutine(zone.StartCooldown(extinguishCooldownTime, initialCooldownTime));                
    }
}
