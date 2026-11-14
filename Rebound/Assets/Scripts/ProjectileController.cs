
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;
    public int maxBounces = 3;
    public float lifetime = 8f;

    private Rigidbody rb;
    private Vector3 direction;
    private int bounceCount;
    private bool initialized;

    public event Action<PlayerController> OnPlayerHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        bounceCount = 0;
        initialized = true;

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized)
            return;

        //if (!collision.collider.CompareTag("Wall"))
        //    return;

        // Check whether a player was hit.
        PlayerController player =
            collision.collider.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            OnPlayerHit?.Invoke(player);
            Destroy(gameObject);
            return;
        }

        // Bounce off arena walls.
        ContactPoint contact = collision.GetContact(0);

        direction = Vector3.Reflect(direction, contact.normal).normalized;

        bounceCount++;

        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }

        rb.linearVelocity = direction * speed;
    }
}
