using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject bulletPrefab;
    public AlienFleet fleet;
    
    private const float InitialFireProbabilityPerPhysicsTick = .002f;
    private static float fireProbabilityPerPhysicsTick = InitialFireProbabilityPerPhysicsTick;
    public float lastShot = 0;
    public static float shotCooldown = .1f;
    private bool dying;
    public int ShieldCharges { get; private set; }
    public SpriteRenderer ShieldVisual { get; private set; }
    private Sprite shieldSprite;
    private float shieldFlash;
    private SphereCollider shieldCollider;
    private Quaternion startingRotation;

    void Awake()
    {
        startingRotation = transform.rotation;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.useGravity = false;
            body.constraints |= RigidbodyConstraints.FreezeRotation;
            // The fleet controls movement; projectile impacts must not move aliens.
            body.isKinematic = true;
        }
    }

    void LateUpdate()
    {
        transform.rotation = startingRotation;
    }

    public void GiveShield()
    {
        ShieldCharges = GameSettings.ShieldCapacity;
        if (ShieldVisual == null)
        {
            shieldSprite = SpaceArt.CreateShieldRing();
            GameObject shield = new GameObject("Alien shield", typeof(SpriteRenderer));
            shield.transform.SetParent(transform, false);
            shield.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            shield.transform.localScale = Vector3.one * 1.5f;
            ShieldVisual = shield.GetComponent<SpriteRenderer>();
            ShieldVisual.sprite = shieldSprite;
            shield.layer = gameObject.layer;
            shieldCollider = shield.AddComponent<SphereCollider>();
            shieldCollider.isTrigger = true;
            // The ring radius is 0.44 in its local sprite coordinates.
            shieldCollider.radius = 0.44f;
        }
        RefreshShield();
    }

    private void RefreshShield()
    {
        if (ShieldVisual == null) return;
        shieldCollider.enabled = ShieldCharges > 0;
        ShieldVisual.enabled = ShieldCharges > 0 || shieldFlash > 0f;
        ShieldVisual.color = shieldFlash > 0f ? Color.white
            : new Color(0.75f, 0.4f, 1f, 0.3f + 0.2f * ShieldCharges);
    }

    public static void ResetFireRate()
    {
        fireProbabilityPerPhysicsTick = InitialFireProbabilityPerPhysicsTick;
    }

    public static void IncreaseFireRate()
    {
        fireProbabilityPerPhysicsTick = Mathf.Min(1f, fireProbabilityPerPhysicsTick * GameSettings.AlienFireRateMultiplier);
    }

    void Update()
    {
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        shieldFlash = Mathf.Max(0f, shieldFlash - Time.deltaTime);
        RefreshShield();
    }

    void FixedUpdate()
    {
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        Vector3 position = transform.position;
        
        if(UnityEngine.Random.value < fireProbabilityPerPhysicsTick && Time.time - lastShot > shotCooldown){
            GameObject bullet = Instantiate<GameObject>(bulletPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (dying) return;
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        Spaceship player = coll.gameObject.GetComponentInParent<Spaceship>();
        if (player != null)
        {
            player.Die();
            return;
        }

        Dart dart = coll.gameObject.GetComponentInParent<Dart>();
        TryTakeDartHit(dart);
    }

    void OnTriggerEnter(Collider other)
    {
        TryTakeDartHit(other.GetComponentInParent<Dart>());
    }

    private void TryTakeDartHit(Dart dart)
    {
        if (dying || (GameSession.Instance != null && !GameSession.Instance.IsPlaying)) return;
        if (dart == null || !dart.TryConsume()) return;
        if (ShieldCharges > 0)
        {
            ShieldCharges--;
            shieldFlash = 0.15f;
            RefreshShield();
            CollisionBurst.Spawn(transform.position, new Color(0.75f, 0.4f, 1f), 0.8f, 0.3f);
            return;
        }
        dying = true;
        CollisionBurst.Spawn(transform.position, new Color(0.4f, 1f, 0.45f), 1.1f);
        fleet.AlienKilled(gameObject);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (shieldSprite == null) return;
        Destroy(shieldSprite.texture);
        Destroy(shieldSprite);
    }
}
