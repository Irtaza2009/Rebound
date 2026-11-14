
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;

    [Header("Dodge Settings")]
    public float maxDodgeDistance = 6f;
    public float dodgeDuration = 0.4f;

    public void ExecuteDodge(Vector3 direction, float strength)
    {
        StartCoroutine(Dodge(direction, strength));
    }

    private IEnumerator Dodge(Vector3 direction, float strength)
    {
        Vector3 startPosition = transform.position;

        Vector3 targetPosition = startPosition +
            direction.normalized * (strength * maxDodgeDistance);

        float elapsed = 0f;

        while (elapsed < dodgeDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / dodgeDuration
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
