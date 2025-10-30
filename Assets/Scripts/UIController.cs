using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text creditsDisplay;
    public TMP_Text betDisplay;
    public TMP_Text winDisplay;
    public Button spinButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;

    public static event Action OnSpinButtonPressed;
    public static event Action OnBetIncreasePressed;
    public static event Action OnBetDecreasePressed;

    void Start()
    {
        spinButton.onClick.AddListener(() => OnSpinButtonPressed?.Invoke());
        increaseBetButton.onClick.AddListener(() => OnBetIncreasePressed?.Invoke());
        decreaseBetButton.onClick.AddListener(() => OnBetDecreasePressed?.Invoke());
    }

    public void UpdateGameUI(int credits, int bet, int win)
    {
        creditsDisplay.text = "CREDITS: " + credits;
        betDisplay.text = "BET: " + bet;
        winDisplay.text = "WIN: " + win;
    }

    public void ToggleGameButtons(bool spinActive, bool betUpActive, bool betDownActive)
    {
        spinButton.interactable = spinActive;
        increaseBetButton.interactable = betUpActive;
        decreaseBetButton.interactable = betDownActive;
    }

    public void PlayWinAnimation(int winAmount)
    {
        StartCoroutine(AnimateWinDisplay());
    }

    private System.Collections.IEnumerator AnimateWinDisplay()
    {
        for (int i = 0; i < 6; i++)
        {
            winDisplay.color = winDisplay.color == Color.yellow ? Color.white : Color.yellow;
            yield return new WaitForSeconds(0.2f);
        }
        winDisplay.color = Color.white;
    }
}