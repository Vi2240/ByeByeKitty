using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Objective : MonoBehaviour
{
    [SerializeField] float maxHp;
    [SerializeField] float heal;
    [SerializeField] float burningDmg;
    [SerializeField] float healSpeed;
    [SerializeField] float burnSpeed;

    [SerializeField] bool enemyKillzone;

    [SerializeField] GameObject burnEffect;
    [SerializeField] GameObject healingEffect;

    [SerializeField] GameObject winCanvas;

    [SerializeField] GameObject waveManagerObject;

    public bool CanSpawn => isBurning;

    float currentHp;

    bool playersInZone;
    bool playerIn;
    bool enemyIn;
    bool isTakingBurningDmg;
    bool isHealing;
    bool isBurning;

    AudioPlayer audioPlayer;
    WaveManager waveManager;

    void Start()
    {
        winCanvas.SetActive(false);

        currentHp = maxHp;
        waveManager = waveManagerObject.GetComponent<WaveManager>();
        audioPlayer = FindAnyObjectByType<AudioPlayer>();
    }

    void Update()
    {
        if(playersInZone)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isBurning)
            {
                isBurning = true;
                waveManager.StartWave(WaveType.WaveType0, SpawnType.AreaAroundPosition, gameObject.transform);
            }
        }
    }

    void FixedUpdate()
    {
        if(!isBurning && currentHp < maxHp && !isHealing)
        {
            StartCoroutine(HealingHealth());
        }
        if(isBurning && !isTakingBurningDmg)
        {
            StartCoroutine(BurningTickDmg());
            //audioPlayer.SfxPlayer("Fire_Sound");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playersInZone = true;
        }
        if (other.tag == "Enemy" && enemyKillzone == false)
        {
            isBurning = false;
        }
        if(other.tag == "Enemy" && enemyKillzone == true) 
        {
            Destroy(other.gameObject);
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

        if(currentHp > 0)
        {
            currentHp -= burningDmg;
        }
        else
        {
            //Debug.Log("Died");
            StartCoroutine(WinGame());
        }

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

    public bool GetIsBurning()
    { 
        return isBurning;
    }

    IEnumerator WinGame()
    {
        winCanvas.SetActive(true);
        yield return new WaitForSeconds(1);
        Loader.LoadNetwork(Loader.Scene.MenuScene);
    }
}
