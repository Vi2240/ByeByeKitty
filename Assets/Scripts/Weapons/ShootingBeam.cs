using System.Collections;
using TMPro;
using UnityEngine;

public class ShootingBeam : WeaponBase
{
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmo;
    [SerializeField] GameObject damageNumber;
    [SerializeField] float damagePerTick = 15f;
    [SerializeField] float beamLength = 5f;

    private LineRenderer lineRenderer;
    private bool beamActive = false;
    private float savedBeamLength;

    protected override void Start()
    {
        base.Start();
        savedBeamLength = beamLength;
        lineRenderer = GetComponentInChildren<LineRenderer>();

        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmo = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmo.SetText("");
        magCapacityText.SetText(Inventory.laserEnergy.ToString());
    }


    // Switch the text in UI to match this weapon when it is enabled in different scripts
    private void OnEnable()
    {
        //audioPlayer.SfxPlayer("RevolverReload_Sound");
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmo = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmo.SetText("");
        magCapacityText.SetText(Inventory.laserEnergy.ToString());
    }


    protected override void Update()
    {
        base.Update();
        if (beamActive)
        {
            BeamLogic();
            DrawBeam();
        }

        if (Input.GetMouseButtonDown(0))
        {
            beamActive = true;
            lineRenderer.enabled = true;
            DrawBeam();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            beamActive = false;
            lineRenderer.enabled = false;
        }
    }

    protected override void Fire() { /* Not used in beam */ }


    private void BeamLogic()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(projectileSpawnLocation.position, transform.right, beamLength);
        RaycastHit2D validHit = new RaycastHit2D();
        float closest = beamLength;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !hit.collider.CompareTag("Objective") && hit.distance < closest)
            {
                validHit = hit;
                closest = hit.distance;
            }
        }

        if (validHit.collider == null)
        {
            beamLength = savedBeamLength;
        }
        else
        {
            beamLength = closest;
            if (validHit.collider.CompareTag("Enemy") && canFire)
            {
                HitEnemy(validHit);
            }
        }
    }

    private void HitEnemy(RaycastHit2D hit)
    {
        StartCoroutine(ShootCooldown());
        hit.collider.gameObject.GetComponent<EnemyHealth>().TakeDamage(damagePerTick);
        Instantiate(damageNumber, hit.transform.position, Quaternion.identity).GetComponent<FloatingHealthNumber>().SetText(damagePerTick.ToString());
    }

    private void DrawBeam()
    {
        lineRenderer.SetPosition(0, projectileSpawnLocation.position);
        lineRenderer.SetPosition(1, projectileSpawnLocation.position + projectileSpawnLocation.right * beamLength);
    }


    // Work on more later if there's time
    //void SplitTotargets(int amount)
    //{
    //    if (amount <= 0)
    //    {
    //        return;
    //    }

    //    // Find all GameObjects with the tag "Enemy" and sort the list based on distance to get the splitAmount closest enemies.
    //    List<GameObject> enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
    //    enemies.Sort((a, b) =>
    //        UnityEngine.Vector2.Distance(transform.position, a.transform.position)
    //        .CompareTo(UnityEngine.Vector2.Distance(transform.position, b.transform.position)));

    //    for (int i = 0; i < splitAmount; i++)
    //    {
    //        if (enemies[i] != null)
    //        {
    //            enemies[i].SetActive(false);
    //        }
    //        print("Set to false");
    //    }

    //}

    //Checks that the cursor isn't inside the Shoot Range collider
}