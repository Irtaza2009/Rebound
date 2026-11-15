
using UnityEngine;

public class InfiniteProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Spawn Positions")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("Movement")]
    public Vector3 direction1 = new Vector3(1, 0, 1);
    public Vector3 direction2 = new Vector3(-1, 0, 1);

    [Header("Projectile Settings")]
    public float projectileSpeed = 15f;
    public float projectileLifetime = 10000f;
    public int maxBounces = 10000;

    private void Start()
    {
        SpawnProjectile(spawnPoint1.position, direction1);
        SpawnProjectile(spawnPoint2.position, direction2);
    }

    private void SpawnProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            position,
            Quaternion.LookRotation(direction)
        );

        ProjectileController projectile =
            projectileObject.GetComponent<ProjectileController>();

        projectile.speed = projectileSpeed;
        projectile.lifetime = projectileLifetime;
        projectile.maxBounces = maxBounces;

        projectile.Initialize(direction);
    }
}
