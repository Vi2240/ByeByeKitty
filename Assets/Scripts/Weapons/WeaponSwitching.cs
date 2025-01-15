using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField] GameObject[] weapons;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchTo(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchTo(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchTo(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchTo(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchTo(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SwitchTo(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SwitchTo(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SwitchTo(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SwitchTo(9);
        }
    }

    void SwitchTo(int number)
    {
        foreach (var weapon in weapons) { weapon.SetActive(false); }
        if (weapons[number - 1])
        {
            weapons[number - 1].SetActive(true);
        }
    }
}
