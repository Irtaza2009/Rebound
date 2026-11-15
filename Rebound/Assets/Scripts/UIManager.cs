using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerUI
    {
        public GameObject panel;
        public GameObject yourTurnText;
        public TMP_Text instructionText;
        public Image[] hearts;
    }

    [Header("Player UI")]
    public PlayerUI player1UI;
    public PlayerUI player2UI;
    public Sprite greyHeartSprite;
    public Sprite fullHeartSprite;

    private int player1Health;
    private int player2Health;

    private void Awake()
    {
        HideTurnUI();

        SetHealth(1, 3);
        SetHealth(2, 3);
    }

    public void ShowTurn(int playerNumber, string instruction)
    {
        HideTurnUI();

        PlayerUI ui = GetPlayerUI(playerNumber);

        if (ui == null)
            return;

        if (ui.yourTurnText != null)
            ui.yourTurnText.SetActive(true);

        if (ui.instructionText != null)
        {
            ui.instructionText.gameObject.SetActive(true);
            ui.instructionText.text = instruction;
        }
    }

    public void HideTurnUI()
    {
        HideTurnUI(player1UI);
        HideTurnUI(player2UI);
    }

    private void HideTurnUI(PlayerUI ui)
    {
        if (ui == null)
            return;

        if (ui.yourTurnText != null)
            ui.yourTurnText.SetActive(false);

        if (ui.instructionText != null)
            ui.instructionText.gameObject.SetActive(false);
    }

    public void UpdateInstruction(int playerNumber, string instruction)
    {
        PlayerUI ui = GetPlayerUI(playerNumber);

        if (ui == null)
            return;

        if (ui.instructionText != null)
        {
            ui.instructionText.text = instruction;
            ui.instructionText.gameObject.SetActive(true);
        }
    }

    public void HideAllUI()
    {
        HideTurnUI();
    }

    public void SetHealth(int playerNumber, int health)
    {
        PlayerUI ui = GetPlayerUI(playerNumber);

        if (ui == null)
            return;

        health = Mathf.Clamp(health, 0, ui.hearts.Length);

        if (playerNumber == 1)
            player1Health = health;
        else
            player2Health = health;

        for (int i = 0; i < ui.hearts.Length; i++)
        {
            if (ui.hearts[i] == null)
                continue;

            ui.hearts[i].gameObject.SetActive(true);

            if (i >= health && greyHeartSprite != null)
                ui.hearts[i].sprite = greyHeartSprite;
            else if (i < health && fullHeartSprite != null)
                ui.hearts[i].sprite = fullHeartSprite;
        }
    }

    public void LoseHeart(int playerNumber)
    {
        PlayerUI ui = GetPlayerUI(playerNumber);

        if (ui == null)
            return;

        int health = playerNumber == 1 ? player1Health : player2Health;
        SetHealth(playerNumber, health - 1);
    }

    private PlayerUI GetPlayerUI(int playerNumber)
    {
        if (playerNumber == 1)
            return player1UI;

        if (playerNumber == 2)
            return player2UI;

        return null;
    }
}
