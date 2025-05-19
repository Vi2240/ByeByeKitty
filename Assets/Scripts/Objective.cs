using System.Collections;
using UnityEngine;

public class Objective : MonoBehaviour
{
    [SerializeField] GameObject numberEffect;

    [Header("Tree Variables")]
    [SerializeField] GameObject healingEffect;
    [SerializeField] float maxHp;
    [SerializeField] float healSpeed;
    [SerializeField] float treeHeal;
    [SerializeField] bool enemyKillzone;
    float currentHp;
    bool treeAlive = true;

    [Header("Fire Variables")]
    [SerializeField] bool canFireBeExtinguished = true;
    [SerializeField] float maxFireHp;
    [SerializeField] float fireHeal;
    [SerializeField] float fireHealSpeed;
    [SerializeField] float burningDmg;
    [SerializeField] float burnSpeed;
    [SerializeField] float BurnEffectScaleMult = 1;
    float fireIntensityPercentageFactor;
    float fireHP;
    Vector3 maxBurnEffectScale_Vec;

    [Header("Fire Activation Variables")]
    [SerializeField] GameObject burnEffect;
    [SerializeField] float rekindleFireDelay = 5f;

    [SerializeField] GameObject winCanvas;

    [SerializeField] GameObject waveManagerObject;

    bool playersInZone;
    Wrapper<bool> isBurning;
    bool canRekindleFire = true;
    bool gameWon;

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
        gameWon = false;
        treeAlive = true;
        waveManager = waveManagerObject.GetComponent<WaveManager>();

        if (burnEffect != null)
        {
            burnEffect.SetActive(false);
            maxBurnEffectScale_Vec = burnEffect.transform.localScale * BurnEffectScaleMult;
        }
        else Debug.LogError("BurnEffect GameObject is not assigned in the Inspector!", this);

        // flip all timers -- set to TPS instead of SPT
        healSpeed = (healSpeed == 0) ? 0 : 1 / healSpeed;
        burnSpeed = (burnSpeed == 0) ? 0 : 1 / burnSpeed;
        fireHealSpeed = (fireHealSpeed == 0) ? 0 : 1 / fireHealSpeed;

    }

    void Update()
    {
        if (!treeAlive) return;

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
        if (!treeAlive) return;

        if (isBurning.value)
        {
            healingEffect.SetActive(false);
            fireIntensityPercentageFactor = fireHP / maxFireHp;
            HandleBurningDamage(Time.fixedDeltaTime);
            HandleFireGrowth(Time.fixedDeltaTime);
            //DebugShowHP(Time.fixedDeltaTime)
            return;
        }

        burnEffect.SetActive(false);

        if (currentHp < maxHp)
        {
            HandleHealing(Time.fixedDeltaTime);
            return;
        }

        healingEffect.SetActive(false);
    }

    // float debugShowHPTimer = 0f;
    // void DebugShowHP(float dt)
    // {
    //     debugShowHPTimer += dt;
    //     if (debugShowHPTimer <= 0.5f) return;
    //     debugShowHPTimer = 0f;
    //     Debug.Log("Current HP: " + currentHp + "\tFireHP: " + fireHP);        
    // }

    float burnDMGTimer = 0f;
    void HandleBurningDamage(float dt)
    {
        burnDMGTimer += dt;
        float curBurnSpeed = burnSpeed / fireIntensityPercentageFactor;
        while (burnDMGTimer >= curBurnSpeed)
        {
            burnDMGTimer -= curBurnSpeed;
            currentHp -= burningDmg;
            var dmgnr = Instantiate(numberEffect, transform.position, Quaternion.identity);
            dmgnr.GetComponent<FloatingHealthNumber>().SetText(burningDmg.ToString(), 1);
            if (currentHp <= 0 && treeAlive)
            {
                treeAlive = false;
                waveManager.StartBossWave();
            }
        }
    }

    float growthTimer = 0f;
    void HandleFireGrowth(float dt)
    {
        growthTimer += dt;
        while (growthTimer >= fireHealSpeed)
        {
            growthTimer -= fireHealSpeed;
            fireHP += fireHeal;
            fireHP = (fireHP > maxFireHp) ? maxFireHp : fireHP;
        }
        UpdateBurnEffectScale();
    }

    float healTimer = 0f;
    void HandleHealing(float dt)
    {
        healTimer += dt;
        healingEffect.SetActive(true);
        while (healTimer >= healSpeed)
        {
            healTimer -= healSpeed;
            float tmpHp = currentHp + treeHeal;
            currentHp = (tmpHp < maxHp) ? tmpHp : maxHp;

            var healnr = Instantiate(numberEffect, transform.position, Quaternion.identity);
            healnr.GetComponent<FloatingHealthNumber>().SetText(treeHeal.ToString(), 2);
        }
    }

    void UpdateBurnEffectScale()
    {
        if (burnEffect == null || !burnEffect.activeSelf)
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
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyStopFire>().SetInObjectiveZone(false, this.gameObject);
        }
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
        if (!canFireBeExtinguished) return;
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
        if (gameWon) yield break;
        gameWon = true;
        winCanvas.SetActive(true);
        yield return new WaitForSeconds(5);
        Loader.LoadNetwork(Loader.Scene.MenuScene);
    }

    IEnumerator RekindleFireTimmer()
    {
        canRekindleFire = false;
        yield return new WaitForSeconds(rekindleFireDelay);
        canRekindleFire = true;
    }   
}
