using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField] GameObject[] weapons;
    [SerializeField] string[] weaponNames;
    [SerializeField] GameObject[] weaponItems;

    List<GameObject> collectedWeapons = new List<GameObject>();
    string activeWeapon;

    void Start()
    {
        collectedWeapons.Add(weapons[0]);

        foreach (GameObject weapon in weapons) { collectedWeapons.Add(weapon); } // For testing to unlock all weapons at start. Remove later.

        activeWeapon = weaponNames[0];
        SwitchTo(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SwitchTo(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SwitchTo(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SwitchTo(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SwitchTo(3); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SwitchTo(4); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SwitchTo(5); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SwitchTo(6); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SwitchTo(7); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { SwitchTo(8); }
        if (Input.GetKeyDown(KeyCode.Q)) { DropCurrentWeapon(); }
    }

    public bool CollectWeapon(string weaponName)
    {
        for (int i = 0; i < weaponNames.Length; i++)
        {
            if (weaponNames[i] == weaponName)
            {
                if (collectedWeapons.Contains(weapons[i])) { return false; }
                if (collectedWeapons.Count >= weapons.Length) { return false; }
                print("Collected: " + weaponName);
                collectedWeapons.Add(weapons[i]);
                return true;
            }
        }
        return false;
    }

    void SwitchTo(int index)
    {
        foreach (var weapon in collectedWeapons) { weapon.SetActive(false); }

        if (index < collectedWeapons.Count)
        {
            collectedWeapons[index].SetActive(true);
            for (int i = 0; i < weapons.Length; i++)
            {
                if (collectedWeapons[index] == weapons[i])
                {
                    activeWeapon = weaponNames[i];
                    break;
                }
            }
        }
    }

    void DropCurrentWeapon()
    {
        for (int i = 0; i < weaponNames.Length; i++)
        {
            if (weaponNames[i] == activeWeapon)
            {
                Vector3 offset = new Vector3(0, 2, 0);
                Instantiate(weaponItems[i], transform.position + offset, Quaternion.identity);
                break;
            }
        }
    }
}