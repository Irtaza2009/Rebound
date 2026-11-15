using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public int startingLives = 3;
    public GameObject defeatedPrefab;

    [Header("Dodge Settings")]
    public float maxDodgeDistance = 6f;
    public float dodgeDuration = 0.4f;

    [Header("Physics")]
    public float drag = 0.2f;

    private Rigidbody rb;
    private bool isDodging;
    private int lives;
    private bool isDefeated;

    public int Lives => lives;
    public bool IsDefeated => isDefeated;

    public Color GetPlayerColor()
    {
        Renderer playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null && playerRenderer.sharedMaterial != null)
            return playerRenderer.sharedMaterial.color;

        return Color.white;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lives = startingLives;

        rb.useGravity = true;
        rb.linearDamping = drag;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public int TakeDamage()
    {
        if (isDefeated)
            return lives;

        lives = Mathf.Max(0, lives - 1);

        if (lives == 0)
            BecomeDefeated();

        return lives;
    }

    private void BecomeDefeated()
    {
        isDefeated = true;

        if (defeatedPrefab != null)
        {
            GameObject defeatedObject = Instantiate(
                defeatedPrefab,
                transform.position,
                transform.rotation
            );

            ApplyPlayerMaterial(defeatedObject);
        }
        else
        {
            Debug.LogWarning(
                "Player " + playerNumber +
                " has no defeated prefab assigned."
            );
        }

        gameObject.SetActive(false);
    }

    private void ApplyPlayerMaterial(GameObject defeatedObject)
    {
        Renderer sourceRenderer = GetComponentInChildren<Renderer>();
        if (sourceRenderer == null)
            return;

        Material playerMaterial = sourceRenderer.sharedMaterial;
        if (playerMaterial == null)
            return;

        Renderer[] defeatedRenderers =
            defeatedObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer defeatedRenderer in defeatedRenderers)
        {
            Material[] materials = defeatedRenderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
                materials[i] = playerMaterial;

            defeatedRenderer.sharedMaterials = materials;
        }
    }

    public void ExecuteDodge(Vector3 direction, float strength)
    {
        if (isDodging)
            return;

        StartCoroutine(Dodge(direction, strength));
    }

    private IEnumerator Dodge(Vector3 direction, float strength)
    {
        isDodging = true;

        direction.y = 0f;
        direction.Normalize();

        float dodgeSpeed = maxDodgeDistance / dodgeDuration;

        rb.linearVelocity = direction * dodgeSpeed * strength;

        yield return new WaitForSeconds(dodgeDuration);

        isDodging = false;
    }
}
