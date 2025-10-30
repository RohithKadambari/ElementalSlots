using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    public TMP_Text analyticsDisplayText;
    private string analyticsLog = "";
    private const int MAX_DISPLAY_LINES = 6;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordGameStart()
    {
        SendAnalyticsEvent("game_start");
    }

    public void RecordSpinInitiated(int betPlaced)
    {
        SendAnalyticsEvent("spin_start", "bet", betPlaced.ToString());
    }

    public void RecordSpinOutcome(int amountWon, string[,] symbolLayout, int betPlaced)
    {
        List<string> allSymbols = new List<string>();
        for (int row = 0; row < 4; row++)
        {
            for (int reel = 0; reel < 5; reel++)
            {
                allSymbols.Add(symbolLayout[reel, row]);
            }
        }

        SendAnalyticsEvent("spin_result",
            "win", amountWon.ToString(),
            "symbols", FormatSymbolsAsJson(allSymbols));
    }

    public void RecordBonusActivation(string bonusName)
    {
        SendAnalyticsEvent("bonus_triggered", "bonus_type", bonusName);
    }

    public void RecordBonusCompletion(string bonusName)
    {
        SendAnalyticsEvent("bonus_complete", "bonus_type", bonusName);
    }

    public void RecordBetChange(int updatedBet)
    {
        SendAnalyticsEvent("bet_changed", "new_bet", updatedBet.ToString());
    }

    public void RecordSessionConclusion()
    {
        SendAnalyticsEvent("session_end");
    }

    private void SendAnalyticsEvent(string eventType, params string[] additionalData)
    {
        string jsonOutput = "{\"event\":\"" + eventType + "\"";

        for (int i = 0; i < additionalData.Length; i += 2)
        {
            if (i + 1 < additionalData.Length)
            {
                string fieldName = additionalData[i];
                string fieldValue = additionalData[i + 1];

                jsonOutput += ",\"" + fieldName + "\":" + fieldValue;
            }
        }

        jsonOutput += "}";
        UpdateAnalyticsDisplay(jsonOutput);
    }

    private string FormatSymbolsAsJson(List<string> symbols)
    {
        string jsonArray = "[";
        for (int i = 0; i < symbols.Count; i++)
        {
            jsonArray += "\"" + symbols[i] + "\"";
            if (i < symbols.Count - 1)
                jsonArray += ",";
        }
        jsonArray += "]";
        return jsonArray;
    }

    private void UpdateAnalyticsDisplay(string analyticsData)
    {
        if (analyticsDisplayText != null)
        {
            analyticsLog = analyticsData + "\n" + analyticsLog;

            string[] lines = analyticsLog.Split('\n');
            if (lines.Length > MAX_DISPLAY_LINES)
            {
                analyticsLog = string.Join("\n", lines, 0, MAX_DISPLAY_LINES);
            }

            analyticsDisplayText.text = analyticsLog;
        }
    }

    public void ClearAnalyticsDisplay()
    {
        analyticsLog = "";
        if (analyticsDisplayText != null)
        {
            analyticsDisplayText.text = "";
        }
    }
}