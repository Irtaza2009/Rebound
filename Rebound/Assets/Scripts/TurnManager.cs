
using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public enum ActionPhase
    {
        Shoot,
        Dodge,
        Executing
    }

    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("UI")]
    public UIManager uiManager;

    [Header("Drag Settings")]
    public float maxDragDistance = 3f;
    public float minDragDistance = 0.2f;

    [Header("Visuals")]
    public Transform dodgeAimArrow;
    public Transform shootAimArrow;

    [Header("Turn Settings")]
    public float turnDelay = 0.5f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpawnOffset = 0.8f;

    private Camera mainCamera;
    private Plane groundPlane;

    private PlayerController currentPlayer;
    private ActionPhase currentPhase;

    private bool isDragging;

    private Vector3 dragDirection;
    private float dragStrength;

    private Vector3 player1ShotDirection;
    private Vector3 player2ShotDirection;

    private Vector3 player1DodgeDirection;
    private Vector3 player2DodgeDirection;

    private float player1DodgeStrength;
    private float player2DodgeStrength;

    public float projectilePhaseDuration = 3f;

    private void Start()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (uiManager == null)
            uiManager = GetComponent<UIManager>();

        if (dodgeAimArrow != null)
            dodgeAimArrow.gameObject.SetActive(false);

        if (shootAimArrow != null)
            shootAimArrow.gameObject.SetActive(false);

        StartPlayerTurn(player1);
    }

    private void Update()
    {
        if (currentPhase == ActionPhase.Executing)
            return;

        if (Input.GetMouseButtonDown(0))
            StartDrag();

        if (isDragging && Input.GetMouseButton(0))
            UpdateDrag();

        if (isDragging && Input.GetMouseButtonUp(0))
            FinishDrag();
    }

    private void StartPlayerTurn(PlayerController player)
    {
        currentPlayer = player;
        currentPhase = ActionPhase.Shoot;

        if (uiManager != null)
        {
            uiManager.ShowTurn(
                player.playerNumber,
                "Choose shoot direction"
            );
        }

        Debug.Log("Player " + player.playerNumber + ": Choose shoot direction");
    }

    private void StartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (hit.transform != currentPlayer.transform &&
            !hit.transform.IsChildOf(currentPlayer.transform))
            return;

        isDragging = true;
        dragStrength = 0f;
    }

    private void UpdateDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 mousePosition = ray.GetPoint(distance);

        Vector3 drag = mousePosition - currentPlayer.transform.position;
        drag.y = 0f;

        if (drag.magnitude < 0.001f)
            return;

        dragDirection = drag.normalized;

        dragStrength = Mathf.Clamp01(
            drag.magnitude / maxDragDistance
        );

        UpdateArrow();
    }

    private void UpdateArrow()
    {
        Transform activeAimArrow =
            currentPhase == ActionPhase.Shoot ? shootAimArrow : dodgeAimArrow;

        if (activeAimArrow == null)
            return;

        if (shootAimArrow != null)
            shootAimArrow.gameObject.SetActive(currentPhase == ActionPhase.Shoot);

        if (dodgeAimArrow != null)
            dodgeAimArrow.gameObject.SetActive(currentPhase == ActionPhase.Dodge);

        activeAimArrow.position =
            currentPlayer.transform.position + Vector3.up * 0.1f;

        activeAimArrow.rotation = Quaternion.LookRotation(dragDirection);

        Vector3 scale = activeAimArrow.localScale;
        scale.z = dragStrength * maxDragDistance;
        activeAimArrow.localScale = scale;
    }

    private void FinishDrag()
    {
        isDragging = false;

        if (dodgeAimArrow != null)
            dodgeAimArrow.gameObject.SetActive(false);

        if (shootAimArrow != null)
            shootAimArrow.gameObject.SetActive(false);

        if (dragStrength < minDragDistance / maxDragDistance)
            return;

        if (currentPhase == ActionPhase.Shoot)
        {
            if (currentPlayer == player1)
                player1ShotDirection = dragDirection;
            else
                player2ShotDirection = dragDirection;

            currentPhase = ActionPhase.Dodge;

            if (uiManager != null)
            {
                uiManager.UpdateInstruction(
                    currentPlayer.playerNumber,
                    "Choose dodge direction"
                );
            }

            Debug.Log("Player " + currentPlayer.playerNumber +
                      ": Now choose dodge direction");
        }
        else if (currentPhase == ActionPhase.Dodge)
        {
            if (currentPlayer == player1)
            {
                player1DodgeDirection = dragDirection;
                player1DodgeStrength = dragStrength;

                StartPlayerTurn(player2);
            }
            else
            {
                player2DodgeDirection = dragDirection;
                player2DodgeStrength = dragStrength;

                StartCoroutine(ExecuteTurn());
            }
        }

        dragStrength = 0f;
    }

    private IEnumerator ExecuteTurn()
    {
        currentPhase = ActionPhase.Executing;

        if (uiManager != null)
            uiManager.HideTurnUI();

        Debug.Log("Both players have submitted their actions!");
        Debug.Log("Executing turn...");

        // shoot
        FireProjectile(player1, player1ShotDirection);
        FireProjectile(player2, player2ShotDirection);

        // dodge 
        player1.ExecuteDodge(
            player1DodgeDirection,
            player1DodgeStrength
        );

        player2.ExecuteDodge(
            player2DodgeDirection,
            player2DodgeStrength
        );

        yield return new WaitForSeconds(
            Mathf.Max(player1.dodgeDuration, projectilePhaseDuration)
        );

        yield return new WaitForSeconds(turnDelay);

        StartPlayerTurn(player1);
    }


    private void FireProjectile(PlayerController shooter, Vector3 direction)
    {
        Vector3 spawnPosition = shooter.transform.position +
                                direction.normalized * projectileSpawnOffset;

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        ProjectileController projectile =
            projectileObject.GetComponent<ProjectileController>();

        projectile.OnPlayerHit += (hitPlayer) =>
        {
            if (uiManager != null)
                uiManager.LoseHeart(hitPlayer.playerNumber);

            Debug.Log("Player " + hitPlayer.playerNumber + " was hit!");
        };

        projectile.Initialize(direction);
    }

}
