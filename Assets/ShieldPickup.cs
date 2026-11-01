using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    private Sprite icon;
    private bool collected;
    private Camera gameplayCamera;

    public static void Spawn(Vector3 position)
    {
        GameObject pickup = new GameObject("Shield refill", typeof(SpriteRenderer), typeof(SphereCollider), typeof(Rigidbody));
        pickup.transform.position = position;
        SphereCollider trigger = pickup.GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.65f;
        Rigidbody body = pickup.GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        pickup.AddComponent<ShieldPickup>();
    }

    void Start()
    {
        gameplayCamera = Camera.main;
        icon = SpaceArt.CreatePickupIcon();
        SpriteRenderer visual = GetComponent<SpriteRenderer>();
        visual.sprite = icon;
        visual.sortingOrder = 2;
        transform.localScale = Vector3.one * 1.1f;
    }

    void Update()
    {
        if (GameSession.Instance != null && !GameSession.Instance.IsPlaying) return;
        transform.position += Vector3.down * GameSettings.ShieldPickupFallSpeed * Time.deltaTime;
        if (gameplayCamera != null)
        {
            if (gameplayCamera.WorldToViewportPoint(transform.position).y < -0.1f)
                Destroy(gameObject);
        }
        else if (transform.position.y < -12f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || (GameSession.Instance != null && !GameSession.Instance.IsPlaying)) return;
        Spaceship player = other.GetComponentInParent<Spaceship>();
        if (player == null) return;
        collected = true;
        player.RefillShield();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (icon == null) return;
        Destroy(icon.texture);
        Destroy(icon);
    }
}
