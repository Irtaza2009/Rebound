using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;
    public int maxBounces = 3;
    public float lifetime = 8f;

    [Header("Neon Laser Visuals (Optional)")]
    public GameObject bounceVfxPrefab;
    public GameObject hitVfxPrefab;
    public TrailRenderer trailRenderer;

    private Rigidbody rb;
    private Vector3 direction;
    private int bounceCount;
    private bool initialized;

    public event Action<PlayerController> OnPlayerHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configure Rigidbody for reliable, non-gravity laser flight
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        bounceCount = 0;
        initialized = true;

        // Point the laser mesh along its travel direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Clear any leftover trail if the projectile is ever reused/pooled
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized)
            return;

        // Check whether a player was hit.
        PlayerController player =
            collision.collider.GetComponentInParent<PlayerController>();

        ContactPoint contact = collision.GetContact(0);

        if (player != null)
        {
            SpawnVfx(hitVfxPrefab, contact.point, contact.normal);
            OnPlayerHit?.Invoke(player);
            Destroy(gameObject);
            return;
        }

        // Bounce off arena walls.
        direction = Vector3.Reflect(direction, contact.normal).normalized;
        bounceCount++;

        if (bounceCount >= maxBounces)
        {
            SpawnVfx(hitVfxPrefab, contact.point, contact.normal);
            Destroy(gameObject);
            return;
        }

        // Spawn bounce spark and rotate the laser bolt to face the new direction
        SpawnVfx(bounceVfxPrefab, contact.point, contact.normal);

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        rb.linearVelocity = direction * speed;
    }

    private void SpawnVfx(GameObject prefab, Vector3 position, Vector3 normal)
    {
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.LookRotation(normal));
        }
    }
}
