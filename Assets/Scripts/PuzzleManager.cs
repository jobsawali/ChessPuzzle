using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public string fen;
    public string[] solution;
    public int difficulty;
}

[System.Serializable]
public class PuzzleList
{
    public PuzzleData[] puzzles;
}

public class PuzzleManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public BoardManager boardManager;
    public UIManager uiManager;

    private List<PuzzleData> puzzles = new List<PuzzleData>();
    private PuzzleData currentPuzzle;
    private int currentMoveIndex = 0;
    private int currentPuzzleIndex = 0;
    private string pendingComputerMove;

    void Start()
    {
        LoadPuzzles();
        Invoke(nameof(LoadNextPuzzle), 0.2f);
    }

    void LoadPuzzles()
    {
        TextAsset json = Resources.Load<TextAsset>("puzzles");
        if (json == null)
        {
            Debug.LogError("File puzzles.json non trovato in Assets/Resources!");
            return;
        }

        string wrappedJson = "{\"puzzles\":" + json.text + "}";
        PuzzleList data = JsonUtility.FromJson<PuzzleList>(wrappedJson);
        puzzles.AddRange(data.puzzles);

        puzzles.Sort((a, b) => a.difficulty.CompareTo(b.difficulty));
    }

    public void LoadNextPuzzle()
    {
        if (puzzles.Count == 0) return;

        int windowSize = 3;
        int maxIndex = Mathf.Min(currentPuzzleIndex + windowSize, puzzles.Count - 1);
        int randomIndex = Random.Range(currentPuzzleIndex, maxIndex + 1);

        currentPuzzle = puzzles[randomIndex];
        Debug.Log("ID: " + currentPuzzle.id + " FEN: " + currentPuzzle.fen);
        currentPuzzleIndex = randomIndex + 1;
        currentMoveIndex = 0;

       
        bool computerIsWhite = currentPuzzle.fen.Contains(" w ");
        bool playerIsBlack = computerIsWhite;

      
        boardManager.blackAtBottom = playerIsBlack;

        
        boardManager.LoadPosition(currentPuzzle.fen);

       
        if (uiManager != null)
            uiManager.UpdateWhoToMove(!playerIsBlack);

        
        if (currentPuzzle.solution.Length > 0)
            Invoke(nameof(ExecuteInitialMove), 0.5f);
    }

    void ExecuteInitialMove()
    {
        string m = currentPuzzle.solution[0];
        currentMoveIndex = 1;
        boardManager.ClearHighlights();
        boardManager.HighlightMove(m, new Color(1f, 0.9f, 0f, 0.6f));
        boardManager.ExecuteMove(m, true);
    }

    public void TryMove(string uci)
    {
        if (currentPuzzle == null) return;

        if (uci == currentPuzzle.solution[currentMoveIndex])
        {
            boardManager.ClearHighlights();
            boardManager.HighlightMove(uci, new Color(0f, 1f, 0f, 0.6f));
            boardManager.ExecuteMove(uci);
            currentMoveIndex++;

            if (currentMoveIndex < currentPuzzle.solution.Length)
            {
                pendingComputerMove = currentPuzzle.solution[currentMoveIndex];
                currentMoveIndex++;
                Invoke(nameof(ExecuteResponseMove), 0.6f);
            }
            else
            {
                if (uiManager != null)
                {
                    uiManager.PuzzleSolved();
                    uiManager.ShowPuzzleCorrect();
                    uiManager.AddHistoryItem(true, currentPuzzle.difficulty);
                }
                Invoke(nameof(LoadNextPuzzle), 1.2f);
            }
        }
        else
        {
            boardManager.ClearHighlights();
            boardManager.ExecuteMove(uci);
            boardManager.HighlightMove(uci, new Color(1f, 0f, 0f, 0.6f));

            if (uiManager != null)
            {
                uiManager.LoseLife();
                uiManager.ShowPuzzleIncorrect();
                uiManager.AddHistoryItem(false, currentPuzzle.difficulty);
            }

            if (uiManager != null && uiManager.IsGameOver())
                return;

            Invoke(nameof(LoadNextPuzzle), 1.2f);
        }
    }

    void ExecuteResponseMove()
    {
        boardManager.ClearHighlights();
        boardManager.HighlightMove(pendingComputerMove, new Color(1f, 0.9f, 0f, 0.6f));
        boardManager.ExecuteMove(pendingComputerMove, true);

        if (currentMoveIndex >= currentPuzzle.solution.Length)
        {
            Invoke(nameof(LoadNextPuzzle), 1.2f);
        }
    }
}