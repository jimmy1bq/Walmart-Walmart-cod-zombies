using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IDamageAble
{
    [Header("Stats")]
    //grab from rounds manager(currentround-1)
    public int roundsSurvived = 0;
    //grab from points manager(totalPoints)
    public int totalPoints = 0;
    //increments by zombies when they die
    public int zombieKills = 0;
    public int headShotKills = 0;
    public int powerUpsCollected = 0;
    public int highestRound = 0;
    //Delatime
    public double timePlayed = 0;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;

    [Header("Weapons")]
    public Weapon[] weaponSlots = new Weapon[2];
    public Transform weaponHolder;
    public WeaponData startingWeaponData;   // Assign the Pistol asset here in the Inspector

    [Header("Sprint Weapon Tilt")]
    public Vector3 sprintTiltEuler = new Vector3(10f, -40f, 45f);
    public float sprintTiltSpeed = 10f;

    [Header("Health")]
    public int maxHits = 5;
    public float regenDelay = 5f;

    [Header("PowerUpsUI")]
    public GameObject powerUpUILayOut;
    public UnityEngine.UI.Image doublePointsIcon;
    public UnityEngine.UI.Image maxAMMO;
    public UnityEngine.UI.Image doubleShot;

    public bool doublePointsIconHere = false;
    public bool maxAMMOIconHere = false;
    public bool doubleShotIconHere = false;

    [SerializeField] GameObject deathPanel;
    CharacterController _controller;
    Vector3 _velocity;
    float _verticalRotation;
    int _currentWeaponIndex;
    Camera playerCam;

    AudioSource footStep;
    AudioSource heartBeat;
    AudioSource heavyBreathing;

    int _totalHitsRemaining;
    int _hitsRemaining;
    float _regenTimer;
    bool _isDead;
    bool _cantSprint = false;
    public bool _pasued = false;
    public TextMeshProUGUI windowRepairStatus;
    float totalStamina = 5f;
    float stamina = 0f;

    public AudioClip playerFootStep;
    public AudioClip playerSprintBreath;
    public AudioClip playerHitBreath;



    private void Awake()
    {
        deathPanel.SetActive(false);
        highestRound = highestRoundData.instance.LoadData().playerData.highestRound;
        AudioSource[] arrayOfSources = GetComponentsInChildren<AudioSource>();
        footStep = arrayOfSources[0];
        heartBeat = arrayOfSources[1];
        heavyBreathing = arrayOfSources[2];
        _hitsRemaining = maxHits;
        windowRepairStatus.gameObject.SetActive(false);

        var bseGO = new GameObject("BloodScreenEffect");
        bseGO.AddComponent<BloodScreenEffect>();
    }
    bool _isSprinting;
    Quaternion _weaponIdleRotation;
    Quaternion _currentSprintTilt = Quaternion.Euler(0, 180, 0);
    float _repairCooldown = 0f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign the pistol to slot 0
        if (startingWeaponData != null && weaponSlots[0] != null)
        {
            weaponSlots[0].weaponData = startingWeaponData;
            weaponSlots[0].Initialize(); // re-init so ammo reflects the assigned data
        }

        // Guarantee slot 1 starts with NO weapon data regardless of Inspector values
        if (weaponSlots.Length > 1 && weaponSlots[1] != null)
        {
            weaponSlots[1].weaponData = null;
            weaponSlots[1].gameObject.SetActive(false);
        }

        if (weaponHolder == null && cameraHolder != null)
            weaponHolder = cameraHolder.Find("WeaponHolder");

        // Hide every gun in the holder; EquipWeapon will reveal only the active slot
        if (weaponHolder != null)
            foreach (Transform child in weaponHolder)
                child.gameObject.SetActive(false);

        _weaponIdleRotation = weaponHolder != null ? weaponHolder.localRotation : Quaternion.identity;

        playerCam = cameraHolder != null ? cameraHolder.GetComponentInChildren<Camera>() : Camera.main;

        EquipWeapon(0);
    }

    void Update()
    {
        if (_isDead) return;

        _regenTimer += Time.deltaTime;
        if (_regenTimer >= regenDelay && _hitsRemaining < maxHits)
        {
            _hitsRemaining++;
            _regenTimer = 0f;
            BloodScreenEffect.Instance?.OnRegen(_hitsRemaining, maxHits);
            if (_hitsRemaining == 0)
            {
                heartBeat.Stop();
            }
        }

        HandleMouseLook();
        HandleMovement();
        HandleWeaponTilt();
        HandleWeaponSwitch();
        HandleReload();
        HandleShooting();
        TickReloads();
        repairWindow();
        PauseGame();
        mouseSensitivity = audioManagerZombies.instance.mouseSensitivity;
        timePlayed += Time.deltaTime;
    }

    //if the game isn't paused and we hit ESC pause it otherwise we unpause it
    void PauseGame()
    {
        if (!_pasued && Input.GetKeyDown(KeyCode.Escape))
        {
            _pasued = true;
            UIManager.Instance.pauseScreen();
        }
        else if (_pasued && Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.unPauseScreen();
            _pasued = false;
        }
    }

    // Tick reload timers from PlayerController so they complete even when
    // a weapon's GameObject is inactive in the hierarchy.
    void TickReloads()
    {
        foreach (Weapon slot in weaponSlots)
            if (slot != null) slot.TickReload();
    }
    void repairWindow()
    {
        _repairCooldown -= Time.deltaTime;

        Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 2.5f)) { windowRepairStatus.gameObject.SetActive(false); return; }

        if (hit.collider.gameObject.CompareTag("PotentialBoard"))
        {
            IInteractable interact = hit.collider.transform.parent.Find("woodenBoard").GetComponent<IInteractable>();
            updateBoardStatus(hit.collider.gameObject);
            if ((Input.GetKey(KeyCode.E) || Input.GetKeyDown(KeyCode.E)) && _repairCooldown <= 0f && !interact.zombieInteract())
            {
                hit.collider.transform.parent.Find("woodenBoard").GetComponent<IHealAble>().action(20);
                _repairCooldown = 1.25f;
            }
        }
    }
    void updateBoardStatus(GameObject interactable)
    {
        IInteractable interact = interactable.transform.parent.Find("woodenBoard").GetComponent<IInteractable>();
        IDamageAble damageable = interactable.transform.parent.Find("woodenBoard").GetComponent<IDamageAble>();
        windowRepairStatus.gameObject.SetActive(true);
        if (interact.zombieInteract())
        {
            windowRepairStatus.text = "Zombie at window, can't Build";
        }
        else if (damageable.returnHP() >= 120f)
        {
            windowRepairStatus.text = "Fully Boarded";
        }
        else if (damageable.returnHP() < 120f)
        {
            windowRepairStatus.text = "Add Board";
        }
    }
    void HandleMouseLook()
    {
        if (_pasued) return;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -80f, 80f);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        bool grounded = _controller.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        //so in game everytime you sprint and unsprint you have to wait until it resets
        _isSprinting = Input.GetKey(KeyCode.LeftShift) && (h != 0f || v != 0f);

        float speed = moveSpeed;
        //if we are shifting and not on cool and we have stamina

        if (_isSprinting && !_cantSprint && stamina < 5f)
        {
            speed *= sprintMultiplier;
            stamina += Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, totalStamina);
            Debug.Log("stamina right now: " + stamina);
            if (stamina / totalStamina >= 0.8f && !heavyBreathing.isPlaying)
            {
                heavyBreathing.clip = playerSprintBreath;
                heavyBreathing.Play();
            }
        }
        //if we stop we have to wait for stamina to finish regening or down to 0
        else { _isSprinting = false; _cantSprint = true; stamina -= Time.deltaTime; stamina = Mathf.Clamp(stamina, 0, totalStamina); if (stamina == 0) { stamina = 0f; _cantSprint = false; } }

        Vector3 move = transform.right * h + transform.forward * v;
        _controller.Move(move * speed * Time.deltaTime);


        if (/*(Input.GetKey(KeyCode.W) || Input.GetKeyDown(KeyCode.W))*/ (v != 0f || h != 0f) && !footStep.isPlaying)
        {
            //the audio is lengthen by audacity since you can't effecitley make this audio long enough in unity
            footStep.pitch = 0.5f;
            footStep.volume = audioManagerZombies.instance.sfxVolume;
            footStep.PlayOneShot(playerFootStep);
        }
        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);


        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleWeaponTilt()
    {
        if (weaponHolder == null) return;

        // Sync weapon holder to camera's vertical look each frame (WeaponHolder is a
        // sibling of Camera, not a child, so it doesn't inherit the look rotation).
      //  Quaternion lookRot = _weaponIdleRotation * Quaternion.Euler(_verticalRotation, 0f, 0f);

        // Sprint tilt layered on top, lerped for a smooth transition
        Quaternion sprintTarget = _isSprinting ? Quaternion.Euler(sprintTiltEuler) : Quaternion.Euler(0, 180, 0);
        _currentSprintTilt = Quaternion.Lerp(_currentSprintTilt, sprintTarget, sprintTiltSpeed * Time.deltaTime);

        CurrentWeapon.transform.localRotation = /*lookRot **/ _currentSprintTilt;
    }

    void HandleWeaponSwitch()
    {
        // F key — cycle to the next occupied slot
        if (Input.GetKeyDown(KeyCode.F))
        {
            int next = (_currentWeaponIndex + 1) % weaponSlots.Length;
            if (HasWeapon(next)) EquipWeapon(next);
        }

        // Scroll wheel — cycle to the next occupied slot
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int next = (_currentWeaponIndex + 1) % weaponSlots.Length;
            if (HasWeapon(next)) EquipWeapon(next);
        }
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
            CurrentWeapon?.ForceReload();
    }

    void EquipWeapon(int index)
    {
        if (!HasWeapon(index)) return;

        // Activate the container first so weapons have a valid activeInHierarchy state
        if (weaponHolder != null)
            weaponHolder.gameObject.SetActive(true);

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
                weaponSlots[i].gameObject.SetActive(i == index);
        }

        _currentWeaponIndex = index;
    }

    // A slot counts as occupied only if it has a Weapon component with data assigned
    bool HasWeapon(int index) =>
        index >= 0 && index < weaponSlots.Length &&
        weaponSlots[index] != null &&
        weaponSlots[index].weaponData != null;

    /// <summary>
    /// Returns the best slot index to place a new weapon:
    /// - First empty slot (component exists, no weapon data) → goes there (slot 1 stays free for pistol swap)
    /// - All slots filled → returns the currently held slot (replaces it)
    /// - Returns -1 only if no Weapon components are set up at all
    /// </summary>
    public int GetTargetSlot()
    {
        // Pass 1: find the first slot that has a Weapon component but no weapon assigned
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].weaponData == null)
                return i;
        }

        // Pass 2: all slots occupied — replace whichever the player is holding
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == CurrentWeapon) return i;
        }

        return -1;
    }

    /// <summary>
    /// Finds the physical weapon GameObject in the WeaponHolder whose WeaponData
    /// matches <paramref name="data"/>, assigns it to the slot, and equips it.
    /// </summary>
    public void GiveWeapon(int slotIndex, WeaponData data)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) { Debug.Log("not here"); return; }

        Weapon incoming = FindWeaponInHolder(data);
        if (incoming == null)
        {
            Debug.LogWarning($"PlayerController: No weapon with data '{data.weaponName}' found in WeaponHolder.");
            return;
        }
        Debug.Log("down here");
        // Hide whatever was in this slot before
        if (weaponSlots[slotIndex] != null)
        {
            Debug.Log("down here pt2");
            weaponSlots[slotIndex].gameObject.SetActive(false);
        }

        Debug.Log("equpping");
        weaponSlots[slotIndex] = incoming;
        incoming.Initialize();
        EquipWeapon(slotIndex);
    }

    // Searches direct children of WeaponHolder for a Weapon whose data matches.
    Weapon FindWeaponInHolder(WeaponData data)
    {
        Debug.Log("WEAPON NAME: " + data.name);
        if (weaponHolder == null) return null;
        foreach (Transform child in weaponHolder)
        {
            Weapon w = child.GetComponent<Weapon>();

            if (w != null && w.weaponData == data)
                return w;
        }
        return null;
    }

    void HandleShooting()
    {
        if (_isSprinting || _pasued) return;

        Weapon current = CurrentWeapon;
        if (current == null || current.weaponData == null) return;

        if (current.weaponData.isAutomatic)
        {
            if (Input.GetButton("Fire1")) current.TryShoot(cameraHolder);
        }
        else
        {
            if (Input.GetButtonDown("Fire1")) current.TryShoot(cameraHolder);
        }
    }

    public Weapon CurrentWeapon => weaponSlots[_currentWeaponIndex];

    public float takeDamage(float damage, int damageType)
    {
        if (_isDead) return 0f;
        if (!heartBeat.isPlaying)
        {
            heartBeat.clip = playerHitBreath;
            heartBeat.Play();
        }
        _hitsRemaining--;
        heartBeat.pitch = 1 + (maxHits - _hitsRemaining) / 2f;
        _regenTimer = 0f;
        BloodScreenEffect.Instance?.OnHit(_hitsRemaining, maxHits);
        if (_hitsRemaining <= 0)
            Die();
        return _hitsRemaining;
    }

    public float returnHP() => _hitsRemaining;

    void Die()
    {
        _isDead = true;
        BloodScreenEffect.Instance?.ResetEffect();
        if (deathPanel != null) deathPanel.SetActive(true);
        updateDeathPanel();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void updateDeathPanel()
    {

        Transform deathPanelTransform = deathPanel.transform;
        deathPanelTransform.Find("RoundsSurvivedText").GetComponent<TextMeshProUGUI>().text = "Rounds Survived " + roundsSurvived;
        deathPanelTransform.Find("PointtotalTXT").GetComponent<TextMeshProUGUI>().text = "Points Total " + PointsManager.Instance.TotalPoints;
        deathPanelTransform.Find("ZOMBIEKILLEDTXT").GetComponent<TextMeshProUGUI>().text = "Zombies Killed " + zombieKills;
        deathPanelTransform.Find("HeadShotKillTXT").GetComponent<TextMeshProUGUI>().text = "HeadShot Kills " + headShotKills;
        deathPanelTransform.Find("PowerUpcollectedTXT").GetComponent<TextMeshProUGUI>().text = "powerup collected " + powerUpsCollected;
        int hours = (int)Math.Floor(timePlayed / 3600);
        //leftover for mintue conversion
        double leftOverMintues = timePlayed - hours * 3600;
        int mintues = (int)Math.Floor(leftOverMintues / 60);
        //leftOver seconds
        int leftOverSeconds = (int)(leftOverMintues - mintues * 60);
        deathPanelTransform.Find("TimeSurvivedTXT").GetComponent<TextMeshProUGUI>().text = "Time " + hours + ":" + mintues + ":" + leftOverSeconds;
        if (roundsSurvived > highestRound)
        {
            newHighScore(deathPanelTransform.Find("HighscoreText").gameObject);
        }
        else { deathPanelTransform.Find("HighscoreText").gameObject.SetActive(false); }
        //deathPanelTransform.Find("HighscoreText").GetComponent<TextMeshProUGUI>().text = "Points Total " + PointsManager.Instance.TotalPoints;
    }
    void newHighScore(GameObject deathpanel)
    {
        pingPongColorTween(deathpanel.GetComponent<TextMeshProUGUI>());
        pingPongRotTween(deathpanel.GetComponent<TextMeshProUGUI>());
    }
    void pingPongColorTween(TextMeshProUGUI deathpanelText)
    {
        LeanTween.value(deathpanelText.gameObject, Color.red, Color.white, 1f).setOnUpdate((Color col) => {
            deathpanelText.color = col;
        })
        .setLoopPingPong(999);
    }
    void pingPongRotTween(TextMeshProUGUI deathpanelText)
    {
        LeanTween.value(deathpanelText.gameObject, 10f, -10f, 1f).setOnUpdate((float angle) => {
            deathpanelText.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        })
       .setLoopPingPong(999);
    }

    //add powerUp onto the UI
    public void addPowerupUI(int powerupType)
    {
        switch (powerupType)
        {
            case 0: if (!doublePointsIconHere) { GameObject image = Instantiate(doublePointsIcon, powerUpUILayOut.transform.GetChild(0).GetChild(0)).gameObject; doublePointsIconHere = true; StartCoroutine(flash(image, 15f, 0)); } break;
            case 1: if (!maxAMMOIconHere) { GameObject image = Instantiate(maxAMMO, powerUpUILayOut.transform.GetChild(0).GetChild(0)).gameObject; maxAMMOIconHere = true; StartCoroutine(flash(image, 15f, 1)); } break;
            case 2: if (!doubleShotIconHere) { GameObject image = Instantiate(doubleShot, powerUpUILayOut.transform.GetChild(0).GetChild(0)).gameObject; doubleShotIconHere = true; StartCoroutine(flash(image, 15f, 2)); } break;
        }
    }
    IEnumerator flash(GameObject flashObject, float duration, int which)
    {
        float timeLeft = duration;
        while (true)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 3f)
            {
                blink(flashObject, timeLeft);
                StartCoroutine(setDestruction(flashObject, timeLeft, which));
                break;
            }
            yield return null;
        }
    }

    //flash when theres like 5 seconds left
    void blink(GameObject blinker, float timeLeft)
    {
        LeanTween.value(blinker, 1f, 0f, 0.5f).setOnUpdate((float alpha) =>
        {
            Color c = blinker.GetComponent<UnityEngine.UI.Image>().color;
            c.a = alpha;
            blinker.GetComponent<UnityEngine.UI.Image>().color = c;
        }).setLoopPingPong(999);
    }
    IEnumerator setDestruction(GameObject objectToDestroy, float duration, int which)
    {

        yield return new WaitForSeconds(duration);
        switch (which)
        {
            case 0: doublePointsIconHere = false; break;
            case 1: maxAMMOIconHere = false; break;
            case 2: doubleShotIconHere = false; break;

        }
        Destroy(objectToDestroy);
    }
}
