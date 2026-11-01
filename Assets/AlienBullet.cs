using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Inscribed")]

    const float initialSpeed = GameSettings.InitialAlienBulletSpeed;
    static float speed = initialSpeed;
    private bool consumed;
    private Camera gameplayCamera;
    void Start()
    {
        gameplayCamera = Camera.main;
    }

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= speed;
        transform.position = position;
        bool belowScreen = gameplayCamera != null
            ? gameplayCamera.WorldToViewportPoint(position).y < -0.1f
            : position.y < -15f;
        if (belowScreen)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (consumed) return;
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        consumed = true;
        Spaceship player = coll.gameObject.GetComponentInParent<Spaceship>();
        if (player != null)
        {
            Vector3 hitPosition = coll.contactCount > 0 ? coll.GetContact(0).point : transform.position;
            player.TakeBulletHit(hitPosition);
            Destroy(gameObject);
            return;
        }

        CollisionBurst.Spawn(transform.position, new Color(1f, 0.65f, 0.25f), 0.45f, 0.25f);
        Destroy(gameObject);
        Dart dart = coll.gameObject.GetComponentInParent<Dart>();
        Destroy(dart != null ? dart.gameObject : coll.gameObject);
    }
    public static void ResetSpeed()
    {
        speed = initialSpeed;
    }

    public static void IncreaseSpeed()
    {
        speed *= GameSettings.AlienSpeedMultiplier;
    }
}
