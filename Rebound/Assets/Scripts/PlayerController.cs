using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;

    [Header("Dodge Settings")]
    public float maxDodgeDistance = 6f;
    public float dodgeDuration = 0.4f;

    [Header("Physics")]
    public float drag = 0.2f;

    private Rigidbody rb;
    private bool isDodging;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.linearDamping = drag;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
