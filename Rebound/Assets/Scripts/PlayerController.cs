
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;

    [Header("Dodge Settings")]
    public float maxDodgeDistance = 6f;
    public float dodgeDuration = 0.4f;
    public float minDragDistance = 0.2f;

    [Header("Drag Settings")]
    public float maxDragDistance = 3f;

    [Header("Visuals")]
    public Transform aimArrow;

    private Camera mainCamera;
    private bool isDragging;
    private bool isDodging;

    private Vector3 dragDirection;
    private float dragStrength;

    private Plane groundPlane;

    private void Awake()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private void Update()
    {
        if (isDodging)
            return;

        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (isDragging && Input.GetMouseButton(0))
            UpdateDrag();

        if (isDragging && Input.GetMouseButtonUp(0))
            ReleaseDrag();
    }

    private void TryStartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                isDragging = true;
        }
    }

    private void UpdateDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 mousePosition = ray.GetPoint(distance);

        Vector3 drag = mousePosition - transform.position;
        drag.y = 0;

        dragDirection = drag.normalized;

        float dragDistance = Mathf.Min(drag.magnitude, maxDragDistance);
        dragStrength = dragDistance / maxDragDistance;

        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (aimArrow == null)
            return;

        aimArrow.gameObject.SetActive(true);
        aimArrow.position = transform.position + Vector3.up * 0.1f;

        if (dragDirection.sqrMagnitude > 0.001f)
            aimArrow.rotation = Quaternion.LookRotation(dragDirection);

        Vector3 scale = aimArrow.localScale;
        scale.z = dragStrength * maxDragDistance;
        aimArrow.localScale = scale;
    }

    private void ReleaseDrag()
    {
        isDragging = false;

        if (aimArrow != null)
            aimArrow.gameObject.SetActive(false);

        if (dragStrength < minDragDistance / maxDragDistance)
            return;

        float dodgeDistance = dragStrength * maxDodgeDistance;

        StartCoroutine(Dodge(dragDirection, dodgeDistance));

        dragStrength = 0;
    }

    private IEnumerator Dodge(Vector3 direction, float distance)
    {
        isDodging = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction * distance;

        float elapsed = 0;

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

        isDodging = false;
    }
}
