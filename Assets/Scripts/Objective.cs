using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Objective : MonoBehaviour
{

    [Header("Tree Variables")]
    [SerializeField] float maxHp;
    [SerializeField] float healSpeed;
    [SerializeField] float treeHeal;
    [SerializeField] bool enemyKillzone;
    float currentHp;

    [Header("Fire Variables")]
    [SerializeField] float maxFireHp;
    [SerializeField] float fireHeal;
    [SerializeField] float fireHealSpeed;
    [SerializeField] float burningDmg;
    [SerializeField] float burnSpeed;
    [SerializeField] bool canExtinguishFire;
    [SerializeField] float BurnEffectScaleMult = 1;
    float fireIntensityPercentageFactor;
    float fireHP;
    Vector3 maxBurnEffectScale_Vec;

    [Header("Fire Activation Variables")]
    [SerializeField] GameObject burnEffect;
    [SerializeField] GameObject healingEffect;
    [SerializeField] float rekindleFireDelay = 5f;

    [SerializeField] GameObject winCanvas;

    [SerializeField] GameObject waveManagerObject;
    [SerializeField] float delay = 5;

    public bool CanSpawn => isBurning.value;

    bool playersInZone;
    bool playerIn;
    bool enemyIn;
    bool isTakingBurningDmg;
    bool isGrowingFire;
    bool isHealing;
    Wrapper<bool> isBurning;
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
        currentHp = maxHp;
        isBurning = new Wrapper<bool>(false);
        fireIntensityPercentageFactor = 0f;
        isTakingBurningDmg = false;
        isGrowingFire = false;
        isHealing = false;
        waveManager = waveManagerObject.GetComponent<WaveManager>();

        if (burnEffect != null)
            maxBurnEffectScale_Vec = burnEffect.transform.localScale * BurnEffectScaleMult;
        else Debug.LogError("BurnEffect GameObject is not assigned in the Inspector!", this);
    }

    void Update()
    {
        if (!playersInZone || isBurning.value || !Input.GetKey(KeyCode.E) || !canRekindleFire)
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
            isBurning.value = true;
            fireHP = maxFireHp / 4;
            waveManager.StartContinuousWaves(gameObject.transform, isBurning);
            StartCoroutine(RekindleFireTimmer());
        }
    }

    void FixedUpdate()
    {
        if (!isBurning.value)
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

        fireIntensityPercentageFactor = fireHP / maxFireHp;

        HandleBurningDamage(Time.fixedDeltaTime);
        HandleFireGrowth(Time.fixedDeltaTime);
        DebugShowHP(Time.fixedDeltaTime);
    }

    float debugShowHPTimer = 0f;
    void DebugShowHP(float dt)
    {
        debugShowHPTimer += dt;
        if (debugShowHPTimer < 1f) return;
        debugShowHPTimer = 0f;
        Debug.Log("Current HP: " + currentHp);
        Debug.Log("Fire HP: " + fireHP);
    }

    float burnDMGTimer = 0f;
    void HandleBurningDamage(float dt)
    {
        burnDMGTimer += dt;
        while (burnDMGTimer >= burnSpeed)
        {
            burnDMGTimer -= burnSpeed;
            currentHp -= burningDmg * fireIntensityPercentageFactor;
            if (currentHp <= 0) StartCoroutine(WinGame());
        }
    }

    float growthTimer = 0f;
    void HandleFireGrowth(float dt)
    {
        while (growthTimer >= fireHealSpeed)
        {
            growthTimer -= fireHealSpeed;            
            fireHP += fireHeal;
            fireHP = (fireHP > maxFireHp) ? maxFireHp : fireHP;                       
        }
    }

    void UpdateBurnEffectScale()
    {
        if (burnEffect != null && !burnEffect.activeSelf)
        {
            burnEffect.SetActive(true);
        }
        burnEffect.transform.localScale = maxBurnEffectScale_Vec * fireIntensityPercentageFactor;
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

    IEnumerator GrowFire()
    {
        isGrowingFire = true;

        while (isBurning.value) {
        if (fireHP < maxFireHp)
        {
            fireHP += fireHeal;
            fireHP = (fireHP > maxFireHp) ? maxFireHp : fireHP;
        }

        burnEffect.transform.localScale = maxBurnEffectScale_Vec * fireIntensityPercentageFactor;

        yield return new WaitForSeconds(fireHealSpeed);
        }
        isGrowingFire = false;
    }

    IEnumerator BurningDmg()
    {
        isTakingBurningDmg = true;
        burnEffect.SetActive(true);
        while (isBurning.value)
        {
            if (currentHp > 0)
            {
                currentHp -= burningDmg * fireIntensityPercentageFactor;
            }
            else
            {
                StartCoroutine(WinGame());
                isTakingBurningDmg = false;
                if (burnEffect != null) burnEffect.SetActive(false);
                yield break; // Exit the coroutine
            }

            yield return new WaitForSeconds(burnSpeed);            
        }

        isTakingBurningDmg = false;
        burnEffect.SetActive(false);
    }

    IEnumerator HealingHealth()
    {
        isHealing = true;
        healingEffect.SetActive(true);

        currentHp += treeHeal;

        yield return new WaitForSeconds(healSpeed);

        isHealing = false;
        healingEffect.SetActive(false);
    }

    public Wrapper<bool> GetIsBurning()
    {
        return isBurning;
    }
    
    public bool GetIsBurningState()
    {
        return isBurning.value;
    }

    public void FireExtinguish(float fireStoppingPower)
    {
        if (!canExtinguishFire) return;
        fireHP -= fireStoppingPower;
        if (fireHP <= 0)
        {
            isBurning.value = false;
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
