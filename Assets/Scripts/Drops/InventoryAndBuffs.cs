using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryAndBuffs
{
    // -- Inventory --
    public static int ammo = 500;
    public static int maxAmmo = 1000;
    public static int energyAmmo = 500;
    public static int maxEnergyAmmo = 1000;


    // -- Player buffs --
    public static float playerDamageMultiplier = 1f;
    public static float playerHealthMultiplier = 1f;
    public static float playerSpeedMultiplier = 1f;


    // -- Enemy buffs --
    public static float enemyDamageMultiplier = 1f;
    public static float enemyHealthMultiplier = 1f;
    public static float enemySpeedMultiplier = 1f;


    // -- Weapon collection --
    public static List<string> collectedAndDroppedWeapons = new List<string>();
    public static void ResetCollectedWeapons() { collectedAndDroppedWeapons.Clear(); }
}