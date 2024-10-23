using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Objective : MonoBehaviour
{
    [SerializeField] float maxHp;
    [SerializeField] float heal;
    [SerializeField] float burningDmg;
    [SerializeField] float healSpeed;
    [SerializeField] float burnSpeed;

    [SerializeField] GameObject burnEffect;
    [SerializeField] GameObject healingEffect;

    float currentHp;

    bool playersInZone;
    bool playerIn;
    bool enemyIn;
    bool isTakingBurningDmg;
    bool isHealing;
    bool isBurning;

    void Start()
    {
        currentHp = maxHp;
    }

    void Update()
    {
        if(playersInZone)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isBurning = true;
            }
        }
    }

    void FixedUpdate()
    {
        if(!isTakingBurningDmg)
        {
            StartCoroutine(BurningTickDmg());
        }
        if(!isBurning && currentHp < maxHp && !isHealing)
        {
            StartCoroutine(HealingHealth());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playersInZone = true;
        }
        if (other.tag == "Enemy")
        {
            isBurning = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playersInZone = false;
        }
    }

    IEnumerator BurningTickDmg()
    {
        isTakingBurningDmg = true;
        burnEffect.SetActive(true);

        currentHp -= burningDmg;

        yield return new WaitForSeconds(burnSpeed);

        isTakingBurningDmg = false;
        burnEffect.SetActive(false);
    }

    IEnumerator HealingHealth()
    {
        isHealing = true;
        healingEffect.SetActive(true);

        currentHp += heal;

        yield return new WaitForSeconds(healSpeed);

        isHealing = false;
        healingEffect.SetActive(false);
    }

}
