using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Objective : MonoBehaviour
{
    [SerializeField] float maxHp;
    [SerializeField] float maxFireHp;
    [SerializeField] float fireHeal;
    [SerializeField] float burningDmg;
    [SerializeField] float healSpeed;
    [SerializeField] float burnSpeed;
    [SerializeField] bool enemyKillzone;

    [Header("Fire Activation Variables")]
    [SerializeField] GameObject burnEffect;
    [SerializeField] GameObject healingEffect;
    [SerializeField] float rekindleFireDelay = 5f;

    [SerializeField] GameObject winCanvas;

    [SerializeField] GameObject waveManagerObject;
    [SerializeField] float delay = 5;

    public bool CanSpawn => isBurning;

    float currentHp;
    float fireHP;
    float fireTickHeal;
    float fireIntensityPercentageFactor;

    Vector3 maxBurnEffectScale;

    bool playersInZone;
    bool playerIn;
    bool enemyIn;
    bool isTakingBurningDmg;
    bool isHealing;
    bool isBurning;
    bool canRekindleFire = true;

    [Header("Fire Activation Variables")]
    [SerializeField] UnityEngine.Transform fireCircle;
    [SerializeField] float maxScale = 1f;
    [SerializeField] float requiredHoldTime = 5.0f;
    float holdTimer = 0f;

    WaveManager waveManager;
    AudioSource burningAudioSource;

    void Start()
    {
        winCanvas.SetActive(false);

        fireHP = 0;
        fireTickHeal = 0.1f;
        currentHp = maxHp;
        waveManager = waveManagerObject.GetComponent<WaveManager>();

        if (burnEffect != null)
            maxBurnEffectScale = burnEffect.transform.localScale * 1.5f;
        else Debug.LogError("BurnEffect GameObject is not assigned in the Inspector!", this);
    }

    void Update()
    {
        if (!playersInZone || isBurning || !Input.GetKey(KeyCode.E) || !canRekindleFire)
        {
            holdTimer = 0f;
            fireCircle.localScale = Vector3.zero;
            return;
        }

        holdTimer += Time.deltaTime;
        Debug.Log(holdTimer);
        float scale = Mathf.Clamp01(holdTimer / requiredHoldTime) * maxScale;
        fireCircle.localScale = new Vector3(scale, scale, scale);

        if (holdTimer >= requiredHoldTime)
        {
            burningAudioSource = AudioPlayer.Current.PlayLoopingSfx("Fire_Sound", transform.position);
            isBurning = true;
            fireHP = maxFireHp / 4;
            waveManager.StartContinuousWaves(gameObject.transform, new Wrapper<bool>(isBurning));
            StartCoroutine(RekindleFireTimmer());
        }
    }

    void FixedUpdate()
    {
        if (!isBurning)
        {
            if (currentHp < maxHp && !isHealing)
            {
                StartCoroutine(HealingHealth());
            }

            if (burnEffect != null && burnEffect.activeSelf)
            {
                burnEffect.SetActive(false);
            }
            return;
        }

        if (burnEffect != null && !burnEffect.activeSelf)
        {
            burnEffect.SetActive(true);
        }

        fireHP += fireTickHeal;
        fireHP = (fireHP > maxFireHp) ? maxFireHp : fireHP;
        fireIntensityPercentageFactor = fireHP / maxFireHp;

        burnEffect.transform.localScale = maxBurnEffectScale * fireIntensityPercentageFactor;

        if (isTakingBurningDmg) return;
        StartCoroutine(BurningTickDmg());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playersInZone = true;
        }
        if (other.tag == "Enemy" && enemyKillzone == false)
        {
            other.gameObject.GetComponent<EnemyStopFire>().SetInObjectiveZone(true, this.gameObject);
        }
        if (other.tag == "Enemy" && enemyKillzone == true)
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

        if (currentHp > 0)
        {
            currentHp -= burningDmg * fireIntensityPercentageFactor;
        }
        else
        {
            //Debug.Log("Died");
            StartCoroutine(WinGame());
            isTakingBurningDmg = false;
            if (burnEffect != null) burnEffect.SetActive(false);
            yield break; // Exit the coroutine
        }

        yield return new WaitForSeconds(burnSpeed);

        isTakingBurningDmg = false;
        burnEffect.SetActive(false);
    }

    IEnumerator HealingHealth()
    {
        isHealing = true;
        healingEffect.SetActive(true);

        currentHp += fireHeal;

        yield return new WaitForSeconds(healSpeed);

        isHealing = false;
        healingEffect.SetActive(false);
    }

    public bool GetIsBurning()
    {
        return isBurning;
    }

    public void FireExtinguish(float fireStoppingPower) {
        fireHP -= fireStoppingPower;
        if (fireHP <= 0)
        {
            isBurning = false;
            if (burningAudioSource)
            {
                AudioPlayer.Current.StopLoopingSfx(burningAudioSource); 
            }
        }
    }

    IEnumerator WinGame()
    {
        winCanvas.SetActive(true);
        yield return new WaitForSeconds(1);
        Loader.LoadNetwork(Loader.Scene.MenuScene);
    }
    
    IEnumerator RekindleFireTimmer()
    {
        canRekindleFire = false;
        yield return new WaitForSeconds(rekindleFireDelay);
        canRekindleFire = true;
    }
}
