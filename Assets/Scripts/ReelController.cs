using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    public Sprite[] Symbols;
    public string[] SymbolNames;

    private Image[,] symbolDisplays = new Image[5, 4];
    private string[,] currentSymbols = new string[5, 4];

    void Start()
    {
        SymbolNames = new string[]
        {
            "Air", "Fire", "Water", "Earth",
            "Wild", "Scatter", "Bonus",
            "Dummy1", "Dummy2", "Dummy3", "Dummy4", "Dummy5", "Dummy6"
        };

        if (Symbols.Length != SymbolNames.Length)
        {
            Debug.LogError($"Symbols array length ({Symbols.Length}) doesn't match SymbolNames length ({SymbolNames.Length})!");
        }

        InitializeReelComponents();
        GenerateNewSymbolLayout();
    }

    void InitializeReelComponents()
    {
        for (int reel = 0; reel < 5; reel++)
        {
            Transform reelParent = transform.Find("Reel" + (reel + 1));
            if (reelParent == null)
            {
                Debug.LogError($"Reel{reel + 1} not found in hierarchy!");
                continue;
            }

            for (int row = 0; row < 4; row++)
            {
                Transform symbolSlot = reelParent.Find("Symbol" + (row + 1));
                if (symbolSlot == null)
                {
                    Debug.LogError($"Symbol{row + 1} not found in Reel{reel + 1}!");
                    continue;
                }

                symbolDisplays[reel, row] = symbolSlot.GetComponent<Image>();
                if (symbolDisplays[reel, row] == null)
                {
                    Debug.LogError($"Image component missing on Symbol{row + 1} in Reel{reel + 1}!");
                    continue;
                }

                symbolDisplays[reel, row].color = Color.white;
            }
        }
    }

    public void GenerateNewSymbolLayout()
    {
        for (int reel = 0; reel < 5; reel++)
        {
            for (int row = 0; row < 4; row++)
            {
                string symbolName = GetWeightedRandomSymbol();
                int symbolIndex = System.Array.IndexOf(SymbolNames, symbolName);

                if (symbolIndex >= 0 && symbolIndex < Symbols.Length)
                {
                    symbolDisplays[reel, row].sprite = Symbols[symbolIndex];
                    currentSymbols[reel, row] = symbolName;
                }
                else
                {
                    Debug.LogWarning($"Invalid symbol index {symbolIndex} for symbol '{symbolName}'. Using fallback.");
                    symbolDisplays[reel, row].sprite = Symbols[0];
                    currentSymbols[reel, row] = SymbolNames[0];
                }
            }
        }
    }

    private string GetWeightedRandomSymbol()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 0.5f) return "Wild";
        if (randomValue < 2.0f) return "Scatter";
        if (randomValue < 4.0f) return "Bonus";
        if (randomValue < 8.0f) return "Air";
        if (randomValue < 12.0f) return "Fire";
        if (randomValue < 16.0f) return "Water";
        if (randomValue < 20.0f) return "Earth";
        if (randomValue < 30.0f) return "Dummy1";
        if (randomValue < 40.0f) return "Dummy2";
        if (randomValue < 50.0f) return "Dummy3";
        if (randomValue < 60.0f) return "Dummy4";
        if (randomValue < 75.0f) return "Dummy5";
        return "Dummy6";
    }

    public string[,] GetVisibleSymbols()
    {
        return currentSymbols;
    }

    public IEnumerator PlaySpinAnimation()
    {
        float animationTime = 0f;
        while (animationTime < 1.5f)
        {
            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 4; row++)
                {
                    if (Symbols.Length > 0)
                    {
                        int randomIndex = Random.Range(0, Symbols.Length);
                        symbolDisplays[reel, row].sprite = Symbols[randomIndex];
                    }
                }
            }
            animationTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator HighlightWinningSymbols(List<Vector2Int> winningCells)
    {
        foreach (Vector2Int cell in winningCells)
        {
            if (cell.x >= 0 && cell.x < 5 && cell.y >= 0 && cell.y < 4)
            {
                Image winningImage = symbolDisplays[cell.x, cell.y];
                Transform symbolTransform = winningImage.transform;

                float growTime = 0.2f;
                float elapsed = 0f;

                while (elapsed < growTime)
                {
                    float scaleValue = Mathf.Lerp(1f, 1.3f, elapsed / growTime);
                    symbolTransform.localScale = new Vector3(scaleValue, scaleValue, 1f);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                symbolTransform.localScale = new Vector3(1.3f, 1.3f, 1f);
                yield return new WaitForSeconds(0.1f);

                elapsed = 0f;
                while (elapsed < growTime)
                {
                    float scaleValue = Mathf.Lerp(1.3f, 1f, elapsed / growTime);
                    symbolTransform.localScale = new Vector3(scaleValue, scaleValue, 1f);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                symbolTransform.localScale = Vector3.one;
            }
        }
    }
}