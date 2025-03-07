using NUnit.Framework;
using UnityEngine;

public static class Inventory
{
    public static int ammo = 1000;
    public static int laserEnergy = 1000;
    public static Weapon[,] weapons = new Weapon[10, 2];
}

public struct Weapon
{
    public bool weaponCollected;
    public string weaponName;
}