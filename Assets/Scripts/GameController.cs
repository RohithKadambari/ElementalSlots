using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    public int PlayerCredits = 1000;
    public int CurrentBet = 10;
    public int LastWinAmount = 0;

    [Header("Controller References")]
    public ReelController ReelManager;
    public UIController UIManager;

    // Event System for script communication
    public static event System.Action OnSpinStart;
    public static event System.Action<int> OnSpinResult;
    public static event System.Action<string> OnBonusTriggered;
    public static event System.Action<int> OnBetChanged;

    private bool spinningInProgress = false;
    private int spinsCompleted = 0;
    private int lifetimeWinnings = 0;

    void Start()
    {
        UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);
        AnalyticsManager.Instance.RecordGameStart();
    }

    void OnEnable()
    {
        // Subscribing  to UI events in UI controller script
        UIController.OnSpinButtonPressed += HandleSpinRequest;
        UIController.OnBetIncreasePressed += IncreaseBetAmount;
        UIController.OnBetDecreasePressed += DecreaseBetAmount;
    }

    void OnDisable()
    {
        // Unsubscribing of UI events ui controller
        UIController.OnSpinButtonPressed -= HandleSpinRequest;
        UIController.OnBetIncreasePressed -= IncreaseBetAmount;
        UIController.OnBetDecreasePressed -= DecreaseBetAmount;
    }

    void HandleSpinRequest()
    {
        if (!spinningInProgress && PlayerCredits >= CurrentBet)
            StartCoroutine(ExecuteSpinSequence());
    }

    IEnumerator ExecuteSpinSequence()
    {
        spinningInProgress = true;

        // Fire spin start event
        OnSpinStart?.Invoke();
        UIManager.ToggleGameButtons(false, false, false);

        PlayerCredits -= CurrentBet;
        LastWinAmount = 0;
        UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);

        spinsCompleted++;
        AnalyticsManager.Instance.RecordSpinInitiated(CurrentBet);

        yield return StartCoroutine(ReelManager.PlaySpinAnimation());
        ReelManager.GenerateNewSymbolLayout();
        yield return new WaitForSeconds(0.5f);

        string[,] currentSymbols = ReelManager.GetVisibleSymbols();

        List<Vector2Int> winningSymbolPositions;
        LastWinAmount = CalculateWinnings(currentSymbols, out winningSymbolPositions);

        if (winningSymbolPositions.Count > 0)
        {
            yield return StartCoroutine(ReelManager.HighlightWinningSymbols(winningSymbolPositions));
            lifetimeWinnings += LastWinAmount;
            UIManager.PlayWinAnimation(LastWinAmount);
        }

        PlayerCredits += LastWinAmount;
        UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);

        // Fire spin result event
        OnSpinResult?.Invoke(LastWinAmount);
        AnalyticsManager.Instance.RecordSpinOutcome(LastWinAmount, currentSymbols, CurrentBet);
        CalculateCurrentRTP();

        // Check for Free Spins triggering
        if (CountSymbolOccurrences("Scatter", currentSymbols) >= 3)
        {
            // Fire bonus triggered event
            OnBonusTriggered?.Invoke("FreeSpins");
            AnalyticsManager.Instance.RecordBonusActivation("FreeSpins");
            yield return StartCoroutine(RunFreeSpinsRound());
            AnalyticsManager.Instance.RecordBonusCompletion("FreeSpins");
        }

        spinningInProgress = false;
        UIManager.ToggleGameButtons(true, CurrentBet < 100, CurrentBet > 10);
    }

    int CalculateWinnings(string[,] symbolGrid, out List<Vector2Int> winningCells)
    {
        int totalWin = 0;
        winningCells = new List<Vector2Int>();

        // Check each row for winning combinations
        for (int rowIndex = 0; rowIndex < 4; rowIndex++)
        {
            string startingSymbol = symbolGrid[0, rowIndex];
            if (startingSymbol == "Scatter") continue;

            int consecutiveMatches = 1;
            List<Vector2Int> rowWinningCells = new List<Vector2Int> { new Vector2Int(0, rowIndex) };

            for (int reelIndex = 1; reelIndex < 5; reelIndex++)
            {
                if (symbolGrid[reelIndex, rowIndex] == startingSymbol || symbolGrid[reelIndex, rowIndex] == "Wild")
                {
                    consecutiveMatches++;
                    rowWinningCells.Add(new Vector2Int(reelIndex, rowIndex));
                }
                else
                {
                    break;
                }
            }

            if (consecutiveMatches >= 3)
            {
                int symbolPayout = GetSymbolValue(startingSymbol, consecutiveMatches);
                totalWin += symbolPayout * CurrentBet / 10;
                winningCells.AddRange(rowWinningCells);
            }
        }

        // Check for scatter wins anywhere on grid
        int scatterCount = CountSymbolOccurrences("Scatter", symbolGrid);
        if (scatterCount >= 3)
        {
            int scatterWin = GetSymbolValue("Scatter", scatterCount) * CurrentBet / 10;
            totalWin += scatterWin;

            for (int reelIndex = 0; reelIndex < 5; reelIndex++)
            {
                for (int rowIndex = 0; rowIndex < 4; rowIndex++)
                {
                    if (symbolGrid[reelIndex, rowIndex] == "Scatter")
                    {
                        winningCells.Add(new Vector2Int(reelIndex, rowIndex));
                    }
                }
            }
        }

        return totalWin;
    }

    int GetSymbolValue(string symbolType, int matchCount)
    {
        var payoutValues = new Dictionary<string, int[]>
        {
            {"Wild", new int[] {0, 0, 50, 100, 200}},
            {"Scatter", new int[] {0, 0, 5, 10, 25}},
            {"Bonus", new int[] {0, 0, 10, 25, 50}},
            {"Air", new int[] {0, 0, 15, 30, 75}},
            {"Fire", new int[] {0, 0, 15, 30, 75}},
            {"Water", new int[] {0, 0, 15, 30, 75}},
            {"Earth", new int[] {0, 0, 15, 30, 75}},
            {"Dummy1", new int[] {0, 0, 5, 15, 30}},
            {"Dummy2", new int[] {0, 0, 5, 15, 30}},
            {"Dummy3", new int[] {0, 0, 5, 15, 30}},
            {"Dummy4", new int[] {0, 0, 5, 15, 30}},
            {"Dummy5", new int[] {0, 0, 5, 15, 30}},
            {"Dummy6", new int[] {0, 0, 5, 15, 30}}
        };

        return payoutValues.ContainsKey(symbolType) ? payoutValues[symbolType][matchCount] : 0;
    }

    IEnumerator RunFreeSpinsRound()
    {
        for (int spin = 0; spin < 3; spin++)
        {
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(ReelManager.PlaySpinAnimation());
            ReelManager.GenerateNewSymbolLayout();
            yield return new WaitForSeconds(0.5f);

            string[,] freeSpinSymbols = ReelManager.GetVisibleSymbols();
            List<Vector2Int> freeSpinWins;
            int freeSpinWin = CalculateWinnings(freeSpinSymbols, out freeSpinWins);

            if (freeSpinWins.Count > 0)
            {
                yield return StartCoroutine(ReelManager.HighlightWinningSymbols(freeSpinWins));
                lifetimeWinnings += freeSpinWin;
                UIManager.PlayWinAnimation(freeSpinWin);
            }

            PlayerCredits += freeSpinWin;
            LastWinAmount = freeSpinWin;
            UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);
        }
    }

    int CountSymbolOccurrences(string targetSymbol, string[,] symbolGrid)
    {
        int occurrenceCount = 0;
        for (int reelIndex = 0; reelIndex < 5; reelIndex++)
            for (int rowIndex = 0; rowIndex < 4; rowIndex++)
                if (symbolGrid[reelIndex, rowIndex] == targetSymbol)
                    occurrenceCount++;
        return occurrenceCount;
    }

    void IncreaseBetAmount()
    {
        if (CurrentBet < 100)
        {
            CurrentBet += 10;
            UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);

            // Fire bet changed event
            OnBetChanged?.Invoke(CurrentBet);
            AnalyticsManager.Instance.RecordBetChange(CurrentBet);
        }
    }

    void DecreaseBetAmount()
    {
        if (CurrentBet > 10)
        {
            CurrentBet -= 10;
            UIManager.UpdateGameUI(PlayerCredits, CurrentBet, LastWinAmount);

            // Fire bet changed event
            OnBetChanged?.Invoke(CurrentBet);
            AnalyticsManager.Instance.RecordBetChange(CurrentBet);
        }
    }

    void CalculateCurrentRTP()
    {
        if (spinsCompleted > 0)
        {
            float currentRTP = (float)lifetimeWinnings / (spinsCompleted * CurrentBet) * 100f;
            Debug.Log("Current RTP Percentage: " + currentRTP.ToString("F2") + "%");
        }
    }

    void OnApplicationQuit()
    {
        AnalyticsManager.Instance.RecordSessionConclusion();
    }
}