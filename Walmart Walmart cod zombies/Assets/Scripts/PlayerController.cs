using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;

    [Header("Weapons")]
    public Weapon[] weaponSlots = new Weapon[2];
    public Transform weaponHolder;
    public WeaponData startingWeaponData;   // Assign the Pistol asset here in the Inspector

    CharacterController _controller;
    Vector3 _velocity;
    float _verticalRotation;
    int _currentWeaponIndex;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign the pistol (or any starting weapon) to slot 0
        if (startingWeaponData != null && weaponSlots[0] != null)
            weaponSlots[0].weaponData = startingWeaponData;

        // Slot 1 starts empty — hide it until the player picks up a second weapon
        if (weaponSlots[1] != null)
            weaponSlots[1].gameObject.SetActive(false);

        EquipWeapon(0);
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleWeaponSwitch();
        HandleShooting();
    }

    void HandleMouseLook()
    {
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

        Vector3 move = transform.right * h + transform.forward * v;
        _controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleWeaponSwitch()
    {
        // Number keys 1 and 2
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);

        // Scroll wheel — only switch if the target slot has a weapon
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int next = (_currentWeaponIndex + 1) % weaponSlots.Length;
            if (HasWeapon(next)) EquipWeapon(next);
        }
    }

    void EquipWeapon(int index)
    {
        if (!HasWeapon(index)) return;

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

    void HandleShooting()
    {
        Weapon current = CurrentWeapon;
        if (current == null) return;

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
}
