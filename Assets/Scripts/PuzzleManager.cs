using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class PuzzleData
{
    public string id;
    public string fen;
    public string[] solution;
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
    }

 
    public void LoadNextPuzzle()
    {
        if (puzzles.Count == 0) return;

        currentPuzzle = puzzles[currentPuzzleIndex % puzzles.Count];
        currentPuzzleIndex++;
        currentMoveIndex = 0;

        bool playerIsBlack = currentPuzzle.fen.Contains(" w ") && currentPuzzle.solution.Length > 0;
        boardManager.blackAtBottom = playerIsBlack;
        boardManager.LoadPosition(currentPuzzle.fen);

        if (currentPuzzle.solution.Length > 0)
        {
            Invoke(nameof(ExecuteInitialMove), 0.5f);
        }
    }

    void ExecuteInitialMove()
    {
        string m = currentPuzzle.solution[0];
        currentMoveIndex = 1;
        boardManager.ClearHighlights();
        boardManager.HighlightMove(m, new Color(1f, 0.9f, 0f, 0.6f));
        boardManager.ExecuteMove(m,true);
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
                Invoke(nameof(LoadNextPuzzle), 1.2f);
            }
        }
        else
        {
            
            boardManager.ClearHighlights();
            boardManager.ExecuteMove(uci);
            boardManager.HighlightMove(uci, new Color(1f, 0f, 0f, 0.6f));
            Invoke(nameof(ResetPuzzleAfterError), 0.8f);
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

    void ResetPuzzleAfterError()
    {
        boardManager.LoadPosition(currentPuzzle.fen);
        Invoke(nameof(ExecuteInitialMove), 0.3f);
    }
}