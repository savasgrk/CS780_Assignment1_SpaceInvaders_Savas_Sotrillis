using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    public static float shotCooldown = .2f;
    public float lastShot = 0;
    public float speed = 15f;
    [SerializeField, Min(0f)] private float bottomScreenGap = 1f;
    [SerializeField] private InputActionAsset inputActions;
    private InputActionAsset runtimeActions;
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction touchMoveAction;
    private Camera gameplayCamera;
    public int ShieldCharges { get; private set; } = GameSettings.ShieldCapacity;
    private SpriteRenderer shieldVisual;
    private Sprite shieldSprite;
    private float shieldFlash;
    private Quaternion startingRotation;
    private float flightY;
    private float flightZ;
    private bool dying;
    private float movementMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private bool waitForFireRelease = true;

    void Awake()
    {
        startingRotation = transform.rotation;
        flightY = transform.position.y;
        flightZ = transform.position.z;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation
                | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            body.isKinematic = true;
        }

        if (inputActions == null)
        {
            Debug.LogError("Assign the spaceship's input action asset.", this);
            enabled = false;
            return;
        }

        runtimeActions = Instantiate(inputActions);
        moveAction = runtimeActions.FindAction("Player/Move", true);
        attackAction = runtimeActions.FindAction("Player/Attack", true);
        touchMoveAction = runtimeActions.FindAction("Player/TouchMove", true);
    }

    void OnEnable()
    {
        moveAction?.Enable();
        attackAction?.Enable();
        touchMoveAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        attackAction?.Disable();
        touchMoveAction?.Disable();
    }

    void OnDestroy()
    {
        if (shieldSprite != null)
        {
            Destroy(shieldSprite.texture);
            Destroy(shieldSprite);
        }
        if (runtimeActions != null)
        {
            Destroy(runtimeActions);
        }
    }

    void Start()
    {
        int level = GameSession.Instance != null ? GameSession.Instance.Level : 1;
        movementMultiplier = LevelProgression.GetPlayerMovementMultiplier(level);
        fireRateMultiplier = LevelProgression.GetPlayerFireRateMultiplier(level);
        gameplayCamera = Camera.main;
        if (gameplayCamera != null)
        {
            // Set the visible rocket's bottom, rather than its center, above the screen edge.
            float bottomOffset = 0f;
            foreach (SpriteRenderer artwork in GetComponentsInChildren<SpriteRenderer>())
            {
                if (artwork.enabled)
                    bottomOffset = Mathf.Max(bottomOffset, transform.position.y - artwork.bounds.min.y);
            }
            float depth = gameplayCamera.WorldToViewportPoint(transform.position).z;
            float screenBottom = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth)).y;
            Vector3 position = transform.position;
            position.y = screenBottom + bottomScreenGap + bottomOffset;
            transform.position = position;
            flightY = position.y;
            GameSession.Instance?.SetDefeatLineForPlayer(flightY);
        }
        shieldSprite = SpaceArt.CreateShieldRing();
        GameObject shield = new GameObject("Shield", typeof(SpriteRenderer));
        shield.transform.SetParent(transform, false);
        shield.transform.localPosition = new Vector3(0f, 0f, -0.2f);
        shield.transform.localScale = new Vector3(3.6f, 2.5f, 1f);
        shieldVisual = shield.GetComponent<SpriteRenderer>();
        shieldVisual.sprite = shieldSprite;
        RefreshShieldVisual();
    }

    //made changes to allow for gamepad and prevent errors if no keyboard is detected
    void Update()
    {
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying)
        {
            waitForFireRelease = true;
            return;
        }
        shieldFlash = Mathf.Max(0f, shieldFlash - Time.deltaTime);
        RefreshShieldVisual();
        Vector3 position = transform.position;
        position.x += Mathf.Clamp(moveAction.ReadValue<Vector2>().x, -1f, 1f)
            * speed * movementMultiplier * Time.deltaTime;

        // Touch delta is already a distance this frame, so do not multiply by deltaTime.
        float touchDelta = touchMoveAction.ReadValue<Vector2>().x;
        if (gameplayCamera != null && gameplayCamera.pixelWidth > 0 && touchDelta != 0f)
        {
            Vector3 viewportPosition = gameplayCamera.WorldToViewportPoint(position);
            viewportPosition.x += touchDelta * movementMultiplier / gameplayCamera.pixelWidth;
            position.x = gameplayCamera.ViewportToWorldPoint(viewportPosition).x;
        }
        transform.position = position;
        WrapHorizontally();

        if (waitForFireRelease)
        {
            // Starting/resuming with a shoot button must not also fire while it stays held.
            if (!attackAction.IsPressed()) waitForFireRelease = false;
        }
        else if (attackAction.IsPressed() && Time.time - lastShot >= shotCooldown / fireRateMultiplier)
        {
            GameObject bullet = Instantiate<GameObject>(dartPrefab);
            bullet.transform.position = transform.position;
            lastShot = Time.time;
        }
    }

    void LateUpdate()
    {
        // This ship only moves sideways: keep its heading and flight plane fixed.
        Vector3 position = transform.position;
        position.y = flightY;
        position.z = flightZ;
        transform.SetPositionAndRotation(position, startingRotation);
    }

    public void TakeBulletHit(Vector3? hitPosition = null)
    {
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        if (ShieldCharges > 0)
        {
            ShieldCharges--;
            CollisionBurst.Spawn(hitPosition ?? transform.position, new Color(0.2f, 0.8f, 1f), 0.8f, 0.35f);
            shieldFlash = 0.15f;
            RefreshShieldVisual();
            return;
        }
        Die();
    }

    public void RefillShield()
    {
        CollisionBurst.Spawn(transform.position, new Color(0.2f, 0.65f, 1f), 2f, 0.5f);
        ShieldCharges = GameSettings.ShieldCapacity;
        shieldFlash = 0.25f;
        RefreshShieldVisual();
    }

    private void RefreshShieldVisual()
    {
        if (shieldVisual == null) return;
        shieldVisual.enabled = ShieldCharges > 0 || shieldFlash > 0f;
        shieldVisual.color = shieldFlash > 0f ? Color.white
            : new Color(0.2f, 0.85f, 1f, 0.3f + 0.2f * ShieldCharges);
    }

    public void Die()
    {
        if (dying) return;
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        dying = true;
        CollisionBurst.Spawn(transform.position, new Color(1f, 0.4f, 0.15f), 2.5f, 0.6f);
        GameSession.Instance?.FinishRound(false);
        Destroy(gameObject);
    }

    //if spaceship goes off screen, wraps to the other side
    private void WrapHorizontally()
    {
        if (gameplayCamera == null)
        {
            return;
        }

        Vector3 viewportPosition = gameplayCamera.WorldToViewportPoint(transform.position);
        if (viewportPosition.x < 0f || viewportPosition.x > 1f)
        {
            // Preserve any movement beyond the edge when wrapping to the other side.
            viewportPosition.x = Mathf.Repeat(viewportPosition.x, 1f);
            Vector3 position = transform.position;
            position.x = gameplayCamera.ViewportToWorldPoint(viewportPosition).x;
            transform.position = position;
        }
    }
}


