using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dart : MonoBehaviour
{
    // Equivalent to the original 0.3 units per tick at a 0.02-second timestep.
    private const float Speed = 15f;
    private Rigidbody body;
    private bool consumed;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = false;
        body.constraints = RigidbodyConstraints.FreezeRotation
            | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public bool TryConsume()
    {
        if (consumed) return false;
        consumed = true;
        body.linearVelocity = Vector3.zero;
        foreach (Collider hitbox in GetComponentsInChildren<Collider>())
            hitbox.enabled = false;
        Destroy(gameObject);
        return true;
    }
    
    void FixedUpdate()
    {
        if (consumed || (GameSession.Instance != null && !GameSession.Instance.IsPlaying)) return;
        body.linearVelocity = Vector3.up * Speed;
        if (body.position.y > 10f)
        {
            Destroy(gameObject);
        }
    }

}
