using UnityEngine;

[CreateAssetMenu(fileName = "WeaponLvlAndXpScriptableObjectScript", menuName = "Scriptable Objects/WeaponLvlAndXpScriptableObjectScript")]
public class WeaponScriptableObjectScript : ScriptableObject
{
    [Header("Lvl&Xp")]
    [SerializeField] string weaponName;
    [SerializeField] string weaponLevel;
    [SerializeField] string weaponCurrentXp;
    [SerializeField] string weaponMaxXp;
    [SerializeField] string weaponMaxXpScale;

    [Header("OtherWeaponStats")]
    [SerializeField] float damge;
    [SerializeField] float fireRate;
    [SerializeField] int maxAmmo;
}
